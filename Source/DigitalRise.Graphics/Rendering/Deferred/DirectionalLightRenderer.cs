// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Geometry;
using DigitalRise.Data.Materials;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DigitalRise.SceneGraph;
using DigitalRise.Misc;

using DirectionalLight = DigitalRise.Data.Lights.DirectionalLight;

namespace DigitalRise.Rendering.Deferred
{
	internal class DirectionalLightEffectWrapper : EffectWrapper
	{
		public EffectParameter WorldViewProjection { get; private set; }
		public EffectParameter ViewportSize { get; private set; }
		public EffectParameter DiffuseColor { get; private set; }
		public EffectParameter SpecularColor { get; private set; }
		public EffectParameter LightDirection { get; private set; }
		public EffectParameter TextureMatrix { get; private set; }
		public EffectParameter Texture { get; private set; }
		public EffectParameter GBuffer0 { get; private set; }
		public EffectParameter GBuffer1 { get; private set; }
		public EffectParameter ShadowMaskChannel { get; private set; }
		public EffectParameter ShadowMask { get; private set; }

		public EffectPass PassClip { get; private set; }
		public EffectPass PassDefault { get; private set; }
		public EffectPass PassShadowed { get; private set; }
		public EffectPass PassTexturedRgb { get; private set; }
		public EffectPass PassTexturedAlpha { get; private set; }
		public EffectPass PassShadowedTexturedRgb { get; private set; }
		public EffectPass PassShadowedTexturedAlpha { get; private set; }

		public static DirectionalLightEffectWrapper Instance { get; } = new DirectionalLightEffectWrapper();

		private DirectionalLightEffectWrapper() : base("Deferred/DirectionalLight")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			WorldViewProjection = effect.Parameters["WorldViewProjection"];
			ViewportSize = effect.Parameters["ViewportSize"];
			DiffuseColor = effect.Parameters["DirectionalLightDiffuse"];
			SpecularColor = effect.Parameters["DirectionalLightSpecular"];
			LightDirection = effect.Parameters["DirectionalLightDirection"];
			TextureMatrix = effect.Parameters["DirectionalLightTextureMatrix"];
			Texture = effect.Parameters["DirectionalLightTexture"];
			GBuffer0 = effect.Parameters["GBuffer0"];
			GBuffer1 = effect.Parameters["GBuffer1"];
			ShadowMaskChannel = effect.Parameters["ShadowMaskChannel"];
			ShadowMask = effect.Parameters["ShadowMask"];
			PassClip = effect.CurrentTechnique.Passes["Clip"];
			PassDefault = effect.CurrentTechnique.Passes["Default"];
			PassShadowed = effect.CurrentTechnique.Passes["Shadowed"];
			PassTexturedRgb = effect.CurrentTechnique.Passes["TexturedRgb"];
			PassTexturedAlpha = effect.CurrentTechnique.Passes["TexturedAlpha"];
			PassShadowedTexturedRgb = effect.CurrentTechnique.Passes["ShadowedTexturedRgb"];
			PassShadowedTexturedAlpha = effect.CurrentTechnique.Passes["ShadowedTexturedAlpha"];
		}
	}

	/// <summary>
	/// Renders <see cref="DirectionalLight"/>s into the light buffer.
	/// </summary>
	public static class DirectionalLightRenderer
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly Vector3[] _cameraFrustumFarCorners = new Vector3[4];

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

			var effect = DirectionalLightEffectWrapper.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

			var viewport = context.Viewport;
			effect.ViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
			effect.GBuffer0.SetValue(context.GBuffer0);
			effect.GBuffer1.SetValue(context.GBuffer1);

			var cameraNode = context.CameraNode;
			Matrix viewProjection = (Matrix)(cameraNode.View * cameraNode.ViewVolume.Projection);

			var cameraPose = cameraNode.PoseWorld;
			GraphicsHelper.GetFrustumFarCorners(cameraNode.ViewVolume, _cameraFrustumFarCorners);

			// Convert frustum far corners from view space to world space.
			for (int i = 0; i < _cameraFrustumFarCorners.Length; i++)
				_cameraFrustumFarCorners[i] = cameraPose.ToWorldDirection(_cameraFrustumFarCorners[i]);

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			cameraNode.LastFrame = frame;

			var isHdrEnabled = context.IsHdr;
			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var light = lightNode.Light as DirectionalLight;
				if (light == null)
					continue;

				// LightNode is visible in current frame.
				lightNode.LastFrame = frame;

				float hdrScale = isHdrEnabled ? light.HdrScale : 1;
				effect.DiffuseColor.SetValue(light.Color.ToVector3() * light.DiffuseIntensity * hdrScale);
				effect.SpecularColor.SetValue(light.Color.ToVector3() * light.SpecularIntensity * hdrScale);

				Pose lightPose = lightNode.PoseWorld;
				Vector3 lightDirectionWorld = lightPose.ToWorldDirection(Vector3.Forward);
				effect.LightDirection.SetValue(lightDirectionWorld);

				bool hasShadow = (lightNode.Shadow != null && lightNode.Shadow.ShadowMask != null);
				if (hasShadow)
				{
					switch (lightNode.Shadow.ShadowMaskChannel)
					{
						case 0: effect.ShadowMaskChannel.SetValue(new Vector4(1, 0, 0, 0)); break;
						case 1: effect.ShadowMaskChannel.SetValue(new Vector4(0, 1, 0, 0)); break;
						case 2: effect.ShadowMaskChannel.SetValue(new Vector4(0, 0, 1, 0)); break;
						default: effect.ShadowMaskChannel.SetValue(new Vector4(0, 0, 0, 1)); break;
					}

					effect.ShadowMask.SetValue(lightNode.Shadow.ShadowMask);
				}

				bool hasTexture = (light.Texture != null);
				if (hasTexture)
				{
					var textureProjection = Matrix44F.CreateOrthographicOffCenter(
					  -light.TextureOffset.X,
					  -light.TextureOffset.X + Math.Abs(light.TextureScale.X),
					  light.TextureOffset.Y,
					  light.TextureOffset.Y + Math.Abs(light.TextureScale.Y),
					  1,  // Not relevant
					  2); // Not relevant.
					var scale = Matrix44F.CreateScale(Math.Sign(light.TextureScale.X), Math.Sign(light.TextureScale.Y), 1);

					effect.TextureMatrix.SetValue((Matrix)(GraphicsHelper.ProjectorBiasMatrix * scale * textureProjection * lightPose.Inverse));

					effect.Texture.SetValue(light.Texture);
				}

				if (lightNode.Clip != null)
				{
					var data = lightNode.RenderData as LightRenderData;
					if (data == null)
					{
						data = new LightRenderData();
						lightNode.RenderData = data;
					}

					data.UpdateClipSubmesh(lightNode);

					graphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateOnePassStencilFail;
					graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;

					effect.WorldViewProjection.SetValue((Matrix)data.ClipMatrix * viewProjection);
					context.Draw(effect.PassClip, data.ClipSubmesh);

					graphicsDevice.DepthStencilState = lightNode.InvertClip
					  ? GraphicsHelper.DepthStencilStateStencilEqual0
					  : GraphicsHelper.DepthStencilStateStencilNotEqual0;
					graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;
				}
				else
				{
					graphicsDevice.DepthStencilState = DepthStencilState.None;
				}

				EffectPass pass;
				if (hasShadow)
				{
					if (hasTexture)
					{
						if (light.Texture.Format == SurfaceFormat.Alpha8)
							pass = effect.PassShadowedTexturedAlpha;
						else
							pass = effect.PassShadowedTexturedRgb;
					}
					else
					{
						pass = effect.PassShadowed;
					}
				}
				else
				{
					if (hasTexture)
					{
						if (light.Texture.Format == SurfaceFormat.Alpha8)
							pass = effect.PassTexturedAlpha;
						else
							pass = effect.PassTexturedRgb;
					}
					else
					{
						pass = effect.PassDefault;
					}
				}

				context.DrawFullScreenQuadFrustumRay(pass, _cameraFrustumFarCorners);
			}
		}
		#endregion
	}
}
