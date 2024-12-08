// DigitalRise Engine - Copyright (C) DigitalRise GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace DigitalRise.ConverterBase.Meshes
{
  /// <summary>
  /// Stores the processed data for a <strong>Submesh</strong> asset.
  /// </summary>
  public class DRSubmeshContent
  {
    /// <summary>
    /// Gets or sets the vertex buffer associated with this submesh.
    /// </summary>
    /// <value>The vertex buffer associated with this submesh.</value>
    [ContentSerializer(ElementName = "VertexBuffer", SharedResource = true)]
    public VertexBufferContent VertexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the index of the first vertex in the vertex buffer that belongs to this submesh
    /// (a.k.a base vertex or vertex offset).
    /// </summary>
    /// <value>The index of the first vertex in the vertex buffer.</value>
    public int StartVertex { get; set; }


    /// <summary>
    /// Gets or sets the number of vertices used in this submesh.
    /// </summary>
    /// <value>The number of vertices used in this submesh.</value>
    public int VertexCount { get; set; }


    /// <summary>
    /// Gets or sets the index buffer associated with this submesh.
    /// </summary>
    /// <value>The index buffer associated with this submesh.</value>
    [ContentSerializer(ElementName = "IndexBuffer", SharedResource = true)]
    public IndexCollection IndexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the location in the index buffer at which to start reading vertices.
    /// </summary>
    /// <value>The location in the index buffer at which to start reading vertices.</value>
    public int StartIndex { get; set; }


    /// <summary>
    /// Gets or sets the number of primitives to render for this submesh.
    /// </summary>
    /// <value>The number of primitives in this submesh.</value>
    public int PrimitiveCount { get; set; }


    /// <summary>
    /// Gets or sets the morph targets associated with this submesh.
    /// </summary>
    /// <value>The morph targets. The default value is <see langword="null"/>.</value>
    public List<DRMorphTargetContent> MorphTargets { get; set; }


    /// <summary>
    /// Gets or sets a user-defined object.
    /// </summary>
    /// <value>A user-defined object.</value>
    [ContentSerializer(ElementName = "UserData", SharedResource = true)]
    public object UserData { get; set; }
  }
}
