// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Debugging
{
	/// <summary>
	/// Renders a batch of triangles.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A valid <see cref="Effect"/> must be set; otherwise, <see cref="Render"/> will not draw any 
	/// triangles. The <see cref="TriangleBatch"/> uses the currently set render state (blend state,
	/// depth-stencil state, rasterizer state).
	/// </para>
	/// </remarks>
	internal sealed class TriangleBatch
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private VertexPositionNormalColor[] _buffer = new VertexPositionNormalColor[512];
		private int _numberOfTriangles;
		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the effect.
		/// </summary>
		/// <value>The effect.</value>
		/// <remarks>
		/// If this value is <see langword="null"/>, then <see cref="Render"/> does nothing.
		/// </remarks>
		public BasicEffect Effect { get; set; }
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="TriangleBatch"/> class.
		/// </summary>
		/// <param name="effect">
		/// The effect. If this value is <see langword="null"/>, then the batch will not draw anything
		/// when <see cref="Render"/> is called.
		/// </param>
		public TriangleBatch(BasicEffect effect)
		{
			Effect = effect;
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Removes all triangles.
		/// </summary>
		public void Clear()
		{
			_numberOfTriangles = 0;
		}


		/// <overloads>
		/// <summary>
		/// Adds a triangle.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Adds a triangle.
		/// </summary>
		/// <param name="vertex0">The first vertex position in world space.</param>
		/// <param name="normal0">The normal vector of the first vertex position.</param>
		/// <param name="vertex1">The second vertex position in world space.</param>
		/// <param name="normal1">The normal vector of second vertex position.</param>
		/// <param name="vertex2">The third vertex position in world space.</param>
		/// <param name="normal2">The normal vector of third vertex position.</param>
		/// <param name="color">The color (using non-premultiplied alpha).</param>
		/// <remarks>
		/// Triangles have to use clockwise winding for front sides.
		/// </remarks>
		public void Add(Vector3 vertex0, Vector3 normal0, Vector3 vertex1, Vector3 normal1,
						Vector3 vertex2, Vector3 normal2, Color color)
		{
			// Premultiply color with alpha.
			color = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);

			ResizeBuffer(_numberOfTriangles + 1);

			_buffer[_numberOfTriangles * 3 + 0].Position = vertex0;
			_buffer[_numberOfTriangles * 3 + 0].Normal = normal0;
			_buffer[_numberOfTriangles * 3 + 0].Color = color;
			_buffer[_numberOfTriangles * 3 + 1].Position = vertex1;
			_buffer[_numberOfTriangles * 3 + 1].Normal = normal1;
			_buffer[_numberOfTriangles * 3 + 1].Color = color;
			_buffer[_numberOfTriangles * 3 + 2].Position = vertex2;
			_buffer[_numberOfTriangles * 3 + 2].Normal = normal2;
			_buffer[_numberOfTriangles * 3 + 2].Color = color;

			_numberOfTriangles++;
		}


		/// <summary>
		/// Adds a triangle.
		/// </summary>
		/// <param name="vertex0">The first vertex position in world space.</param>
		/// <param name="vertex1">The second vertex position in world space.</param>
		/// <param name="vertex2">The third vertex position in world space.</param>
		/// <param name="normal">The normal vector of the triangle.</param>
		/// <param name="color">The color (using non-premultiplied alpha).</param>
		/// <remarks>
		/// Triangles have to use clockwise winding for front sides.
		/// </remarks>
		public void Add(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2,
						Vector3 normal, Color color)
		{
			// Premultiply color with alpha.
			color = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);

			ResizeBuffer(_numberOfTriangles + 1);

			_buffer[_numberOfTriangles * 3 + 0].Position = vertex0;
			_buffer[_numberOfTriangles * 3 + 0].Normal = normal;
			_buffer[_numberOfTriangles * 3 + 0].Color = color;
			_buffer[_numberOfTriangles * 3 + 1].Position = vertex1;
			_buffer[_numberOfTriangles * 3 + 1].Normal = normal;
			_buffer[_numberOfTriangles * 3 + 1].Color = color;
			_buffer[_numberOfTriangles * 3 + 2].Position = vertex2;
			_buffer[_numberOfTriangles * 3 + 2].Normal = normal;
			_buffer[_numberOfTriangles * 3 + 2].Color = color;

			_numberOfTriangles++;
		}


		/// <summary>
		/// Adds a triangle.
		/// </summary>
		/// <param name="vertex0">The first vertex position in world space.</param>
		/// <param name="vertex1">The second vertex position in world space.</param>
		/// <param name="vertex2">The third vertex position in world space.</param>
		/// <param name="normal">The normal vector of the triangle.</param>
		/// <param name="color">The color (using non-premultiplied alpha).</param>
		/// <remarks>
		/// Triangles have to use clockwise winding for front sides.
		/// </remarks>
		public void Add(ref Vector3 vertex0, ref Vector3 vertex1, ref Vector3 vertex2,
						ref Vector3 normal, ref Color color)
		{
			// Premultiply color with alpha.
			var colorPremultiplied = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);

			ResizeBuffer(_numberOfTriangles + 1);

			_buffer[_numberOfTriangles * 3 + 0].Position = vertex0;
			_buffer[_numberOfTriangles * 3 + 0].Normal = normal;
			_buffer[_numberOfTriangles * 3 + 0].Color = colorPremultiplied;
			_buffer[_numberOfTriangles * 3 + 1].Position = vertex1;
			_buffer[_numberOfTriangles * 3 + 1].Normal = normal;
			_buffer[_numberOfTriangles * 3 + 1].Color = colorPremultiplied;
			_buffer[_numberOfTriangles * 3 + 2].Position = vertex2;
			_buffer[_numberOfTriangles * 3 + 2].Normal = normal;
			_buffer[_numberOfTriangles * 3 + 2].Color = colorPremultiplied;

			_numberOfTriangles++;
		}


		/// <summary>
		/// Draws the triangles.
		/// </summary>
		/// <param name="cameraNode"></param>
		/// <remarks>
		/// If <see cref="Effect"/> is <see langword="null"/>, then <see cref="Render"/> does nothing.
		/// </remarks>
		public void Render(CameraNode cameraNode)
		{
			if (cameraNode == null)
			{
				throw new ArgumentNullException(nameof(cameraNode));
			}

			if (Effect == null || _numberOfTriangles == 0)
				return;

			Effect.Validate();

			var graphicsDevice = DR.GraphicsDevice;

			// Effect parameters.
			Effect.Alpha = 1;
			Effect.DiffuseColor = new Vector3(1, 1, 1);
			// Lighting is used for solid, but not for wireframe triangles.
			Effect.LightingEnabled = graphicsDevice.RasterizerState.FillMode == FillMode.Solid;
			Effect.TextureEnabled = false;
			Effect.VertexColorEnabled = true;
			Effect.World = Matrix.Identity;
			Effect.View = (Matrix)cameraNode.View;
			Effect.Projection = (Matrix)cameraNode.ViewVolume.Projection;
			Effect.CurrentTechnique.Passes[0].Apply();

			// Submit triangles. The loop is only needed if we have more triangles that can be submitted 
			// with one draw call.
			int startTriangleIndex = 0;
			int maxPrimitivesPerCall = graphicsDevice.GetMaxPrimitivesPerCall();
			while (startTriangleIndex < _numberOfTriangles)
			{
				// Number of triangle in this batch.
				int trianglesPerBatch = Math.Min(_numberOfTriangles - startTriangleIndex, maxPrimitivesPerCall);

				// Draw triangles.
				graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _buffer, startTriangleIndex * 3, trianglesPerBatch);

				startTriangleIndex += trianglesPerBatch;
			}
		}


		private void ResizeBuffer(int numberOfTriangles)
		{
			int requiredBufferLength = numberOfTriangles * 3;
			if (_buffer.Length >= requiredBufferLength)
				return;

			// Double buffer until it is large enough.
			int newBufferLength = _buffer.Length * 2;
			while (newBufferLength < requiredBufferLength)
				newBufferLength *= 2;

			// Copy old buffer to new buffer.
			var newBuffer = new VertexPositionNormalColor[newBufferLength];
			Array.Copy(_buffer, newBuffer, _buffer.Length);

			_buffer = newBuffer;
		}
		#endregion
	}
}
