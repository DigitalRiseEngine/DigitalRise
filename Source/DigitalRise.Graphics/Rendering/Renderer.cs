// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Geometry.Shapes;
using DigitalRise.Misc;
using DigitalRise.Rendering.Billboards;
using DigitalRise.Rendering.Deferred;
using DigitalRise.Rendering.Shadows;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Queries;
using DigitalRise.SceneGraph.Scenes;
using DigitalRune.Rendering.Sky;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DigitalRise.Rendering
{
	public class Renderer
	{
		private readonly RenderContext _context = new RenderContext();
		private BillboardRenderer _billboardRenderer;

		public bool EnableLod { get; set; }

		public RenderTarget2D GBuffer0 => _context.GBuffer0;
		public RenderTarget2D GBuffer1 => _context.GBuffer1;
		public RenderTarget2D LightBuffer0 => _context.LightBuffer0;
		public RenderTarget2D LightBuffer1 => _context.LightBuffer1;

		public RenderStatistics Statistics => _context.Statistics;

		/// <summary>
		/// Returns <see langword="true"/> if the given processor is the last processor that renders 
		/// into the back buffer. 
		/// </summary>
		private static bool IsLastOutputProcessor(IList<PostProcessorNode> processors, int processorIndex)
		{
			// Return false if there is a post-processor after the given index which is enabled.
			int numberOfProcessors = processors.Count;
			for (int i = processorIndex + 1; i < numberOfProcessors; i++)
			{
				var processor = processors[i];
				if (processor.Processor.Enabled)
					return false;
			}

			return true;
		}


		private void PostProcess(List<PostProcessorNode> processorNodes)
		{
			if (processorNodes == null || processorNodes.Count == 0)
			{
				return;
			}

			Debug.Assert(_context != null);
			Debug.Assert(_context.SourceTexture != null);

			if (_context == null)
				throw new ArgumentNullException("context");

			var renderTargetPool = _context.RenderTargetPool;
			var graphicsDevice = DR.GraphicsDevice;

			var originalSourceTexture = _context.SourceTexture;
			var originalRenderTarget = _context.RenderTarget;
			var originalViewport = _context.Viewport;

			// Some normal post-processors can be used with any blend state. In a chain
			// alpha blending does not make sense.
			graphicsDevice.BlendState = BlendState.Opaque;

			// Intermediate render targets for ping-ponging.
			// TODO: Use the originalRenderTarget in the ping-ponging.
			// (Currently, we create up to 2 temp targets. If the originalRenderTarget is not null,
			// and if the viewport is the whole target, then we could use the originalRenderTarget
			// in the ping-ponging. But care must be taken that the originalRenderTarget is never
			// used as the output for the post-processor before the last post-processor...)
			RenderTarget2D tempSource = null;
			RenderTarget2D tempTarget = null;

			// The size and format for intermediate render target is determined by the source image.
			var tempFormat = new RenderTargetFormat(originalSourceTexture)
			{
				Mipmap = false,
				DepthStencilFormat = DepthFormat.None,
			};

			// Remember if any processor has written into target.
			bool targetWritten = false;

			// Execute all processors.
			var numberOfProcessors = processorNodes.Count;
			for (int i = 0; i < numberOfProcessors; i++)
			{
				var processor = processorNodes[i].Processor;
				if (!processor.Enabled)
					continue;

				// Find effective output target:
				// If this processor is the last, then we render into the user-defined target. 
				// If this is not the last processor, then we use an intermediate buffer.
				if (IsLastOutputProcessor(processorNodes, i))
				{
					_context.RenderTarget = originalRenderTarget;
					_context.Viewport = originalViewport;
					targetWritten = true;
				}
				else
				{
					// This is an intermediate post-processor, so we need an intermediate target.
					// If we have one, does it still have the correct format? If not, recycle it.
					if (tempTarget != null && !processor.DefaultTargetFormat.IsCompatibleWith(tempFormat))
					{
						renderTargetPool.Recycle(tempTarget);
						tempTarget = null;
					}

					if (tempTarget == null)
					{
						// Get a new render target. 
						// The format that the processor wants has priority. The current format 
						// is the fallback.
						tempFormat = new RenderTargetFormat(
						  processor.DefaultTargetFormat.Width ?? tempFormat.Width,
						  processor.DefaultTargetFormat.Height ?? tempFormat.Height,
						  processor.DefaultTargetFormat.Mipmap ?? tempFormat.Mipmap,
						  processor.DefaultTargetFormat.SurfaceFormat ?? tempFormat.SurfaceFormat,
						  processor.DefaultTargetFormat.DepthStencilFormat ?? tempFormat.DepthStencilFormat);
						tempTarget = renderTargetPool.Obtain2D(tempFormat);
					}

					_context.RenderTarget = tempTarget;
					_context.Viewport = new Viewport(0, 0, tempFormat.Width.Value, tempFormat.Height.Value);
				}

				processor.ProcessInternal(_context);

				_context.SourceTexture = _context.RenderTarget;

				// If we have rendered into tempTarget, then we remember it in tempSource 
				// and reuse the render target in tempSource if any is set.
				if (_context.RenderTarget == tempTarget)
					Mathematics.MathHelper.Swap(ref tempSource, ref tempTarget);
			}

			// If there are no processors, or no processor is enabled, then we have to 
			// copy the source to the target manually.
			/*			if (!targetWritten)
							graphicsService.GetCopyFilter().ProcessInternal(_context);*/

			_context.SourceTexture = originalSourceTexture;

			// The last processor should have written into the original target.
			Debug.Assert(_context.RenderTarget == originalRenderTarget);

			renderTargetPool.Recycle(tempSource);
			renderTargetPool.Recycle(tempTarget);
		}

		public RenderTarget2D Render(Scene scene, CameraNode camera, GameTime gameTime, Point? size = null, Action<RenderContext> postRender = null)
		{
			var graphicsDevice = DR.GraphicsDevice;

			var oldViewport = graphicsDevice.Viewport;

			RenderTarget2D result = null;
			try
			{
				scene.Update(gameTime.ElapsedGameTime);

				foreach (var updateable in scene.UpdateableNodes)
				{
					updateable.Update(gameTime);
				}

				var viewSize = size ?? new Point(oldViewport.Width, oldViewport.Height);
				_context.Prepare(viewSize);

				// Our scene and the camera must be set in the render context. This info is
				// required by many renderers.
				_context.Scene = scene;
				_context.CameraNode = camera;

				// Update aspect
				var asPerspective = _context.CameraNode.ViewVolume as PerspectiveViewVolume;
				if (asPerspective != null)
				{
					asPerspective.AspectRatio = _context.AspectRatio;
				}

				// LOD (level of detail) settings are also specified in the context.
				_context.LodCameraNode = camera;
				_context.LodHysteresis = 0.5f;
				_context.LodBias = EnableLod ? 1.0f : 0.0f;
				_context.LodBlendingEnabled = false;

				// ----- Scene Rendering
				// Get all scene nodes which overlap the camera frustum.
				CustomSceneQuery sceneQuery = scene.Query<CustomSceneQuery>(_context.CameraNode, _context);

				// ----- G-Buffer Pass
				// The GBufferRenderer creates context.GBuffer0 and context.GBuffer1.
				GBufferRenderer.Render(_context, sceneQuery.RenderableNodes, sceneQuery.DecalNodes);

				// ----- Shadow Pass
				// The ShadowMapRenderer renders the shadow maps which are stored in the light nodes.
				CascadedShadowMapRenderer.Render(_context, sceneQuery.Lights);
				CubeMapShadowMapRenderer.Render(_context, sceneQuery.Lights);
				StandardShadowMapRenderer.Render(_context, sceneQuery.Lights);

				// The ShadowMaskRenderer renders the shadows and stores them in one or more render
				// targets ("shadows masks").
				ShadowMaskRenderer.Render(_context, sceneQuery.Lights);

				// ----- Light Buffer Pass
				// The LightBufferRenderer creates context.LightBuffer0 (diffuse light) and
				// context.LightBuffer1 (specular light).
				LightBufferRenderer.Render(_context, sceneQuery.Lights);

				// Material pass
				_context.RenderTarget = _context.Output;
				_context.Clear(new Color(3, 3, 3, 255));
				graphicsDevice.DepthStencilState = DepthStencilState.Default;
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				graphicsDevice.BlendState = BlendState.Opaque;
				MeshRenderer.Render(_context, RenderPass.Material, sceneQuery.RenderableNodes);
				// _decalRenderer.Render(sceneQuery.DecalNodes, _context);

				// ----- Forward Rendering of Alpha-Blended Meshes and Particles
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				graphicsDevice.BlendState = BlendState.AlphaBlend;

				if (_billboardRenderer == null)
				{
					_billboardRenderer = new BillboardRenderer(2048);
				}
				_billboardRenderer.Render(_context, sceneQuery.RenderableNodes, RenderOrder.BackToFront);
				graphicsDevice.ResetTextures();

				_context.SourceTexture = null;

				// ----- Sky
				SkyboxRendererInternal.Render(_context, sceneQuery.RenderableNodes);

				// ----- Post Processors
				if (sceneQuery.PostProcessorNodes.Count > 0)
				{
					_context.SourceTexture = _context.Output;
					_context.RenderTarget = _context.Output2;
					PostProcess(sceneQuery.PostProcessorNodes);
				}

				result = _context.RenderTarget;
				
				if (DRDebugOptions.VisualizeBuffers)
				{
					var spriteBatch = Resources.SpriteBatch;

					spriteBatch.Begin();

					// G Buffers
					spriteBatch.Draw(GBuffer0, new Rectangle(0, 0, 256, 256), Color.White);
					spriteBatch.Draw(GBuffer1, new Rectangle(256, 0, 256, 256), Color.White);

					// Shadow map
					var lightNode = scene.GetDescendants().OfType<LightNode>().Where(n => n.Shadow != null && n.Shadow.ShadowMap != null).FirstOrDefault();
					if (lightNode != null)
					{
						spriteBatch.Draw((Texture2D)lightNode.Shadow.ShadowMap, new Rectangle(512, 0, 512, 256), Color.White);

						if (lightNode.Shadow.ShadowMask != null)
						{
							spriteBatch.Draw(lightNode.Shadow.ShadowMask, new Rectangle(0, 256, 256, 256), Color.White);
						}
					}

					spriteBatch.Draw(LightBuffer0, new Rectangle(256, 256, 256, 256), Color.White);
					spriteBatch.Draw(LightBuffer1, new Rectangle(512, 256, 256, 256), Color.White);

					spriteBatch.End();
				}

				if (postRender != null)
				{
					postRender(_context);
				}
			}
			finally
			{
				// ----- Clean-up
				graphicsDevice.SetRenderTarget(null);
				graphicsDevice.Viewport = oldViewport;
				_context.Scene = null;
				_context.CameraNode = null;
				_context.LodCameraNode = null;
				_context.LodHysteresis = 0;

				ShadowMaskRenderer.RecycleShadowMasks(_context);
			}

			return result;
		}
	}
}
