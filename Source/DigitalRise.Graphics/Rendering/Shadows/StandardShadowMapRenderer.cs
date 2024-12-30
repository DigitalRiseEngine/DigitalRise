using DigitalRise.Data.Lights;
using DigitalRise.Data.Shadows;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework;
using DigitalRise.Rendering.Deferred;
using DigitalRise.SceneGraph.Queries;

namespace DigitalRise.Rendering.Shadows
{
	public static class StandardShadowMapRenderer
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly CameraNode _perspectiveCameraNode = new CameraNode(new PerspectiveViewVolume());
		private static readonly CameraNode _orthographicCameraNode = new CameraNode(new OrthographicViewVolume());
		#endregion

		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		public static void Render(RenderContext context, IList<SceneNode> nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");
			if (context == null)
				throw new ArgumentNullException("context");

			int numberOfNodes = nodes.Count;
			if (numberOfNodes == 0)
				return;

			// Note: The camera node is not used by the StandardShadowMapRenderer.
			// Still throw an exception if null for consistency. (All other shadow map
			// renderers need a camera node.)
			context.ThrowIfCameraMissing();
			context.ThrowIfSceneMissing();

			var originalRenderTarget = context.RenderTarget;
			var originalViewport = context.Viewport;
			var originalReferenceNode = context.ReferenceNode;

			var cameraNode = context.CameraNode;

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			cameraNode.LastFrame = frame;

			context.Technique = "Default";

			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();

			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var shadow = lightNode.Shadow as StandardShadow;
				if (shadow == null)
					continue;

				// LightNode is visible in current frame.
				lightNode.LastFrame = frame;

				// Get a new shadow map if necessary.
				if (shadow.ShadowMap == null)
				{
					shadow.ShadowMap = context.RenderTargetPool.Obtain2D(
					  new RenderTargetFormat(
						shadow.PreferredSize,
						shadow.PreferredSize,
						false,
						shadow.Prefer16Bit ? SurfaceFormat.HalfSingle : SurfaceFormat.Single,
						DepthFormat.Depth24));
				}

				// Create a suitable shadow camera.
				CameraNode lightCameraNode;
				if (lightNode.Light is ProjectorLight)
				{
					var light = (ProjectorLight)lightNode.Light;
					if (light.Projection is PerspectiveViewVolume)
					{
						var lp = (PerspectiveViewVolume)light.Projection;
						var cp = (PerspectiveViewVolume)_perspectiveCameraNode.ViewVolume;
						cp.SetFieldOfView(lp.FieldOfViewY, lp.AspectRatio, lp.Near, lp.Far);

						lightCameraNode = _perspectiveCameraNode;
					}
					else //if (light.Projection is OrthographicViewVolume)
					{
						var lp = (OrthographicViewVolume)light.Projection;
						var cp = (OrthographicViewVolume)_orthographicCameraNode.ViewVolume;
						cp.SetOffCenter(lp.Left, lp.Right, lp.Bottom, lp.Top, lp.Near, lp.Far);

						lightCameraNode = _orthographicCameraNode;
					}
				}
				else if (lightNode.Light is Spotlight)
				{
					var light = (Spotlight)lightNode.Light;
					var cp = (PerspectiveViewVolume)_perspectiveCameraNode.ViewVolume;
					cp.SetFieldOfView(2 * light.CutoffAngle, 1, shadow.DefaultNear, light.Range);

					lightCameraNode = _perspectiveCameraNode;
				}
				else
				{
					throw new GraphicsException("StandardShadow can only be used with a Spotlight or a ProjectorLight.");
				}

				lightCameraNode.PoseWorld = lightNode.PoseWorld;

				// Store data for use in StandardShadowMaskRenderer.
				shadow.Near = lightCameraNode.ViewVolume.Near;
				shadow.Far = lightCameraNode.ViewVolume.Far;
				shadow.View = lightCameraNode.PoseWorld.Inverse;
				shadow.Projection = (Matrix)lightCameraNode.ViewVolume.Projection;

				// World units per texel at a planar distance of 1 world unit.
				float unitsPerTexel = lightCameraNode.ViewVolume.Rectangle.Width / (shadow.ShadowMap.Height * shadow.Near);

				// Convert depth bias from "texel" to world space.
				// Minus to move receiver depth closer to light.
				shadow.EffectiveDepthBias = -shadow.DepthBias * unitsPerTexel;

				// Convert normal offset from "texel" to world space.
				shadow.EffectiveNormalOffset = shadow.NormalOffset * unitsPerTexel;

				context.RenderTarget = shadow.ShadowMap;
				context.Clear(Color.White);

				// The scene node renderer should use the light camera instead of the player camera.
				context.CameraNode = lightCameraNode;
				context.ReferenceNode = lightNode;
				context.Object = shadow;

				graphicsDevice.DepthStencilState = DepthStencilState.Default;
				graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
				graphicsDevice.BlendState = BlendState.Opaque;

				bool shadowMapContainsSomething = false;

				var shadowQuery = context.Scene.Query<ShadowCasterQuery>(context.CameraNode, context);
				if (shadowQuery.ShadowCasters.Count > 0)
				{
					MeshRenderer.Render(context, RenderPass.ShadowMap, shadowQuery.ShadowCasters);
					shadowMapContainsSomething = true;
				}

				if (!shadowMapContainsSomething)
				{
					// Shadow map is empty. Recycle it.
					context.RenderTargetPool.Recycle(shadow.ShadowMap);
					shadow.ShadowMap = null;
				}
			}

			savedRenderState.Restore();

			context.Technique = null;
			context.RenderTarget = originalRenderTarget;
			context.Viewport = originalViewport;
			context.ReferenceNode = originalReferenceNode;
			context.Object = null;
		}
		#endregion
	}
}
