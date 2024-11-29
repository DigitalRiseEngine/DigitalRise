using System.Collections.Generic;
using DigitalRise.Misc;
using DigitalRise.PostProcessing.Processing;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	// This renderer renders opaque geometry (usually the opaque submeshes of MeshNodes) 
	// and creates the G-Buffer which stores the depth values, normal vectors and other information.
	// The G-Buffer is stored in the render context. It can be used by the following
	// renderers (e.g. by the LightBufferRenderer or the post-processors), and it must
	// be recycled by the graphics screen.
	public static class GBufferRenderer
	{
		private static readonly DownsampleFilter _downsampleFilter = new DownsampleFilter();

		// Pre-allocated data structures to avoid allocations at runtime.

		public static void Render(RenderContext context, IList<SceneNode> sceneNodes, IList<SceneNode> decalNodes)
		{
			var graphicsDevice = DR.GraphicsDevice;
			var viewport = graphicsDevice.Viewport;

			// The G-buffer consists of two full-screen render targets into which we render 
			// depth values, normal vectors and other information.
			var width = viewport.Width;
			var height = viewport.Height;

			// Set the device render target to the G-buffer.
			graphicsDevice.SetRenderTargets(context.GBuffer0, context.GBuffer1);

			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.BlendState = BlendState.Opaque;

			// Clear the z-buffer.
			graphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);

			// Initialize the G-buffer with default values. 
			ClearGBufferRenderer.Render(context);

			// Render the scene nodes using the "GBuffer" material pass.
			graphicsDevice.DepthStencilState = DepthStencilState.Default;
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.BlendState = BlendState.Opaque;
			MeshRenderer.Render(context, RenderPass.GBuffer, sceneNodes);

			/*			if (_decalRenderer != null && decalNodes.Count > 0)
						{
							// Render decal nodes using the "GBuffer" material pass.
							// Decals are rendered as "deferred decals". The geometry information is 
							// read from GBuffer0 and the decal normals are blended with GBuffer1, which
							// has to be set as the first render target. (That means a new GBuffer1 is 
							// created. The original GBuffer1 is recycled afterwards.)
							var renderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(width, height, false, SurfaceFormat.Color, DepthFormat.None));
							graphicsDevice.SetRenderTarget(renderTarget);
							context.RenderTarget = renderTarget;

							// Copy GBuffer1 to current render target and restore the depth buffer.
							var rebuildZBufferRenderer = (RebuildZBufferRenderer)context.Data[RenderContextKeys.RebuildZBufferRenderer];
							rebuildZBufferRenderer.Render(context, context.GBuffer1);

							// Blend decals with the render target.
							_decalRenderer.Render(decalNodes, context);

							// The new render target replaces the GBuffer1.
							renderTargetPool.Recycle(context.GBuffer1);
							context.GBuffer1 = renderTarget;
						}*/

			// The depth buffer is downsampled into a buffer of half width and half height.
			var depthBufferHalf = context.DepthBufferHalf;
			context.SourceTexture = context.GBuffer0;

			graphicsDevice.SetRenderTarget(depthBufferHalf);
			_downsampleFilter.Process(context);
			context.SourceTexture = null;

			graphicsDevice.ResetRenderTargets();
		}
	}
}
