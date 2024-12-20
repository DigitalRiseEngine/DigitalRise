// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace DigitalRise.Geometry.Meshes
{
  /// <summary>
  /// A mesh represented by a Doubly-Connected Edge List (DCEL).
  /// </summary>
  /// <remarks>
  /// <para>
  /// This implementation follows the description of Doubly-Connected Edge Lists in the book
  /// "Computational Geometry", de Berg et al.
  /// </para>
  /// <para>
  /// This class exposes one vertex of the mesh. Other mesh components can be found by 
  /// navigating the DCEL data structures beginning at <see cref="Vertex"/>.
  /// </para>
  /// <para>
  /// For convenience, component lists (<see cref="Vertices"/>, <see cref="Edges"/> and 
  /// <see cref="Faces"/>) are automatically generated when required. Whenever the mesh is modified 
  /// (components are added or removed), the flag <see cref="Dirty"/> must be set. Then the 
  /// component list are automatically recreated the next time they are accessed.
  /// </para>
  /// <para>
  /// <strong>Handling Tags:</strong> The DCEL data structures (<see cref="DcelVertex"/>, 
  /// <see cref="DcelEdge"/> and <see cref="DcelFace"/>) have "tags" (integer properties) which can 
  /// be used to mark components when navigating or modifying the mesh. This tags can be used by 
  /// different operations that act on the mesh.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public partial class DcelMesh
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="DcelMesh"/> is dirty.
    /// </summary>
    /// <value><see langword="true"/> if dirty; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This property must be set when the mesh is modified. 
    /// </remarks>
    public bool Dirty
    {
      get { return _dirty; }
      set { _dirty = value; }
    }
    private bool _dirty = true;


    /// <summary>
    /// Gets the edges (as a read-only list).
    /// </summary>
    /// <value>The edges.</value>
    /// <remarks>
    /// This list is automatically re-generated when the mesh is new or <see cref="Dirty"/>.
    /// </remarks>
    public ReadOnlyCollection<DcelEdge> Edges
    {
      get 
      {
        UpdateCache();

        if (_edgesReadOnly == null)
          _edgesReadOnly = new ReadOnlyCollection<DcelEdge>(_edges);

        return _edgesReadOnly;
      }
    }
    private List<DcelEdge> _edges;
    private ReadOnlyCollection<DcelEdge> _edgesReadOnly;


    /// <summary>
    /// Gets the faces (as a read-only list).
    /// </summary>
    /// <value>The faces.</value>
    /// <remarks>
    /// This list is automatically re-generated when the mesh is new or <see cref="Dirty"/>.
    /// </remarks>
    public ReadOnlyCollection<DcelFace> Faces
    {
      get 
      {
        UpdateCache();

        if (_facesReadOnly == null)
          _facesReadOnly = new ReadOnlyCollection<DcelFace>(_faces);

        return _facesReadOnly;
      }
    }
    private List<DcelFace> _faces;
    private ReadOnlyCollection<DcelFace> _facesReadOnly;


    /// <summary>
    /// Gets or sets the vertex.
    /// </summary>
    /// <value>The vertex.</value>
    public DcelVertex Vertex { get; set; }


    /// <summary>
    /// Gets the vertices (as a read-only list).
    /// </summary>
    /// <value>The vertices.</value>
    /// <remarks>
    /// This list is automatically re-generated when the mesh is new or <see cref="Dirty"/>.
    /// </remarks>
    public ReadOnlyCollection<DcelVertex> Vertices
    {
      get 
      {
        UpdateCache();
        
        if (_verticesReadOnly == null)
          _verticesReadOnly = new ReadOnlyCollection<DcelVertex>(_vertices);

        return _verticesReadOnly;
      }
    }
    private List<DcelVertex> _vertices;
    private ReadOnlyCollection<DcelVertex> _verticesReadOnly;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DcelMesh"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new empty instance of the <see cref="DcelMesh"/> class.
    /// </summary>
    public DcelMesh()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DcelMesh" /> class that is a copy of the given 
    /// mesh.
    /// </summary>
    /// <param name="mesh">
    /// The source mesh to copy. Can be <see langword="null"/> to create an empty 
    /// <see cref="DcelMesh"/>.
    /// </param>
    /// <remarks>
    /// This constructor creates a new <see cref="DcelMesh"/> which is a copy of the given 
    /// <paramref name="mesh"/>. 
    /// </remarks>
    public DcelMesh(DcelMesh mesh)
    {
      if (mesh == null)
        return;

      var originalVertices = mesh.Vertices;
      var originalEdges = mesh.Edges;
      var originalFaces = mesh.Faces;

      // 3 empty lists.
      _vertices = new List<DcelVertex>(originalVertices.Count);
      _edges = new List<DcelEdge>(originalEdges.Count);
      _faces = new List<DcelFace>(originalFaces.Count);
      
      // Use the internal tags to store list indices.
      for (int i = 0; i < originalVertices.Count; i++)
        originalVertices[i].InternalTag = i;

      for (int i = 0; i < originalEdges.Count; i++)
        originalEdges[i].InternalTag = i;

      for (int i = 0; i < originalFaces.Count; i++)
        originalFaces[i].InternalTag = i;

      // Add empty instances to the new lists.
      for (int i = 0; i < originalVertices.Count; i++)
        _vertices.Add(new DcelVertex());

      for (int i = 0; i < originalEdges.Count; i++)
        _edges.Add(new DcelEdge());

      for (int i = 0; i < originalFaces.Count; i++)
        _faces.Add(new DcelFace());

      // Clone the DCEL parts.
      for (int i = 0; i < originalVertices.Count; i++)
      {
        var s = originalVertices[i];  // source
        var t = _vertices[i];         // target
        
        t.Position = s.Position;

        if (s.Edge != null)
          t.Edge = _edges[s.Edge.InternalTag];

        t.Tag = s.Tag;
        t.UserData = s.UserData;
      }

      for (int i = 0; i < originalEdges.Count; i++)
      {
        var s = originalEdges[i];
        var t = _edges[i];

        if (s.Origin != null)
          t.Origin = _vertices[s.Origin.InternalTag];

        if (s.Twin != null)
          t.Twin = _edges[s.Twin.InternalTag];

        if (s.Face != null)
          t.Face = _faces[s.Face.InternalTag];

        if (s.Next != null)
          t.Next = _edges[s.Next.InternalTag];

        if (s.Previous != null)
          t.Previous = _edges[s.Previous.InternalTag];

        t.Tag = s.Tag;
        t.UserData = s.UserData;
      }

      for (int i = 0; i < originalFaces.Count; i++)
      {
        var s = originalFaces[i];
        var t = _faces[i];
        if (s.Boundary != null)
          t.Boundary = _edges[s.Boundary.InternalTag];

        if (s.Holes != null)
        {
          t.Holes = new List<DcelEdge>(s.Holes.Count);
          for (int j = 0; j < s.Holes.Count; j++)
            if (s.Holes[j] != null)
              t.Holes.Add(_edges[s.Holes[j].InternalTag]);
        }

        t.Tag = s.Tag;
        t.UserData = s.UserData;
      }

      Dirty = false;

      if (mesh.Vertex != null)
        Vertex = _vertices[mesh.Vertex.InternalTag];

      mesh.ResetInternalTags();

      Debug.Assert(_vertices.Count == mesh._vertices.Count, "Cloning of DcelMesh failed. Clone has different number of vertices.");
      Debug.Assert(_edges.Count == mesh._edges.Count, "Cloning of DcelMesh failed. Clone has different number of edges.");
      Debug.Assert(_faces.Count == mesh._faces.Count, "Cloning of DcelMesh failed. Clone has different number of faces.");
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Ensures that the given parameter is an empty list.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="list">The list.</param>
    private static void EnsureEmptyList<T>(ref List<T> list)
    {
      if (list == null)
        list = new List<T>();
      else
        list.Clear();
    }


    /// <summary>
    /// Updates the read-only component lists.
    /// </summary>
    private void UpdateCache()
    {
      if (!Dirty && _vertices != null && _edges != null && _faces != null)
        return;

      Dirty = false;

      // ----- Create new lists or clear existing lists.
      EnsureEmptyList(ref _vertices);
      EnsureEmptyList(ref _edges);
      EnsureEmptyList(ref _faces);

      // ----- Build lists.
      if (Vertex != null)
      {
        _vertices.Add(Vertex);
        Vertex.InternalTag = 1; // InternalTag == 1: element is in list.

        // Build vertex, edge and face lists. 
        BuildLists(Vertex.Edge);

        // Note: Internal tags are reset in BuildLists().
      }
    }


    /// <summary>
    /// Builds the component lists.
    /// </summary>
    /// <param name="edge">The current edge.</param>
    /// <remarks>
    /// This method calls itself recursively.
    /// </remarks>
    private void BuildLists(DcelEdge edge)
    {
      if (edge == null)
        return;

      Stack<DcelEdge> todoStack = new Stack<DcelEdge>();
      todoStack.Push(edge);
      while (todoStack.Count > 0)
      {
        edge = todoStack.Pop();

        if (edge.InternalTag == 1)
          continue;

        // Register edge.
        edge.InternalTag = 1;
        _edges.Add(edge);

        // Register new vertices.
        if (edge.Origin != null && edge.Origin.InternalTag != 1)
        {
          edge.Origin.InternalTag = 1;
          _vertices.Add(edge.Origin);

          AddUntaggedEdgeToStackInternal(edge.Origin.Edge, todoStack, 1);
        }

        // Register new faces.
        if (edge.Face != null && edge.Face.InternalTag != 1)
        {
          edge.Face.InternalTag = 1;
          _faces.Add(edge.Face);

          AddUntaggedEdgeToStackInternal(edge.Face.Boundary, todoStack, 1);
          if (edge.Face.Holes != null)
            for (int i = 0; i < edge.Face.Holes.Count; i++)
              AddUntaggedEdgeToStackInternal(edge.Face.Holes[i], todoStack, 1);
        }

        // Follow neighboring edges.
        AddUntaggedEdgeToStackInternal(edge.Next, todoStack, 1);
        AddUntaggedEdgeToStackInternal(edge.Previous, todoStack, 1);
        AddUntaggedEdgeToStackInternal(edge.Twin, todoStack, 1);
      }

      // Reset tags.
      ResetInternalTags();
    }



    /// <summary>
    /// Resets the tags in the DCEL data. (Tags are set to 0.)
    /// </summary>
    public void ResetTags()
    {
      // Make sure the lists are up-to-date.
      UpdateCache();
      
      int numberOfVertices = _vertices.Count;
      for (int i = 0; i < numberOfVertices; i++)
        _vertices[i].Tag = 0;

      int numberOfEdges = _edges.Count;
      for (int i = 0; i < numberOfEdges; i++)
        _edges[i].Tag = 0;

      int numberOfFaces = _faces.Count;
      for (int i = 0; i < numberOfFaces; i++)
        _faces[i].Tag = 0;
    }


    /// <summary>
    /// Resets the internal tags in the DCEL data. (Internal tags are set to 0.)
    /// </summary>
    /// <remarks>
    /// The component lists must be up-to-date when this method is called! Therefore, internal tags 
    /// must always be reset <strong>before</strong> the mesh is modified!
    /// </remarks>
    private void ResetInternalTags()
    {
      int numberOfVertices = _vertices.Count;
      for (int i = 0; i < numberOfVertices; i++)
        _vertices[i].InternalTag = 0;

      int numberOfEdges = _edges.Count;
      for (int i = 0; i < numberOfEdges; i++)
        _edges[i].InternalTag = 0;

      int numberOfFaces = _faces.Count;
      for (int i = 0; i < numberOfFaces; i++)
        _faces[i].InternalTag = 0;
    }
    #endregion
  }
}
