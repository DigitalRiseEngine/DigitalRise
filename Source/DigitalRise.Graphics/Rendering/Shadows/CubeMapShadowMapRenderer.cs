// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Data.Lights;
using DigitalRise.Data.Shadows;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Misc;
using DigitalRise.Rendering.Deferred;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Queries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Shadows
{
	/// <summary>
	/// Creates the shadow map of a <see cref="CubeMapShadow"/>.
	/// </summary>
	/// <inheritdoc cref="ShadowMapRenderer"/>
	public static class CubeMapShadowMapRenderer
	{
		//--------------------------------------------------------------
		#region Constants
		//--------------------------------------------------------------

		private static readonly CubeMapFace[] CubeMapFaces =
		{
		  CubeMapFace.PositiveX, CubeMapFace.NegativeX,
		  CubeMapFace.PositiveY, CubeMapFace.NegativeY,
		  CubeMapFace.PositiveZ, CubeMapFace.NegativeZ
		};

		// Note: Cube map faces are left-handed! Therefore +Z is actually -Z.
		private static readonly Vector3[] CubeMapForwardVectors =
		{
		  Vector3.UnitX, -Vector3.UnitX,
		  Vector3.UnitY, -Vector3.UnitY,
		  -Vector3.UnitZ, Vector3.UnitZ   // Switch Z because cube maps are left handed
		};

		private static readonly Vector3[] CubeMapUpVectors =
		{
		  Vector3.UnitY, Vector3.UnitY,
		  Vector3.UnitZ, -Vector3.UnitZ,
		  Vector3.UnitY, Vector3.UnitY
		};


		#endregion


		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly CameraNode _perspectiveCameraNode = new CameraNode(new PerspectiveViewVolume());
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

			context.ThrowIfCameraMissing();
			context.ThrowIfSceneMissing();

			var originalRenderTarget = context.RenderTarget;
			var originalViewport = context.Viewport;
			var originalReferenceNode = context.ReferenceNode;

			var cameraNode = context.CameraNode;

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			cameraNode.LastFrame = frame;

			// The scene node renderer should use the light camera instead of the player camera.
			context.CameraNode = _perspectiveCameraNode;
			context.Technique = "Omnidirectional";

			var graphicsDevice = DR.GraphicsDevice;
			var renderTargetPool = context.RenderTargetPool;
			var savedRenderState = new RenderStateSnapshot();

			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var shadow = lightNode.Shadow as CubeMapShadow;
				if (shadow == null)
					continue;

				var light = lightNode.Light as PointLight;
				if (light == null)
					throw new GraphicsException("CubeMapShadow can only be used with a PointLight.");

				// LightNode is visible in current frame.
				lightNode.LastFrame = frame;

				if (shadow.ShadowMap == null)
				{
					shadow.ShadowMap = renderTargetPool.ObtainCube(
					  new RenderTargetFormat(
						shadow.PreferredSize,
						null,
						false,
						shadow.Prefer16Bit ? SurfaceFormat.HalfSingle : SurfaceFormat.Single,
						DepthFormat.Depth24));
				}

				((PerspectiveViewVolume)_perspectiveCameraNode.ViewVolume).SetFieldOfView(90, 1, shadow.Near, light.Range);

				// World units per texel at a planar distance of 1 world unit.
				float unitsPerTexel = _perspectiveCameraNode.ViewVolume.Rectangle.Width / (shadow.ShadowMap.Size * shadow.Near);

				// Convert depth bias from "texel" to  world space.
				// Minus to move receiver closer to light.
				shadow.EffectiveDepthBias = -shadow.DepthBias * unitsPerTexel;

				// Convert normal offset from "texel" to world space.
				shadow.EffectiveNormalOffset = shadow.NormalOffset * unitsPerTexel;

				var pose = lightNode.PoseWorld;

				context.ReferenceNode = lightNode;
				context.Object = shadow;

				bool shadowMapContainsSomething = false;
				for (int side = 0; side < 6; side++)
				{
					context.SetRenderTargetCube(shadow.ShadowMap, CubeMapFaces[side]);
					// context.RenderTarget = shadow.ShadowMap;   // TODO: Support cube maps targets in the render context.
					context.Viewport = graphicsDevice.Viewport;

					context.Clear(Color.White);

					_perspectiveCameraNode.View = Matrix44F.CreateLookAt(
					  pose.Position,
					  pose.ToWorldPosition(CubeMapForwardVectors[side]),
					  pose.ToWorldDirection(CubeMapUpVectors[side]));

					// Abort if this cube map frustum does not touch the camera frustum.
					if (!context.Scene.HaveContact(cameraNode, _perspectiveCameraNode))
						continue;

					graphicsDevice.DepthStencilState = DepthStencilState.Default;
					graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
					graphicsDevice.BlendState = BlendState.Opaque;

					var shadowQuery = context.Scene.Query<ShadowCasterQuery>(_perspectiveCameraNode, context);
					if (shadowQuery.ShadowCasters.Count > 0)
					{
						MeshRenderer.Render(context, RenderPass.ShadowMap, shadowQuery.ShadowCasters);
						shadowMapContainsSomething = true;
					}
				}

				// Recycle shadow map if empty.
				if (!shadowMapContainsSomething)
				{
					renderTargetPool.Recycle(shadow.ShadowMap);
					shadow.ShadowMap = null;
				}
			}

			savedRenderState.Restore();

			context.CameraNode = cameraNode;
			context.Technique = null;
			context.RenderTarget = originalRenderTarget;
			context.Viewport = originalViewport;
			context.ReferenceNode = originalReferenceNode;
			context.Object = null;
		}
		#endregion
	}
}
