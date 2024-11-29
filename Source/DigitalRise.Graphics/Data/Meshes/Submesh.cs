// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Meshes.Morphing;
using DigitalRise.Misc;
using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Data.Meshes
{
	/// <summary>
	/// Represents a batch of geometry information to submit to the graphics device during rendering.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Graphics.Mesh"/> has a collection of <see cref="Graphics.Mesh.Materials"/> and is
	/// subdivided into several submeshes. Each <see cref="Submesh"/> describes a batch of primitives
	/// that use the same vertex buffer and the same material, which means a submesh can be rendered
	/// with one draw call. 
	/// </para>
	/// <para>
	/// The submesh references a <see cref="VertexBuffer"/> and an <see cref="IndexBuffer"/>. These
	/// buffers are usually shared with other submeshes of the same model.
	/// </para>
	/// <para>
	/// The submesh uses a continuous part of the <see cref="VertexBuffer"/>, starting at
	/// <see cref="StartVertex"/> and containing <see cref="VertexCount"/> vertices. The submesh also
	/// uses a continuous part of the <see cref="IndexBuffer"/>, starting at <see cref="StartIndex"/>.
	/// <see cref="PrimitiveCount"/> defines the number of primitives (usually triangles) that belong
	/// to this 
	/// </para>
	/// </remarks>
	/// <seealso cref="EffectBinding"/>
	/// <seealso cref="Material"/>
	/// <seealso cref="Mesh"/>
	public class Submesh : IDisposable
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		/// <summary>Temporary ID set during rendering.</summary>
		internal uint Id;
		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets the mesh that own this 
		/// </summary>
		/// <value>The mesh.</value>
		[Category("Common")]
		public Mesh Mesh { get; internal set; }


		/// <summary>
		/// Gets or sets the type of the primitive.
		/// </summary>
		/// <value>The type of the primitive. The default type is triangle list.</value>
		[Category("Graphics")]
		public PrimitiveType PrimitiveType { get; set; }


		/// <summary>
		/// Gets or sets the vertex buffer.
		/// </summary>
		/// <value>The vertex buffer.</value>
		/// <remarks>
		/// Vertex buffers and index buffers may be shared between meshes, submeshes, or morph targets.
		/// </remarks>
		[Category("Graphics")]
		public VertexBuffer VertexBuffer { get; set; }


		/// <summary>
		/// Gets or sets the index of the first vertex in the vertex buffer that belongs to this submesh
		/// (a.k.a base vertex or vertex offset).
		/// </summary>
		/// <value>The index of the first vertex in the vertex buffer.</value>
		[Category("Graphics")]
		public int StartVertex { get; set; }


		/// <summary>
		/// Gets or sets the number of vertices.
		/// </summary>
		/// <value>The number of vertices.</value>
		[Category("Graphics")]
		public int VertexCount { get; set; }


		/// <summary>
		/// Gets or sets the index buffer.
		/// </summary>
		/// <value>The index buffer.</value>
		/// <remarks>
		/// Vertex buffers and index buffers may be shared between meshes or submeshes.
		/// </remarks>
		[Category("Graphics")]
		public IndexBuffer IndexBuffer { get; set; }


		/// <summary>
		/// Gets or sets the location in the index array at which to start reading vertices.
		/// </summary>
		/// <value>Location in the index array at which to start reading vertices.</value>
		[Category("Graphics")]
		public int StartIndex { get; set; }


		/// <summary>
		/// Gets or sets the number of primitives (usually the number of triangles).
		/// </summary>
		/// <value>The number of primitives.</value>
		[Category("Graphics")]
		public int PrimitiveCount { get; set; }


		[Category("Material")]
		public IMaterial Material { get; set; }

		/// <summary>
		/// Gets a value indicating whether this submesh has morph targets.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this submesh has morph targets; otherwise,
		/// <see langword="false"/>.
		/// </value>
		[Category("Animation")]
		internal bool HasMorphTargets
		{
			get { return _morphTargets != null && _morphTargets.Count > 0; }
		}


		/// <summary>
		/// Gets or sets the morph targets of the 
		/// </summary>
		/// <value>
		/// The morph targets of the  The default value is <see langword="null"/>.
		/// </value>
		/// <exception cref="InvalidOperationException">
		/// The specified <see cref="MorphTargetCollection"/> cannot be assigned to the 
		/// <see cref="Submesh"/> because it already belongs to another <see cref="Submesh"/> instance.
		/// </exception>
		[Category("Animation")]
		public MorphTargetCollection MorphTargets
		{
			get { return _morphTargets; }
			set
			{
				if (_morphTargets == value)
					return;

				if (value != null && value.Submesh != null)
					throw new InvalidOperationException("Cannot assign MorphTargetCollection to  The MorphTargetCollection already belongs to another ");

				if (_morphTargets != null)
					_morphTargets.Submesh = null;

				_morphTargets = value;

				if (value != null)
					value.Submesh = this;

				InvalidateMorphTargetNames();
			}
		}
		private MorphTargetCollection _morphTargets;

		public BoundingBox BoundingBox { get; }

		public bool HasNormals { get; }


		/// <summary>
		/// Gets or sets user-defined data.
		/// </summary>
		/// <value>User-defined data.</value>
		/// <remarks>
		/// This property is intended for application-specific data and is not used by the submesh 
		/// itself. 
		/// </remarks>
		[Category("Misc")]
		public object UserData { get; set; }
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		internal Submesh()
		{
		}

		public Submesh(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, BoundingBox boundingBox, PrimitiveType primitiveType = PrimitiveType.TriangleList,
			int? vertexCount = null, int? primitiveCount = null)
		{
			VertexBuffer = vertexBuffer ?? throw new ArgumentNullException(nameof(vertexBuffer));
			IndexBuffer = indexBuffer ?? throw new ArgumentNullException(nameof(indexBuffer));
			PrimitiveType = primitiveType;
			VertexCount = vertexCount ?? vertexBuffer.VertexCount;
			PrimitiveCount = primitiveCount ?? primitiveType.GetPrimitiveCount(IndexBuffer.IndexCount);

			BoundingBox = boundingBox;
			HasNormals = (from el in VertexBuffer.VertexDeclaration.GetVertexElements() where el.VertexElementUsage == VertexElementUsage.Normal select el).Count() > 0;
		}

		public Submesh(VertexPositionNormalTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPositionNormalTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPositionTexture[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPositionTexture[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPositionNormal[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPositionNormal[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPosition[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(VertexPosition[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(Vector3[] vertices, ushort[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}

		public Submesh(Vector3[] vertices, int[] indices, PrimitiveType primitiveType = PrimitiveType.TriangleList) :
			this(vertices.CreateVertexBuffer(), indices.CreateIndexBuffer(), vertices.BuildBoundingBox(), primitiveType)
		{
		}


		/// <summary>
		/// Releases all resources used by an instance of the <see cref="Submesh"/> class.
		/// </summary>
		/// <remarks>
		/// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
		/// <see langword="true"/>, and then suppresses finalization of the instance.
		/// </remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Releases the unmanaged resources used by an instance of the <see cref="Submesh"/> class 
		/// and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// <see langword="true"/> to release both managed and unmanaged resources; 
		/// <see langword="false"/> to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose managed resources.
				VertexBuffer.SafeDispose();
				IndexBuffer.SafeDispose();

				if (MorphTargets != null)
					foreach (var morphTarget in MorphTargets)
						morphTarget.Dispose();

				UserData.SafeDispose();
			}
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Clears the morph target names, which are cached by the <see cref="Mesh"/>.
		/// </summary>
		internal void InvalidateMorphTargetNames()
		{
			if (Mesh != null)
				Mesh.InvalidateMorphTargetNames();
		}

		public Submesh Clone()
		{
			return new Submesh
			{
				PrimitiveType = PrimitiveType,
				VertexBuffer = VertexBuffer,
				StartVertex = StartVertex,
				VertexCount = VertexCount,
				IndexBuffer = IndexBuffer,
				StartIndex = StartIndex,
				PrimitiveCount = PrimitiveCount,
				Material = Material.Clone(),
				MorphTargets = MorphTargets,
				UserData = UserData,
			};
		}

		/// <summary>
		/// Draws the <see cref="Submesh"/> using the currently active shader.
		/// </summary>
		/// <param name="submesh">The </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="submesh"/> is <see langword="null"/>.
		/// </exception>
		/// <remarks>
		/// This method sets the <see cref="VertexDeclaration"/>, <see cref="VertexBuffer"/>,
		/// and <see cref="IndexBuffer"/> of the submesh and calls
		/// <see cref="GraphicsDevice.DrawIndexedPrimitives"/>. Effects are not handled in this method.
		/// The method assumes that the correct shader effect is already active.
		/// </remarks>
		public void Draw()
		{
			Debug.Assert(!HasMorphTargets, "Submesh without morph targets expected.");

			var vertexBuffer = VertexBuffer;
			if (vertexBuffer == null || VertexCount <= 0)
				return;

			// VertexBuffer.GraphicsDevice is set to null when VertexBuffer is disposed of.
			// Check VertexBuffer.IsDisposed to avoid NullReferenceException.
			if (vertexBuffer.IsDisposed)
				throw new ObjectDisposedException("VertexBuffer", "Cannot draw mesh. The vertex buffer has already been disposed of.");

			var graphicsDevice = vertexBuffer.GraphicsDevice;
			graphicsDevice.SetVertexBuffer(vertexBuffer);

			var indexBuffer = IndexBuffer;
			if (indexBuffer == null)
			{
				graphicsDevice.DrawPrimitives(
					PrimitiveType,
					StartVertex,
					PrimitiveCount);
			}
			else
			{
				graphicsDevice.Indices = indexBuffer;
#if MONOGAME
				graphicsDevice.DrawIndexedPrimitives(
					PrimitiveType,
					StartVertex,
					StartIndex,
					PrimitiveCount);
#else
				graphicsDevice.DrawIndexedPrimitives(
					PrimitiveType,
					StartVertex,
					0,
					VertexCount,
					StartIndex,
					PrimitiveCount);
#endif
			}
		}
		#endregion
	}
}
