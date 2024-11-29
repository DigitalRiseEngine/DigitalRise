// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRise.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Debugging
{
	/// <summary>
	/// Renders a batch of textures (usually for debugging).
	/// </summary>
	/// <remarks>
	/// A valid <see cref="SpriteBatch"/> must be set; otherwise, <see cref="Render"/> will not draw
	/// any points.
	/// </remarks>
	internal sealed class TextureBatch
	{
		//--------------------------------------------------------------
		#region Nested Types
		//--------------------------------------------------------------

		/// <summary>Describes a draw info for a texture.</summary>
		private struct TextureInfo
		{
			/// <summary>The texture.</summary>
			public readonly Texture2D Texture;

			/// <summary>The target position and size in screen space.</summary>
			public readonly Rectangle Rectangle;

			/// <summary>
			/// Initializes a new instance of the <see cref="TextureInfo"/> struct.
			/// </summary>
			/// <param name="texture">The texture.</param>
			/// <param name="rectangle">The position rectangle in screen space.</param>
			public TextureInfo(Texture2D texture, Rectangle rectangle)
			{
				Texture = texture;
				Rectangle = rectangle;
			}
		}
		#endregion


		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private readonly List<TextureInfo> _textures = new List<TextureInfo>();
		#endregion

		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Removes all textures.
		/// </summary>
		public void Clear()
		{
			_textures.Clear();
		}


		/// <summary>
		/// Adds a texture.
		/// </summary>
		/// <param name="texture">The texture.</param>
		/// <param name="rectangle">The target position and size in screen space.</param>
		public void Add(Texture2D texture, Rectangle rectangle)
		{
			if (texture != null)
				_textures.Add(new TextureInfo(texture, rectangle));
		}


		/// <summary>
		/// Draws the textures.
		/// </summary>
		/// <remarks>
		/// If <see cref="SpriteBatch"/> is <see langword="null"/>, then <see cref="Render"/> does 
		/// nothing.
		/// </remarks>
		public void Render()
		{
			var count = _textures.Count;
			if (count == 0)
				return;

			var spriteBatch = Resources.SpriteBatch;
			var savedRenderState = new RenderStateSnapshot();

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

			for (int i = 0; i < count; i++)
			{
				var textureInfo = _textures[i];

				if (textureInfo.Texture.IsDisposed)
					continue;

				if (TextureHelper.IsFloatingPointFormat(textureInfo.Texture.Format))
				{
					// Floating-point textures must not use linear hardware filtering!
					spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
					spriteBatch.Draw(textureInfo.Texture, textureInfo.Rectangle, Color.White);
					spriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
				}
				else
				{
					spriteBatch.Draw(textureInfo.Texture, textureInfo.Rectangle, Color.White);
				}
			}
			spriteBatch.End();

			savedRenderState.Restore();
		}
		#endregion
	}
}
