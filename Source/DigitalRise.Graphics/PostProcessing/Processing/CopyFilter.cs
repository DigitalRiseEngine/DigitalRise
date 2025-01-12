using DigitalRise.Data.Materials;
using DigitalRise.Misc;
using DigitalRise.PostProcessing;
using DigitalRise.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Graphics.PostProcessing.Processing
{
	internal class CopyFilterEffectBinding : EffectWrapper
	{
		public EffectParameter SourceTexture { get; private set; }
		public EffectParameter ViewportSize { get; private set; }

		public CopyFilterEffectBinding() : base("PostProcessing/CopyFilter")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			SourceTexture = effect.Parameters["SourceTexture"];
			ViewportSize = effect.Parameters["ViewportSize"];
		}
	}

	/// <summary>
	/// Copies a texture into a render target.
	/// </summary>
	public class CopyFilter : PostProcessor
	{
		private readonly CopyFilterEffectBinding _effect = new CopyFilterEffectBinding();


		/// <inheritdoc/>
		protected override void OnProcess(RenderContext context)
		{
			var graphicsDevice = DR.GraphicsDevice;

			// Set sampler state. (Floating-point textures cannot use linear filtering. (XNA would throw an exception.))
			if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
				graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			else
				graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

			// Set the render target - but only if no kind of alpha blending is currently set.
			// If alpha-blending is set, then we have to assume that the render target is already
			// set - everything else does not make sense.
			if (graphicsDevice.BlendState.ColorDestinationBlend == Blend.Zero
				&& graphicsDevice.BlendState.AlphaDestinationBlend == Blend.Zero)
			{
				graphicsDevice.SetRenderTarget(context.RenderTarget);
				graphicsDevice.Viewport = context.Viewport;
			}

			_effect.ViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
			_effect.SourceTexture.SetValue(context.SourceTexture);
			_effect.CurrentTechnique.Passes[0].Apply();
			context.DrawFullScreenQuad(_effect.CurrentTechnique.Passes[0]);

			_effect.SourceTexture.SetValue((Texture2D)null);
		}
	}
}
