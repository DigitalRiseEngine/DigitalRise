// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DigitalRise.Mathematics;
using DigitalRise.Data.Materials;


namespace DigitalRise.Rendering.Deferred
{
	internal class ClearGBufferEffectWrapper : EffectWrapper
	{
		private static ClearGBufferEffectWrapper _instance;

		public EffectParameter Depth { get; private set; }
		public EffectParameter Normal { get; private set; }
		public EffectParameter SpecularPower { get; private set; }

		public static ClearGBufferEffectWrapper Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ClearGBufferEffectWrapper();
				}

				return _instance;
			}
		}

		private ClearGBufferEffectWrapper() : base("Deferred/ClearGBuffer")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			Depth = effect.Parameters["Depth"];
			Normal = effect.Parameters["Normal"];
			SpecularPower = effect.Parameters["SpecularPower"];
		}
	}

	/// <summary>
	/// Clears the G-buffer. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// <strong>Render Targets and Viewport:</strong><br/>
	/// This renderer renders into the current render target and viewport of the graphics device. The
	/// render target should be the G-buffer.
	/// </para>
	/// </remarks>
	public static class ClearGBufferRenderer
	{
		/// <summary>
		/// Clears the current render target (which must be the G-buffer).
		/// </summary>
		/// <param name="context">The render context.</param>
		public static void Render(RenderContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			var effect = ClearGBufferEffectWrapper.Instance;
			effect.Validate();

			var graphicsDevice = DR.GraphicsDevice;
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.BlendState = BlendState.Opaque;

			// Clear to maximum depth.
			effect.Depth.SetValue(1.0f);

			// The environment is facing the camera.
			// --> Set normal = cameraBackward.
			var cameraNode = context.CameraNode;
			effect.Normal.SetValue((cameraNode != null) ? cameraNode.ViewInverse.GetColumn(2).XYZ() : Vector3.Backward);

			// Clear specular to arbitrary value.
			effect.SpecularPower.SetValue(1.0f);

			var pass = effect.CurrentTechnique.Passes[0];

			// Draw full-screen quad using clip space coordinates.
			context.DrawQuad(pass,
			  new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
			  new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)));
		}
	}
}
