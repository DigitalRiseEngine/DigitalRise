using DigitalRise.Data.Lights;
using DigitalRise.Data.Materials;
using DigitalRise.Geometry;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DigitalRise.Rendering.Deferred
{
	internal class ProjectorLightEffectWrapper : EffectWrapper
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
		public EffectPass PassDefaultRgb { get; private set; }
		public EffectPass PassDefaultAlpha { get; private set; }
		public EffectPass PassShadowedRgb { get; private set; }
		public EffectPass PassShadowedAlpha { get; private set; }

		public static ProjectorLightEffectWrapper Instance { get; } = new ProjectorLightEffectWrapper();

		private ProjectorLightEffectWrapper() : base("Deferred/ProjectorLight")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			WorldViewProjection = effect.Parameters["WorldViewProjection"];
			ViewportSize = effect.Parameters["ViewportSize"];
			DiffuseColor = effect.Parameters["ProjectorLightDiffuse"];
			SpecularColor = effect.Parameters["ProjectorLightSpecular"];
			Position = effect.Parameters["ProjectorLightPosition"];
			Range = effect.Parameters["ProjectorLightRange"];
			Attenuation = effect.Parameters["ProjectorLightAttenuation"];
			Texture = effect.Parameters["ProjectorLightTexture"];
			TextureMatrix = effect.Parameters["ProjectorLightTextureMatrix"];
			GBuffer0 = effect.Parameters["GBuffer0"];
			GBuffer1 = effect.Parameters["GBuffer1"];
			ShadowMaskChannel = effect.Parameters["ShadowMaskChannel"];
			ShadowMask = effect.Parameters["ShadowMask"];
			PassClip = effect.CurrentTechnique.Passes["Clip"];
			PassDefaultRgb = effect.CurrentTechnique.Passes["DefaultRgb"];
			PassDefaultAlpha = effect.CurrentTechnique.Passes["DefaultAlpha"];
			PassShadowedRgb = effect.CurrentTechnique.Passes["ShadowedRgb"];
			PassShadowedAlpha = effect.CurrentTechnique.Passes["ShadowedAlpha"];
		}
	}

	/// <summary>
	/// Renders <see cref="ProjectorLight"/>s into the light buffer.
	/// </summary>
	/// <inheritdoc cref="LightRenderer"/>
	public static class ProjectorLightRenderer
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

			var effect = ProjectorLightEffectWrapper.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();

			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

			var viewport = context.Viewport;
			effect.ViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
			effect.GBuffer0.SetValue(context.GBuffer0);
			effect.GBuffer1.SetValue(context.GBuffer1);

			var cameraNode = context.CameraNode;
			var cameraPose = cameraNode.PoseWorld;
			Matrix viewProjection = (Matrix)(cameraNode.View * cameraNode.ViewVolume.Projection);

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			cameraNode.LastFrame = frame;

			var isHdrEnabled = context.IsHdr;
			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var light = lightNode.Light as ProjectorLight;
				if (light == null)
					continue;

				// LightNode is visible in current frame.
				lightNode.LastFrame = frame;

				float hdrScale = isHdrEnabled ? light.HdrScale : 1;
				effect.DiffuseColor.SetValue(light.Color.ToVector3() * light.DiffuseIntensity * hdrScale);
				effect.SpecularColor.SetValue(light.Color.ToVector3() * light.SpecularIntensity * hdrScale);
				effect.Texture.SetValue(light.Texture);

				var lightPose = lightNode.PoseWorld;
				effect.Position.SetValue((lightPose.Position - cameraPose.Position));

				effect.Range.SetValue(light.Projection.Far);
				effect.Attenuation.SetValue(light.Attenuation);
				effect.TextureMatrix.SetValue((Matrix)(GraphicsHelper.ProjectorBiasMatrix * light.Projection.Projection * (lightPose.Inverse * new Pose(cameraPose.Position))));

				var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
				var texCoordTopLeft = new Vector2(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
				var texCoordBottomRight = new Vector2(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
				GraphicsHelper.GetFrustumFarCorners(cameraNode.ViewVolume, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);

				// Convert frustum far corners from view space to world space.
				for (int j = 0; j < _frustumFarCorners.Length; j++)
					_frustumFarCorners[j] = cameraPose.ToWorldDirection(_frustumFarCorners[j]);

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
					if (light.Texture.Format == SurfaceFormat.Alpha8)
					{
						pass = effect.PassShadowedAlpha;
					}
					else
					{
						pass = effect.PassShadowedRgb;
					}
				}
				else
				{
					if (light.Texture.Format == SurfaceFormat.Alpha8)
					{
						pass = effect.PassDefaultAlpha;
					}
					else
					{
						pass = effect.PassDefaultRgb;
					}
				}

				context.DrawQuadFrustumRay(pass, rectangle, _frustumFarCorners);
			}

			savedRenderState.Restore();
		}
		#endregion
	}
}
