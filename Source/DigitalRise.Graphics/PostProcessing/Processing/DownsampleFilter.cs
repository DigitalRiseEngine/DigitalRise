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
		public EffectParameter SourceSizeParameter { get; private set; }
		public EffectParameter TargetSizeParameter { get; private set; }
		public EffectParameter SourceTextureParameter { get; private set; }
		public EffectPass Linear2Pass { get; private set; }
		public EffectPass Linear4Pass { get; private set; }
		public EffectPass Linear6Pass { get; private set; }
		public EffectPass Linear8Pass { get; private set; }
		public EffectPass Point2Pass { get; private set; }
		public EffectPass Point3Pass { get; private set; }
		public EffectPass Point4Pass { get; private set; }
		public EffectPass Point2DepthPass { get; private set; }
		public EffectPass Point3DepthPass { get; private set; }
		public EffectPass Point4DepthPass { get; private set; }

		public DownsampleFilterEffectBinding() : base("PostProcessing/DownsampleFilter")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			SourceSizeParameter = effect.Parameters["SourceSize"];
			TargetSizeParameter = effect.Parameters["TargetSize"];
			SourceTextureParameter = effect.Parameters["SourceTexture"];
			Linear2Pass = effect.CurrentTechnique.Passes["Linear2"];
			Linear4Pass = effect.CurrentTechnique.Passes["Linear4"];
			Linear6Pass = effect.CurrentTechnique.Passes["Linear6"];
			Linear8Pass = effect.CurrentTechnique.Passes["Linear8"];
			Point2Pass = effect.CurrentTechnique.Passes["Point2"];
			Point3Pass = effect.CurrentTechnique.Passes["Point3"];
			Point4Pass = effect.CurrentTechnique.Passes["Point4"];
			Point2DepthPass = effect.CurrentTechnique.Passes["Point2Depth"];
			Point3DepthPass = effect.CurrentTechnique.Passes["Point3Depth"];
			Point4DepthPass = effect.CurrentTechnique.Passes["Point4Depth"];
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
			// The width/height of the current input.
			int sourceWidth = context.SourceTexture.Width;
			int sourceHeight = context.SourceTexture.Height;

			// The target width/height.
			int targetWidth = context.Viewport.Width;
			int targetHeight = context.Viewport.Height;

			// Save original target/viewport
			var originalTarget = context.RenderTarget;
			var originalViewport = context.Viewport;

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
						context.RenderTarget = originalTarget;
						context.Viewport = originalViewport;
					}
					else
					{
						// Get temporary render target for intermediate steps.
						var tempFormat = new RenderTargetFormat(tempTargetWidth, tempTargetHeight, false, context.SourceTexture.Format, DepthFormat.None);
						temp = context.RenderTargetPool.Obtain2D(tempFormat);
						context.RenderTarget = temp;
					}

					_effect.SourceSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
					_effect.TargetSizeParameter.SetValue(new Vector2(tempTargetWidth, tempTargetHeight));
					_effect.SourceTextureParameter.SetValue(last ?? context.SourceTexture);

					EffectPass pass = null;
					if (factor == 2)
					{
						pass = _effect.Linear2Pass;
					}
					else if (factor == 4)
					{
						pass = _effect.Linear4Pass;
					}
					else if (factor == 6)
					{
						pass = _effect.Linear6Pass;
					}
					else if (factor == 8)
					{
						pass = _effect.Linear8Pass;
					}

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
						context.RenderTarget = originalTarget;
						context.Viewport = originalViewport;
					}
					else
					{
						// Get temporary render target for intermediate steps.
						var tempFormat = new RenderTargetFormat(tempTargetWidth, tempTargetHeight, false, context.SourceTexture.Format, DepthFormat.None);
						temp = context.RenderTargetPool.Obtain2D(tempFormat);
						context.RenderTarget = temp;
					}

					_effect.SourceSizeParameter.SetValue(new Vector2(sourceWidth, sourceHeight));
					_effect.TargetSizeParameter.SetValue(new Vector2(tempTargetWidth, tempTargetHeight));
					var source = last ?? context.SourceTexture;
					_effect.SourceTextureParameter.SetValue(source);

					EffectPass pass = null;
					if (source != context.GBuffer0)
					{
						if (factor == 2)
						{
							pass = _effect.Point2Pass;
						}
						else if (factor == 3)
						{
							pass = _effect.Point3Pass;
						}
						else
						{
							pass = _effect.Point4Pass;
						}
					}
					else
					{
						// This is the depth buffer and it needs special handling.
						if (factor == 2)
						{
							pass = _effect.Point2DepthPass;
						}
						else if (factor == 3)
						{
							pass = _effect.Point3DepthPass;
						}
						else
						{
							pass = _effect.Point4DepthPass;
						}
					}

					context.DrawFullScreenQuad(pass);

					context.RenderTargetPool.Recycle(last);
					last = temp;
					sourceWidth = tempTargetWidth;
					sourceHeight = tempTargetHeight;
				} while (sourceWidth > targetWidth || sourceHeight > targetHeight);

				_effect.SourceTextureParameter.SetValue((Texture2D)null);

				Debug.Assert(last == null, "Intermediate render target should have been recycled.");
			}
		}
	}
}
