using System.Collections.Generic;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	// The type of ambient occlusion.
	public enum AmbientOcclusionType
	{
		None,   // No ambient occlusion
		SSAO,   // Using SsaoFilter (Screen Space Ambient Occlusion)
		SAO,    // Using SaoFilter (Scalable Ambient Obscurance)
	}


	// This renderer renders light nodes and creates the light buffer which stores 
	// the accumulated diffuse and specular light intensities. 
	// The light buffer is stored in the render context. It can be used by the 
	// following renderers (usually by the "Material" pass), and it must be recycled 
	// by the graphics screen.
	public static class LightBufferRenderer
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		/*		private static PostProcessor _ssaoFilter;
				private static int _ssaoDownsampleFactor;
				private static readonly CopyFilter _copyFilter;*/

		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		// Get/set ambient occlusion type and the _ssaoFilter.
		/*		public static AmbientOcclusionType AmbientOcclusionType
				{
					get { return _ambientOcclusionType; }
					set
					{
						if (_ambientOcclusionType == value)
							return;

						if (_ssaoFilter != null)
							_ssaoFilter.Dispose();

						_ambientOcclusionType = value;

						switch (_ambientOcclusionType)
						{
							case AmbientOcclusionType.None:
								_ssaoFilter = null;
								break;
							case AmbientOcclusionType.SSAO:
								_ssaoDownsampleFactor = 2;
								_ssaoFilter = new SsaoFilter(_graphicsService)
								{
									DownsampleFactor = _ssaoDownsampleFactor,

									// Normally the SsaoFilter applies the occlusion values directly to the 
									// source texture. But here the filter should ignore the input image and 
									// create a grayscale image (white = no occlusion, black = max occlusion).
									CombineWithSource = false,
								};

								break;
							case AmbientOcclusionType.SAO:
								_ssaoDownsampleFactor = 1;
								_ssaoFilter = new SaoFilter(_graphicsService)
								{
									// Normally the SaoFilter applies the occlusion values directly to the 
									// source texture. But here the filter should ignore the input image and 
									// create a grayscale image (white = no occlusion, black = max occlusion).
									CombineWithSource = false,
								};
								break;
						}

						_ambientOcclusionType = value;
					}
				}
				private static AmbientOcclusionType _ambientOcclusionType;*/
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/*		public LightBufferRenderer(IGraphicsService graphicsService)
				{
					_graphicsService = graphicsService;
					LightRenderer = new LightRenderer(graphicsService);
					AmbientOcclusionType = AmbientOcclusionType.SSAO;
					_copyFilter = new CopyFilter(graphicsService);
				}


				public void Dispose()
				{
					if (!_disposed)
					{
						_disposed = true;
						LightRenderer.Dispose();
						_ssaoFilter.SafeDispose();
						_copyFilter.Dispose();
					}
				}*/
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		public static void Render(RenderContext context, IList<SceneNode> lights)
		{
			var graphicsDevice = DR.GraphicsDevice;
			var viewport = graphicsDevice.Viewport;
			var width = viewport.Width;
			var height = viewport.Height;

			RenderTarget2D aoRenderTarget = null;
			/*			if (_ssaoFilter != null)
						{
							// Render ambient occlusion info into a render target.
							aoRenderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(
							  width / _ssaoDownsampleFactor,
							  height / _ssaoDownsampleFactor,
							  false,
							  SurfaceFormat.Color,
							  DepthFormat.None));

							// PostProcessors require that context.SourceTexture is set. But since 
							// _ssaoFilter.CombineWithSource is set to false, the SourceTexture is not 
							// used and we can set it to anything except null.
							context.SourceTexture = aoRenderTarget;
							context.RenderTarget = aoRenderTarget;
							context.Viewport = new Viewport(0, 0, aoRenderTarget.Width, aoRenderTarget.Height);
							_ssaoFilter.Process(context);
							context.SourceTexture = null;
						}*/

			graphicsDevice.SetRenderTargets(context.LightBuffer0, context.LightBuffer1);

			// Clear the light buffer. (The alpha channel is not used. We can set it to anything.)
			graphicsDevice.Clear(new Color(0, 0, 0, 255));

			// Restore the depth buffer (which XNA destroys in SetRenderTarget).
			// (This is only needed if lights can use a clip geometry (LightNode.Clip).)
			RebuildZBufferRenderer.Render(context, true);

			// Render all lights into the light buffers.
			AmbientLightRenderer.Render(context, lights);
			DirectionalLightRenderer.Render(context, lights);
			PointLightRenderer.Render(context, lights);
			ProjectorLightRenderer.Render(context, lights);

			/*			if (aoRenderTarget != null)
						{
							// Render the ambient occlusion texture using multiplicative blending.
							// This will darken the light buffers depending on the ambient occlusion term.
							// Note: Theoretically, this should be done after the ambient light renderer 
							// and before the directional light renderer because AO should not affect 
							// directional lights. But doing this here has more impact.
							context.SourceTexture = aoRenderTarget;
							graphicsDevice.BlendState = GraphicsHelper.BlendStateMultiply;
							_copyFilter.Process(context);
						}

						// Clean up.
						graphicsService.RenderTargetPool.Recycle(aoRenderTarget);
						context.SourceTexture = null;
						context.RenderTarget = target;
						context.Viewport = viewport;

						_renderTargetBindings[0] = new RenderTargetBinding();
						_renderTargetBindings[1] = new RenderTargetBinding();*/

			graphicsDevice.ResetRenderTargets();
		}
		#endregion
	}
}
