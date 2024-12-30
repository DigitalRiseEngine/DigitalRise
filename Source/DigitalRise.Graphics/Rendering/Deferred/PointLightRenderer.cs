// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Data.Lights;
using DigitalRise.Data.Materials;
using DigitalRise.Geometry;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	internal class PointLightEffectWrapper : EffectWrapper
	{
		public EffectParameter WorldViewProjection { get; private set; }
		public EffectParameter ViewportSize { get; private set; }
		public EffectParameter DiffuseColor { get; private set; }
		public EffectParameter SpecularColor { get; private set; }
		public EffectParameter Position { get; private set; }
		public EffectParameter Range { get; private set; }
		public EffectParameter Attenuation { get; private set; }
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

		public static PointLightEffectWrapper Instance { get; } = new PointLightEffectWrapper();

		private PointLightEffectWrapper() : base("Deferred/PointLight")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			WorldViewProjection = effect.Parameters["WorldViewProjection"];
			ViewportSize = effect.Parameters["ViewportSize"];
			DiffuseColor = effect.Parameters["PointLightDiffuse"];
			SpecularColor = effect.Parameters["PointLightSpecular"];
			Position = effect.Parameters["PointLightPosition"];
			Range = effect.Parameters["PointLightRange"];
			Attenuation = effect.Parameters["PointLightAttenuation"];
			TextureMatrix = effect.Parameters["PointLightTextureMatrix"];
			Texture = effect.Parameters["PointLightTexture"];
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
	/// Renders <see cref="PointLight"/>s into the light buffer.
	/// </summary>
	/// <inheritdoc cref="LightRenderer"/>
	public static class PointLightRenderer
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly Vector3[] _frustumFarCorners = new Vector3[4];

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

			var effect = PointLightEffectWrapper.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();

			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

			var viewport = graphicsDevice.Viewport;
			effect.ViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
			effect.GBuffer0.SetValue(context.GBuffer0);
			effect.GBuffer1.SetValue(context.GBuffer1);

			var cameraNode = context.CameraNode;
			Pose cameraPose = cameraNode.PoseWorld;
			Matrix viewProjection = (Matrix)(cameraNode.View * cameraNode.ViewVolume.Projection);

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			context.CameraNode.LastFrame = frame;

			var isHdrEnabled = context.IsHdr;
			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var light = lightNode.Light as PointLight;
				if (light == null)
					continue;

				// LightNode is visible in current frame.
				lightNode.LastFrame = frame;

				float hdrScale = isHdrEnabled ? light.HdrScale : 1;
				effect.DiffuseColor.SetValue(light.Color.ToVector3() * light.DiffuseIntensity * hdrScale);
				effect.SpecularColor.SetValue(light.Color.ToVector3() * light.SpecularIntensity * hdrScale);

				Pose lightPose = lightNode.PoseWorld;

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

				effect.Position.SetValue((lightPose.Position - cameraPose.Position));
				effect.Range.SetValue(light.Range);
				effect.Attenuation.SetValue(light.Attenuation);

				bool hasTexture = (light.Texture != null);
				if (hasTexture)
				{
					effect.Texture.SetValue(light.Texture);

					// Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
					// cube map and objects or texts in it are mirrored.)
					var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
					effect.TextureMatrix.SetValue((Matrix)(mirrorZ * lightPose.Inverse));
				}

				var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightPose.Position, light.Range);
				var texCoordTopLeft = new Vector2(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
				var texCoordBottomRight = new Vector2(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
				GraphicsHelper.GetFrustumFarCorners(cameraNode.ViewVolume, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);

				// Convert frustum far corners from view space to world space.
				for (int j = 0; j < _frustumFarCorners.Length; j++)
					_frustumFarCorners[j] = cameraPose.ToWorldDirection(_frustumFarCorners[j]);

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
						{
							pass = effect.PassShadowedTexturedAlpha;
						}
						else
						{
							pass = effect.PassShadowedTexturedRgb;
						}
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
						{
							pass = effect.PassTexturedAlpha;
						}
						else
						{
							pass = effect.PassTexturedRgb;
						}
					}
					else
					{
						pass = effect.PassDefault;
					}
				}

				context.DrawQuadFrustumRay(pass, rectangle, _frustumFarCorners);
			}

			savedRenderState.Restore();
		}
		#endregion
	}
}
