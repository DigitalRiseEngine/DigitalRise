// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Data.Materials;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	internal class RebuildZBufferEffectWrapper : EffectWrapper
	{
		public EffectParameter ViewportSize { get; private set; }
		public EffectParameter Projection { get; private set; }
		public EffectParameter CameraFar { get; private set; }
		public EffectParameter GBuffer0 { get; private set; }
		public EffectParameter Color { get; private set; }
		public EffectParameter SourceTexture { get; private set; }
		public EffectTechnique TechniqueOrthographic { get; private set; }
		public EffectTechnique TechniquePerspective { get; private set; }

		public static RebuildZBufferEffectWrapper Instance { get; } = new RebuildZBufferEffectWrapper();

		private RebuildZBufferEffectWrapper() : base("Deferred/RebuildZBuffer")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			ViewportSize = effect.Parameters["ViewportSize"];
			Projection = effect.Parameters["Projection"];
			CameraFar = effect.Parameters["CameraFar"];
			GBuffer0 = effect.Parameters["GBuffer0"];
			Color = effect.Parameters["Color"];
			SourceTexture = effect.Parameters["SourceTexture"];
			TechniqueOrthographic = effect.Techniques["Orthographic"];
			TechniquePerspective = effect.Techniques["Perspective"];
		}
	}

	/// <summary>
	/// Reconstructs the hardware Z-buffer from the G-buffer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This renderer reads the G-Buffer and outputs depth to the hardware Z-buffer. The resulting
	/// Z-buffer is not totally accurate but should be good enough for most operations.
	/// </para>
	/// <para>
	/// <strong>Render Target and Viewport:</strong><br/>
	/// This renderer renders into the current render target and viewport of the graphics device.
	/// </para>
	/// </remarks>
	public static class RebuildZBufferRenderer
	{
		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the factor used to bias the camera near plane distance to avoid 
		/// z-fighting.
		/// </summary>
		/// <value>The near bias factor. The default value is 1 (no bias).</value>
		public static float NearBias { get; set; } = 1;


		/// <summary>
		/// Gets or sets the factor used to bias the camera far plane distance to avoid 
		/// z-fighting.
		/// </summary>
		/// <value>The far bias factor. The default value is 0.995f.</value>
		public static float FarBias { get; set; } = 0.995f;

		#endregion

		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <overloads>
		/// <summary>
		/// Rebuilds the current hardware Z-buffer from the G-Buffer and optionally writes a color or
		/// texture to the render target.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Rebuilds the current hardware Z-buffer from the G-Buffer and writes the specified color 
		/// value to the current render target.
		/// </summary>
		/// <param name="context">
		/// The render context. (<see cref="RenderContext.CameraNode"/> and 
		/// <see cref="RenderContext.GBuffer0"/> need to be set.)
		/// </param>
		/// <param name="color">The color to be written to the render target.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="context"/> is <see langword="null"/>.
		/// </exception>
		public static void Render(RenderContext context, Vector4 color)
		{
			Render(context, color, null, false);
		}


		/// <summary>
		/// Rebuilds the current hardware Z-buffer from the G-Buffer and copies the specified texture
		/// to the render target.
		/// </summary>
		/// <param name="context">
		/// The render context. (<see cref="RenderContext.CameraNode"/> and 
		/// <see cref="RenderContext.GBuffer0"/> need to be set.)
		/// </param>
		/// <param name="colorTexture">
		/// Optional: The color texture to be copied to the render target.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="context"/> is <see langword="null"/>.
		/// </exception>
		public static void Render(RenderContext context, Texture2D colorTexture)
		{
			Render(context, Vector4.Zero, colorTexture, colorTexture == null);
		}


		/// <summary>
		/// Rebuilds the current hardware Z-buffer from the G-Buffer and clears or preserves the current
		/// render target.
		/// </summary>
		/// <param name="context">
		/// The render context. (<see cref="RenderContext.CameraNode"/> and 
		/// <see cref="RenderContext.GBuffer0"/> need to be set.)
		/// </param>
		/// <param name="preserveColor">
		/// If set to <see langword="true"/> color writes are disabled to preserve the current content;
		/// otherwise, <see langword="false"/> to clear the color target.
		/// </param>
		/// <remarks>
		/// Note that the option <paramref name="preserveColor"/> (to disable color writes) is not 
		/// supported by all render target formats.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="context"/> is <see langword="null"/>.
		/// </exception>
		public static void Render(RenderContext context, bool preserveColor)
		{
			Render(context, Vector4.Zero, null, preserveColor);
		}


		private static void Render(RenderContext context, Vector4 color, Texture2D colorTexture, bool preserveColor)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			var effect = RebuildZBufferEffectWrapper.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();
			context.ThrowIfGBuffer0Missing();

			var graphicsDevice = DR.GraphicsDevice;
			graphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateAlways;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;

			if (preserveColor)
				graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;
			else
				graphicsDevice.BlendState = BlendState.Opaque;

			if (colorTexture != null)
			{
				if (TextureHelper.IsFloatingPointFormat(colorTexture.Format))
					graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
				else
					graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
			}

			var volume = context.CameraNode.ViewVolume;
			bool isPerspective = volume is PerspectiveViewVolume;
			float near = volume.Near * NearBias;
			float far = volume.Far * FarBias;

			var rect = volume.Rectangle;
			var biasedProjection = isPerspective
									 ? Matrix44F.CreatePerspectiveOffCenter(
									   rect.Left, rect.Right,
									   rect.Bottom, rect.Top,
									   near, far)
									 : Matrix44F.CreateOrthographicOffCenter(
									   rect.Left, rect.Right,
									   rect.Bottom, rect.Top,
									   near, far);

			var viewport = graphicsDevice.Viewport;
			effect.ViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
			effect.Projection.SetValue((Matrix)biasedProjection);
			effect.CameraFar.SetValue(volume.Far);
			effect.GBuffer0.SetValue(context.GBuffer0);
			effect.Color.SetValue(color);
			effect.SourceTexture.SetValue(colorTexture);

			effect.Effect.CurrentTechnique = isPerspective ? effect.TechniquePerspective : effect.TechniqueOrthographic;
			var pass = effect.Effect.CurrentTechnique.Passes[(colorTexture == null) ? 0 : 1];

			context.DrawFullScreenQuad(pass);

			graphicsDevice.ResetTextures();
		}
		#endregion
	}
}
