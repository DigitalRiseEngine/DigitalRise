// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRise.Mathematics;


namespace DigitalRise.Geometry.Meshes
{
  public partial class DcelMesh
  {
    /// <overloads>
    /// <summary>
    /// Converts the given <see cref="ITriangleMesh"/> to a <see cref="DcelMesh"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the given <see cref="ITriangleMesh"/> to a <see cref="DcelMesh"/>.
    /// </summary>
    /// <param name="mesh">The triangle mesh.</param>
    /// <returns>
    /// The <see cref="DcelMesh"/>.
    /// </returns>
    /// <remarks>
    /// Currently, creating <see cref="DcelMesh"/>es is not supported if the triangle mesh consists
    /// of unconnected sub-meshes or unconnected triangles. All parts of the triangle mesh must be
    /// connected via an edge. (But it is not required that the mesh is closed.)
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="mesh"/> has no vertices or vertex indices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="mesh"/> consists of several unconnected components or sub-meshes.
    /// </exception>
    public static DcelMesh FromTriangleMesh(ITriangleMesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      // The simple way: Create a TriangleMesh first.
      var triangleMesh = new TriangleMesh();
      triangleMesh.Add(mesh, false);
      triangleMesh.WeldVertices();

      return FromTriangleMesh(triangleMesh);
    }


    /// <summary>
    /// Converts the given <see cref="ITriangleMesh"/> to a <see cref="DcelMesh"/>.
    /// </summary>
    /// <param name="mesh">The triangle mesh.</param>
    /// <returns>
    /// The <see cref="DcelMesh"/>.
    /// </returns>
    /// <remarks>
    /// Currently, creating <see cref="DcelMesh"/>es is not supported if the triangle mesh consists
    /// of unconnected sub-meshes or unconnected triangles. All parts of the triangle mesh must be
    /// connected via an edge. (But it is not required that the mesh is closed.)
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="mesh"/> has no vertices or vertex indices.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="mesh"/> consists of several unconnected components or sub-meshes.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public static DcelMesh FromTriangleMesh(TriangleMesh mesh)
    {
      // TODO: To optimize, check tricks of TriangleMeshShape.ComputeNeighbors.

      if (mesh == null)
        throw new ArgumentNullException("mesh");
      if (mesh.Vertices == null || mesh.Indices == null)
        throw new ArgumentException("Input mesh has no vertices or vertex indices.");

      // Create vertices.
      int numberOfVertices = mesh.Vertices.Count;
      var vertices = new List<DcelVertex>(numberOfVertices);
      foreach (var position in mesh.Vertices)
        vertices.Add(new DcelVertex(position, null));

      // Weld similar vertices.
      for (int i = 0; i < numberOfVertices; i++)
        for (int j = i + 1; j < numberOfVertices; j++)
          if (MathHelper.AreNumericallyEqual(vertices[i].Position, vertices[j].Position))
            vertices[i] = vertices[j];

      // Create edges and faces for each triangle.
      // We need at least 3 edges for each triangle. We might need more edges if we have to 
      // connect unconnected islands of triangles.
      var edges = new List<DcelEdge>(mesh.NumberOfTriangles * 3 * 2);
      var faces = new List<DcelFace>(mesh.NumberOfTriangles);
      for (int i = 0; i < mesh.NumberOfTriangles; i++)
      {
        // Get triangle indices.
        var index0 = mesh.Indices[i * 3 + 0];
        var index1 = mesh.Indices[i * 3 + 1];
        var index2 = mesh.Indices[i * 3 + 2];

        // Get DCEL vertices.
        var vertex0 = vertices[index0];
        var vertex1 = vertices[index1];
        var vertex2 = vertices[index2];

        // Create 3 edges.
        var edge0 = new DcelEdge();
        var edge1 = new DcelEdge();
        var edge2 = new DcelEdge();

        // Create 1 face.
        var face = new DcelFace();

        // Fill out face info.
        face.Boundary = edge0;

        // Fill out vertex info.
        vertex0.Edge = edge0;
        vertex1.Edge = edge1;
        vertex2.Edge = edge2;

        // Fill out edge info.
        // Twin links are created later.
        edge0.Face = face;
        edge0.Origin = vertex0;
        edge0.Next = edge1;
        edge0.Previous = edge2;

        edge1.Face = face;
        edge1.Origin = vertex1;
        edge1.Next = edge2;
        edge1.Previous = edge0;

        edge2.Face = face;
        edge2.Origin = vertex2;
        edge2.Next = edge0;
        edge2.Previous = edge1;

        // Add to lists.
        edges.Add(edge0);
        edges.Add(edge1);
        edges.Add(edge2);
        faces.Add(face);
      }

      // Connect triangles that share an edge.
      for (int i = 0; i < faces.Count; i++)
      {
        // Get face and its 3 edges.
        var faceI = faces[i];
        var edgeI0 = faceI.Boundary;
        var edgeI1 = edgeI0.Next;
        var edgeI2 = edgeI1.Next;
        Debug.Assert(edgeI2.Next == edgeI0);

        for (int j = i + 1; j < faces.Count; j++)
        {
          // Get face and its 3 edges.
          var faceJ = faces[j];
          var edgeJ0 = faceJ.Boundary;
          var edgeJ1 = edgeJ0.Next;
          var edgeJ2 = edgeJ1.Next;
          Debug.Assert(edgeJ2.Next == edgeJ0);

          TryLink(edgeI0, edgeJ0);
          TryLink(edgeI0, edgeJ1);
          TryLink(edgeI0, edgeJ2);

          TryLink(edgeI1, edgeJ0);
          TryLink(edgeI1, edgeJ1);
          TryLink(edgeI1, edgeJ2);

          TryLink(edgeI2, edgeJ0);
          TryLink(edgeI2, edgeJ1);
          TryLink(edgeI2, edgeJ2);
        }
      }

      // If the mesh is not closed, we have to add twin edges at the boundaries
      foreach (var edge in edges.ToArray())
      {
        if (edge.Twin == null)
        {
          var twin = new DcelEdge();
          twin.Origin = edge.Next.Origin;
          twin.Twin = edge;
          edge.Twin = twin;
          edges.Add(twin);
        }
      }

      // Yet, twin.Next/Previous were not set.
      foreach (var edge in edges)
      {
        if (edge.Previous == null)
        {
          // The previous edge has not been set.
          // Search the edges around the origin until we find the previous unconnected edge.
          var origin = edge.Origin;
          var originEdge = edge.Twin.Next;  // Another edge with the same origin.
          while (originEdge.Twin.Next != null)
          {
            Debug.Assert(originEdge.Origin == origin);
            originEdge = originEdge.Twin.Next;
          }

          var previous = originEdge.Twin;
          previous.Next = edge;
          edge.Previous = previous;
        }
      }

      // Check if we have one connected mesh.
      if (vertices.Count > 0)
      {
        const int Tag = 1;
        TagLinkedComponents(vertices[0], Tag);

        // Check if all components were reached.
        if (vertices.Any(v => v.Tag != Tag)
            || edges.Any(e => e.Tag != Tag)
            || faces.Any(f => f.Tag != Tag))
        {
          throw new NotSupportedException("The triangle mesh consists of several unconnected components or sub-meshes.");
        }
      }

      var dcelMesh = new DcelMesh { Vertex = vertices.FirstOrDefault() };
      dcelMesh.ResetTags();

      return dcelMesh;
    }


    // Links to half-edges via Twin links if they are on the same edge (same vertices).
    private static void TryLink(DcelEdge edge0, DcelEdge edge1)
    {
      var start0 = edge0.Origin;
      var end0 = edge0.Next.Origin;

      var start1 = edge1.Origin;
      var end1 = edge1.Next.Origin;

      Debug.Assert(edge0 != edge1 && !(start0 == start1 && end0 == end1), "Two edges are identical.");

      if (start0 == end1 && end0 == start1)
      {
        edge0.Twin = edge1;
        edge1.Twin = edge0;
      }
    }


    /// <summary>
    /// Converts this mesh to a <see cref="TriangleMesh"/>.
    /// </summary>
    /// <returns>A triangle mesh that represents the same mesh.</returns>
    /// <remarks>
    /// This mesh must consist only of faces that are convex polygons (triangles, quads, etc.).
    /// Concave faces will not be triangulated correctly.
    /// </remarks>
    /// <exception cref="GeometryException">The DCEL mesh is invalid.</exception>
    public TriangleMesh ToTriangleMesh()
    {
      TriangleMesh triMesh = new TriangleMesh();

      UpdateCache();

      int numberOfVertices = Vertices.Count;
      int numberOfFaces = Faces.Count;

      for (int i = 0; i < numberOfVertices; i++)
      {
        DcelVertex vertex = Vertices[i];

        // Store the vertex indices in the InternalTags of the vertices.
        vertex.InternalTag = i;

        // Add all vertices to the triangle mesh.
        triMesh.Vertices.Add(vertex.Position);
      }

      for (int i = 0; i < numberOfFaces; i++)
      {
        var face = Faces[i];
        var startEdge = face.Boundary;
        var nextEdge = startEdge.Next;

        // Tag edges to see if they have been visited.
        startEdge.InternalTag = 1;
        nextEdge.InternalTag = 1;

        // Triangulate polygon.
        var v0 = startEdge.Origin;
        var v1 = nextEdge.Origin;
        nextEdge = nextEdge.Next;
        while (nextEdge != startEdge)
        {
          if (nextEdge == null || nextEdge.InternalTag == 1)
            throw new GeometryException("DCEL mesh is invalid.");

          nextEdge.InternalTag = 1;
          var v2 = nextEdge.Origin;
          
          // The tags of the vertices store their index number.
          triMesh.Indices.Add(v0.InternalTag);
          triMesh.Indices.Add(v1.InternalTag);
          triMesh.Indices.Add(v2.InternalTag);

          v1 = v2;
          nextEdge = nextEdge.Next;
        }
      }

      ResetInternalTags();

      return triMesh;
    }
  }
}
