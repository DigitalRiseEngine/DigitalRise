// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using System.Linq;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using Plane = DigitalRise.Geometry.Shapes.Plane;

namespace DigitalRise.Geometry.Meshes
{
  public partial class DcelMesh
  {
    // TODO: Add error messages to IsXxx method, so that users see where the problem is.

    /// <summary>
    /// Gets a value indicating whether the tags of all components in the mesh are equal to the
    /// given tag value.
    /// </summary>
    /// <param name="tag">The reference tag value.</param>
    /// <returns>
    /// <see langword="true"/> if the tags of all components are equal to <paramref name="tag"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool AreTagsEqualTo(int tag)
    {
      return Vertices.All(vertex => vertex.Tag == tag)
             && Edges.All(edge => edge.Tag == tag)
             && Faces.All(face => face.Tag == tag);
    }


    /// <summary>
    /// Determines whether the specified point is contained in the mesh. (This method assumes that
    /// the mesh is a convex polyhedron.)
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="epsilon">
    /// The epsilon tolerance. A point counts as "contained" if the distance to the mesh surface is
    /// less than this value. Use a small positive value, e.g. 0.001f, for numerical robustness.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified point is contained; otherwise, 
    /// <see langword="false"/>. (The result is undefined if the mesh is not a convex polyhedron.)
    /// </returns>
    public bool Contains(Vector3 point, float epsilon)
    {
      foreach (var face in Faces)
      {
        // Get normal vector.
        var normal = face.Normal;

        // Skip degenerate faces.
        float normalLength = normal.Length();
        if (Numeric.IsZero(normalLength))
          continue;

        // Normalize.
        normal = normal / normalLength;

        // Create a plane for the face.
        Plane plane = new Plane(normal, face.Boundary.Origin.Position);

        // Get distance from plane.
        float d = Vector3.Dot(point, plane.Normal) - plane.DistanceFromOrigin;

        // Check distance with epsilon tolerance.
        if (d > epsilon * (1 + normalLength + (point - face.Boundary.Origin.Position).Length()))
          return false;
      }
      return true;
    }


    /// <summary>
    /// Determines whether this mesh is a closed mesh.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this instance is a closed mesh; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// In a closed mesh all edges are connected to 2 faces. For example: The mesh of a sphere is a 
    /// closed mesh. An object with holes, like a pullover, is not closed. A flat object, like a 
    /// curtain, is closed if both sides are modeled with faces. It is not closed if only a one side 
    /// is modeled with faces. (A single vertex is considered as closed. A single edge-pair is also
    /// considered as closed.)
    /// </para>
    /// <para>
    /// This method does not check whether the mesh <see cref="IsValid()"/>.
    /// </para>
    /// </remarks>
    public bool IsClosed()
    {
      // Empty mesh is not closed.
      if (Vertex == null)
        return false;

      // A single edge-pair is closed.
      if (Faces.Count == 0 && Edges.Count <= 2)
        return true;

      return Faces.All(f => f.Holes == null)            // No holes allowed.
             && Edges.All(e => e.Face != null           // Faces on both sides of the edge.
                               && e.Twin != null 
                               && e.Twin.Face != null);
    }


    /// <summary>
    /// Determines whether the mesh is a valid triangle mesh.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the mesh is a valid triangle mesh; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks if the mesh consists only of triangle faces without edges or vertices 
    /// that are not connected to a face.
    /// </para>
    /// <para>
    /// This method does not check whether the mesh <see cref="IsValid()"/>.
    /// </para>
    /// </remarks>
    public bool IsTriangleMesh()
    {
      if (Vertices.Count > 0 && Faces.Count == 0)
        return false;

      return Faces.All(f => f.Holes == null)                // No holes allowed.
             && Edges.All(e => e.Face == null 
                               || e.Next != null               // Face boundary consists of exactly 3 edges.
                                  && e.Next.Next == e.Previous
                                  && e.Next != e.Previous);
    }


    /// <summary>
    /// Determines whether the mesh is a planar, two-sided polygon.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the mesh is a planar, two-sided polygon; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method does not check whether the mesh <see cref="IsValid()"/>.
    /// </remarks>
    internal bool IsTwoSidedPolygon()
    {
      return Faces.Count == 2
             && Edges.Count == Vertices.Count * 2
             && IsClosed();
    }


    /// <overloads>
    /// <summary>
    /// Determines whether this instance is a valid mesh.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether this instance is a valid mesh.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks if all components are linked properly. Problems can be: 
    /// <list type="bullet">
    /// <item>Missing components, e.g. a face is not entirely bound by edges.</item>
    /// <item>Wrong links, e.g. edge A has a twin B and the twin of B is not set to A.</item>
    /// <item>...</item>
    /// </list>
    /// </remarks>
    public bool IsValid()
    {
      string dummy;
      return IsValid(out dummy);
    }


    /// <summary>
    /// Determines whether this instance is a valid mesh and returns an error description.
    /// </summary>
    /// <param name="errorDescription">
    /// The error description or <see langword="null"/> if the mesh is valid.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method checks if all components are linked properly. Problems can be:
    /// <list type="bullet">
    /// <item>Missing components, e.g. a face is not entirely bound by edges.</item>
    /// <item>Wrong links, e.g. edge A has a twin B and the twin of B is not set to A.</item>
    /// <item>...</item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public bool IsValid(out string errorDescription)
    {
      foreach (var face in Faces)
      {
        if (!IsValid(face, out errorDescription))
          return false;
      }

      foreach (var edge in Edges)
      {
        if (edge.Twin == null)
        {
          errorDescription = "Edge has no twin edge.";
          return false;
        }

        if (edge.Twin.Twin != edge)
        {
          errorDescription = "Twin of an edge has a different twin.";
          return false;
        }

        if (edge.Twin.Face != null && edge.Twin.Face == edge.Face)
        {
          errorDescription = "edge.Twin links to the same face as the edge.";
          return false;
        }

        if (edge.Twin.Next != null && edge.Twin.Next.Origin != edge.Origin)
        {
          errorDescription = "edge.Origin is different from edge.Twin.Next.Origin.";
          return false;
        }

        if (edge.Next != null && edge.Next.Previous != edge)
        {
          errorDescription = "edge.Next.Previous is not equal to edge.";
          return false;
        }

        if (edge.Origin == null)
        {
          errorDescription = "Edge.Origin is null.";
          return false;
        }
      }

      foreach (var vertex in Vertices)
      {
        if (vertex != Vertex && vertex.Edge == null)
        {
          errorDescription = "Vertex.Edge is null.";
          return false;
        }

        if (vertex != Vertex && vertex.Edge.Origin != vertex)
        {
          errorDescription = "vertex.Edge.Origin is different from vertex.";
          return false;
        }
      }

      errorDescription = null;
      return true;
    }


    // Checks if the face has a boundary and if all boundary edges are set to this face.
    private bool IsValid(DcelFace face, out string errorDescription)
    {
      // TODO: Do we need anything special for holes?

      Debug.Assert(face != null);

      var boundary = face.Boundary;
      if (boundary == null)
      {
        errorDescription = "Face.Boundary is null.";
        return false;
      }

      if (boundary.Face != face)
      {
        errorDescription = "A boundary edge of a face links to another face.";
        return false;
      }

      if (boundary.Next == boundary)
      {
        errorDescription = "Edge.Next links to itself.";
        return false;
      }

      try
      {
        // Check if boundary is closed and if all edges are linked to the face.
        boundary.InternalTag = 1;
        var edge = boundary.Next;
        while (edge != boundary && (edge == null || edge.InternalTag != 1))
        {
          if (edge == null)
          {
            errorDescription = "Edge.Next is not set for a face boundary edge.";
            return false;
          }

          if (edge.Face != face)
          {
            errorDescription = "A boundary edge of a face links to another face.";
            return false;
          }

          edge.InternalTag = 1;
          edge = edge.Next;
        }

        if (edge != boundary)
        {
          errorDescription = "Face has stray boundary edges.";
          return false;
        }
      }
      finally
      {
        ResetInternalTags();
      }

      errorDescription = null;
      return true;
    }
  }
}
