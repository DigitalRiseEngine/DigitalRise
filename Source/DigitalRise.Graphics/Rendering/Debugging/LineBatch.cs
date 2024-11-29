// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Debugging
{
	/// <summary>
	/// Renders a batch of lines.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A valid <see cref="Effect"/> must be set; otherwise, <see cref="Render"/> will not draw any 
	/// lines. The <see cref="LineBatch"/> uses the currently set render state (blend state,
	/// depth-stencil state, rasterizer state).
	/// </para>
	/// </remarks>
	internal sealed class LineBatch
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private VertexPositionColor[] _buffer = new VertexPositionColor[256];
		private int _numberOfLines;
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
		/// Initializes a new instance of the <see cref="LineBatch"/> class.
		/// </summary>
		/// <param name="effect">
		/// The effect. If this value is <see langword="null"/>, then the batch will not draw anything
		/// when <see cref="Render"/> is called.
		/// </param>
		public LineBatch(BasicEffect effect)
		{
			Effect = effect;
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Removes all lines.
		/// </summary>
		public void Clear()
		{
			_numberOfLines = 0;
		}


		/// <summary>
		/// Adds a line.
		/// </summary>
		/// <param name="start">The start position in world space.</param>
		/// <param name="end">The end position in world space.</param>
		/// <param name="color">The color (using non-premultiplied alpha).</param>
		public void Add(Vector3 start, Vector3 end, Color color)
		{
			ResizeBuffer(_numberOfLines + 1);

			// Premultiply color with alpha.
			color = Color.FromNonPremultiplied(color.R, color.G, color.B, color.A);

			_buffer[_numberOfLines * 2 + 0].Position = (Vector3)start;
			_buffer[_numberOfLines * 2 + 0].Color = color;
			_buffer[_numberOfLines * 2 + 1].Position = (Vector3)end;
			_buffer[_numberOfLines * 2 + 1].Color = color;

			_numberOfLines++;
		}


		/// <summary>
		/// Draws the lines.
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

			if (Effect == null)
				return;

			if (_numberOfLines <= 0)
				return;

			Effect.Validate();

			// Reset the texture stages. If a floating point texture is set, we get exceptions
			// when a sampler with bilinear filtering is set.
			var graphicsDevice = DR.GraphicsDevice;
			graphicsDevice.ResetTextures();

			// Effect parameters.
			Effect.Alpha = 1;
			Effect.DiffuseColor = Color.White.ToVector3();
			Effect.LightingEnabled = false;
			Effect.TextureEnabled = false;
			Effect.VertexColorEnabled = true;
			Effect.World = Matrix.Identity;
			Effect.View = (Matrix)cameraNode.View;
			Effect.Projection = cameraNode.Camera.Projection;
			Effect.CurrentTechnique.Passes[0].Apply();

			// Submit lines. The loop is only needed if we have more lines than can be 
			// submitted with one draw call.
			var startLineIndex = 0;
			var maxPrimitivesPerCall = graphicsDevice.GetMaxPrimitivesPerCall();
			while (startLineIndex < _numberOfLines)
			{
				// Number of lines in this batch.
				int linesPerBatch = Math.Min(_numberOfLines - startLineIndex, maxPrimitivesPerCall);

				// Draw lines.
				graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _buffer, startLineIndex * 2, linesPerBatch);

				startLineIndex += linesPerBatch;
			}
		}


		private void ResizeBuffer(int numberOfLines)
		{
			int requiredBufferLength = numberOfLines * 2;
			if (_buffer.Length >= requiredBufferLength)
				return;

			// Double buffer until it is large enough.
			int newBufferLength = _buffer.Length * 2;
			while (newBufferLength < requiredBufferLength)
				newBufferLength *= 2;

			// Copy old buffer to new buffer.
			var newBuffer = new VertexPositionColor[newBufferLength];
			Array.Copy(_buffer, newBuffer, _buffer.Length);

			_buffer = newBuffer;
		}
		#endregion
	}
}
