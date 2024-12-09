// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace DigitalRise.ModelStorage.Meshes
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
		public DRVertexBufferContent VertexBuffer { get; set; }


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
		public List<int> Indices { get; set; }


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

		public bool HasChannel(VertexElementUsage usage)
		{
			if (VertexBuffer == null)
			{
				return false;
			}

			return VertexBuffer.HasChannel(usage);
		}

		public DRVertexChannelContent<T> FindChannel<T>(VertexElementUsage usage)
		{
			if (VertexBuffer == null)
			{
				return null;
			}

			return VertexBuffer.FindChannel<T>(usage);
		}

		public DRVertexChannelContent<T> EnsureChannel<T>(VertexElementUsage usage)
		{
			var result = FindChannel<T>(usage);
			if (result == null)
			{
				throw new Exception($"Unable to find channel with usage {usage}");
			}

			return result;
		}

	}
}
