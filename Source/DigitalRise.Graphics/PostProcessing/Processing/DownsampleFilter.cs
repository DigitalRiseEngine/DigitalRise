// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRise.Data.Materials;
using DigitalRise.Misc;
using DigitalRise.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.PostProcessing.Processing
{
	internal class DownsampleFilterEffectBinding : EffectWrapper
	{
		public EffectParameter _sourceSizeParameter { get; private set; }
		public EffectParameter _targetSizeParameter { get; private set; }
		public EffectParameter _sourceTextureParameter { get; private set; }
		public EffectPass _linear2Pass { get; private set; }
		public EffectPass _linear4Pass { get; private set; }
		public EffectPass _linear6Pass { get; private set; }
		public EffectPass _linear8Pass { get; private set; }
		public EffectPass _point2Pass { get; private set; }
		public EffectPass _point3Pass { get; private set; }
		public EffectPass _point4Pass { get; private set; }
		public EffectPass _point2DepthPass { get; private set; }
		public EffectPass _point3DepthPass { get; private set; }
		public EffectPass _point4DepthPass { get; private set; }

		public DownsampleFilterEffectBinding() : base("PostProcessing/DownsampleFilter")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			_sourceSizeParameter = effect.Parameters["SourceSize"];
			_targetSizeParameter = effect.Parameters["TargetSize"];
			_sourceTextureParameter = effect.Parameters["SourceTexture"];
			_linear2Pass = effect.CurrentTechnique.Passes["Linear2"];
			_linear4Pass = effect.CurrentTechnique.Passes["Linear4"];
			_linear6Pass = effect.CurrentTechnique.Passes["Linear6"];
			_linear8Pass = effect.CurrentTechnique.Passes["Linear8"];
			_point2Pass = effect.CurrentTechnique.Passes["Point2"];
			_point3Pass = effect.CurrentTechnique.Passes["Point3"];
			_point4Pass = effect.CurrentTechnique.Passes["Point4"];
			_point2DepthPass = effect.CurrentTechnique.Passes["Point2Depth"];
			_point3DepthPass = effect.CurrentTechnique.Passes["Point3Depth"];
			_point4DepthPass = effect.CurrentTechnique.Passes["Point4Depth"];
		}
	}

	/// <summary>
	/// Reduces the resolution of an input texture.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This post-processor reduces the resolution of the <see cref="RenderContext.SourceTexture"/>
	/// to match the target <see cref="RenderContext.Viewport"/>. 
	/// </para>
	/// <para>
	/// If this post-processor is used in a <see cref="PostProcessorChain"/>, you can use the 
	/// property <see cref="PostProcessor.DefaultTargetFormat"/> to specify the target resolution.
	/// </para>
	/// <para>
	/// Render targets are downsampled by averaging samples. However, some render targets might
	/// require a different downsample function. The <see cref="DownsampleFilter"/> will
	/// check if the source texture is the depth buffer (<see cref="RenderContext.GBuffer0"/>), 
	/// and when this is the case, it will perform a special downsampling.
	/// </para>
	/// </remarks>
	public class DownsampleFilter : PostProcessor
	{
		private readonly DownsampleFilterEffectBinding _effect = new DownsampleFilterEffectBinding();

		/// <inheritdoc/>
		protected override void OnProcess(RenderContext context)
		{
			var graphicsDevice = DR.GraphicsDevice;

			// The width/height of the current input.
			int sourceWidth = context.SourceTexture.Width;
			int sourceHeight = context.SourceTexture.Height;

			// The target width/height.
			var viewport = graphicsDevice.Viewport;
			int targetWidth = viewport.Width;
			int targetHeight = viewport.Height;

			var lastRenderTarget = (RenderTarget2D)DR.GraphicsDevice.GetCurrentRenderTarget();

			// Surface format of input.
			bool isFloatingPointFormat = TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format);

			// Floating-point formats cannot use linear filtering, so we need two different paths.
			RenderTarget2D last = null;
			if (!isFloatingPointFormat)
			{
				// ----- We can use bilinear hardware filtering.
				do
				{
					// Determine downsample factor. Use the largest possible factor to minimize passes.
					int factor;
					if (sourceWidth / 2 <= targetWidth && sourceHeight / 2 <= targetHeight)
						factor = 2;
					else if (sourceWidth / 4 <= targetWidth && sourceHeight / 4 <= targetHeight)
						factor = 4;
					else if (sourceWidth / 6 <= targetWidth && sourceHeight / 6 <= targetHeight)
						factor = 6;
					else
						factor = 8;

					// Downsample to this target size.
					int tempTargetWidth = Math.Max(targetWidth, sourceWidth / factor);
					int tempTargetHeight = Math.Max(targetHeight, sourceHeight / factor);

					// Is this the final pass that renders into context.RenderTarget?
					bool isFinalPass = (tempTargetWidth <= targetWidth && tempTargetHeight <= targetHeight);
					RenderTarget2D temp = null;
					if (isFinalPass)
					{
						graphicsDevice.SetRenderTarget(lastRenderTarget);
						graphicsDevice.Viewport = viewport;
					}
					else
					{
						// Get temporary render target for intermediate steps.
						var tempFormat = new RenderTargetFormat(tempTargetWidth, tempTargetHeight, false, context.SourceTexture.Format, DepthFormat.None);
						temp = context.RenderTargetPool.Obtain2D(tempFormat);
						graphicsDevice.SetRenderTarget(temp);
					}

					_effect._sourceSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
					_effect._targetSizeParameter.SetValue(new Vector2(tempTargetWidth, tempTargetHeight));
					_effect._sourceTextureParameter.SetValue(last ?? context.SourceTexture);

					EffectPass pass = null;
					if (factor == 2)
						pass = _effect._linear2Pass;
					else if (factor == 4)
						pass = _effect._linear4Pass;
					else if (factor == 6)
						pass = _effect._linear6Pass;
					else if (factor == 8)
						pass = _effect._linear8Pass;

					context.DrawFullScreenQuad(pass);

					context.RenderTargetPool.Recycle(last);
					last = temp;
					sourceWidth = tempTargetWidth;
					sourceHeight = tempTargetHeight;
				} while (sourceWidth > targetWidth || sourceHeight > targetHeight);
			}
			else
			{
				// ----- We cannot use hardware filtering. :-(
				do
				{
					// Determine downsample factor. Use the largest possible factor to minimize passes.
					int factor;
					if (sourceWidth / 2 <= targetWidth && sourceHeight / 2 <= targetHeight)
						factor = 2;
					else if (sourceWidth / 3 <= targetWidth && sourceHeight / 3 <= targetHeight)
						factor = 3;
					else
						factor = 4;

					// Downsample to this target size.
					int tempTargetWidth = Math.Max(targetWidth, sourceWidth / factor);
					int tempTargetHeight = Math.Max(targetHeight, sourceHeight / factor);

					// Is this the final pass that renders into context.RenderTarget?
					bool isFinalPass = (tempTargetWidth <= targetWidth && tempTargetHeight <= targetHeight);
					RenderTarget2D temp = null;
					if (isFinalPass)
					{
						graphicsDevice.SetRenderTarget(lastRenderTarget);
						graphicsDevice.Viewport = viewport;
					}
					else
					{
						// Get temporary render target for intermediate steps.
						var tempFormat = new RenderTargetFormat(tempTargetWidth, tempTargetHeight, false, context.SourceTexture.Format, DepthFormat.None);
						temp = context.RenderTargetPool.Obtain2D(tempFormat);
						graphicsDevice.SetRenderTarget(temp);
					}

					_effect._sourceSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
					_effect._targetSizeParameter.SetValue(new Vector2(tempTargetWidth, tempTargetHeight));
					var source = last ?? context.SourceTexture;
					_effect._sourceTextureParameter.SetValue(source);

					EffectPass pass = null;
					if (source != context.GBuffer0)
					{
						if (factor == 2)
							pass = _effect._point2Pass;
						else if (factor == 3)
							pass = _effect._point3Pass;
						else
							pass = _effect._point4Pass;
					}
					else
					{
						// This is the depth buffer and it needs special handling.
						if (factor == 2)
							pass = _effect._point2DepthPass;
						else if (factor == 3)
							pass = _effect._point3DepthPass;
						else
							pass = _effect._point4DepthPass;
					}

					context.DrawFullScreenQuad(pass);
					context.RenderTargetPool.Recycle(last);
					last = temp;
					sourceWidth = tempTargetWidth;
					sourceHeight = tempTargetHeight;
				} while (sourceWidth > targetWidth || sourceHeight > targetHeight);

				_effect._sourceTextureParameter.SetValue((Texture2D)null);

				Debug.Assert(last == null, "Intermediate render target should have been recycled.");
			}
		}
	}
}
