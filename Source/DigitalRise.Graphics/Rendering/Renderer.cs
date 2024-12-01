using DigitalRise.Geometry.Shapes;
using DigitalRise.Misc;
using DigitalRise.Rendering.Billboards;
using DigitalRise.Rendering.Deferred;
using DigitalRise.Rendering.Shadows;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Scenes;
using DigitalRune.Rendering.Sky;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace DigitalRise.Rendering
{
	public class Renderer
	{
		private readonly RenderContext _context = new RenderContext();
		private readonly BillboardRenderer _billboardRenderer = new BillboardRenderer(2048);

		public bool EnableLod { get; set; }

		public RenderTarget2D GBuffer0 => _context.GBuffer0;
		public RenderTarget2D GBuffer1 => _context.GBuffer1;
		public RenderTarget2D LightBuffer0 => _context.LightBuffer0;
		public RenderTarget2D LightBuffer1 => _context.LightBuffer1;

		public RenderStatistics Statistics => _context.Statistics;


		public RenderTarget2D Render(Scene scene, GameTime gameTime, Action<RenderContext> postRender = null)
		{
			var oldViewport = DR.GraphicsDevice.Viewport;

			try
			{
				foreach (var updateable in scene.UpdateableNodes)
				{
					updateable.Update(gameTime);
				}

				_context.Prepare();

				// Our scene and the camera must be set in the render context. This info is
				// required by many renderers.
				_context.Scene = scene;
				_context.CameraNode = scene.Camera;

				// Update aspect
				var asPerspective = _context.CameraNode.ViewVolume as PerspectiveViewVolume;
				if (asPerspective != null)
				{
					asPerspective.AspectRatio = oldViewport.AspectRatio;
				}

				// LOD (level of detail) settings are also specified in the context.
				_context.LodCameraNode = scene.Camera;
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

				// The ShadowMaskRenderer renders the shadows and stores them in one or more render
				// targets ("shadows masks").
				DR.GraphicsDevice.Viewport = oldViewport;
				ShadowMaskRenderer.Render(_context, sceneQuery.Lights);

				// ----- Light Buffer Pass
				// The LightBufferRenderer creates context.LightBuffer0 (diffuse light) and
				// context.LightBuffer1 (specular light).
				LightBufferRenderer.Render(_context, sceneQuery.Lights);

				// Material pass
				var graphicsDevice = DR.GraphicsDevice;
				graphicsDevice.SetRenderTarget(_context.Output);
				graphicsDevice.Clear(new Color(3, 3, 3, 255));
				graphicsDevice.DepthStencilState = DepthStencilState.Default;
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				graphicsDevice.BlendState = BlendState.Opaque;
				MeshRenderer.Render(_context, RenderPass.Material, sceneQuery.RenderableNodes);
				// _decalRenderer.Render(sceneQuery.DecalNodes, _context);

				// ----- Forward Rendering of Alpha-Blended Meshes and Particles
				graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				graphicsDevice.BlendState = BlendState.AlphaBlend;
				_billboardRenderer.Render(_context, sceneQuery.RenderableNodes, RenderOrder.BackToFront);
				graphicsDevice.ResetTextures();

				_context.SourceTexture = null;

				// ----- Sky
				SkyboxRendererInternal.Render(_context, sceneQuery.RenderableNodes);

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
				DR.GraphicsDevice.SetRenderTarget(null);
				_context.Scene = null;
				_context.CameraNode = null;
				_context.LodCameraNode = null;
				_context.LodHysteresis = 0;

				ShadowMaskRenderer.RecycleShadowMasks(_context);
			}

			return _context.Output;
		}
	}
}
