// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Data.Lights;
using DigitalRise.Data.Materials;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	internal class AmbientLightEffectBinding : EffectWrapper
	{
		public EffectParameter WorldViewProjection { get; private set; }
		public EffectParameter ViewportSize { get; private set; }
		public EffectParameter LightColor { get; private set; }
		public EffectParameter HemisphericAttenuation { get; private set; }
		public EffectParameter Up { get; private set; }
		public EffectParameter GBuffer0 { get; private set; }
		public EffectParameter GBuffer1 { get; private set; }
		public EffectPass PassClip { get; private set; }
		public EffectPass PassLight { get; private set; }

		public static AmbientLightEffectBinding Instance { get; } = new AmbientLightEffectBinding();

		private AmbientLightEffectBinding() : base("Deferred/AmbientLight")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			WorldViewProjection = effect.Parameters["WorldViewProjection"];
			ViewportSize = effect.Parameters["ViewportSize"];
			LightColor = effect.Parameters["AmbientLight"];
			HemisphericAttenuation = effect.Parameters["AmbientLightAttenuation"];
			Up = effect.Parameters["AmbientLightUp"];
			GBuffer0 = effect.Parameters["GBuffer0"];
			GBuffer1 = effect.Parameters["GBuffer1"];
			PassClip = effect.CurrentTechnique.Passes["Clip"];
			PassLight = effect.CurrentTechnique.Passes["Light"];
		}
	}

	/// <summary>
	/// Renders <see cref="AmbientLight"/>s into the light buffer.
	/// </summary>
	public static class AmbientLightRenderer
	{
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

			var effect = AmbientLightEffectBinding.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

			var viewport = graphicsDevice.Viewport;
			effect.ViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
			effect.GBuffer0.SetValue(context.GBuffer0);
			effect.GBuffer1.SetValue(context.GBuffer1);

			var cameraNode = context.CameraNode;
			Matrix viewProjection = (Matrix)cameraNode.View * cameraNode.Camera.Projection;

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			cameraNode.LastFrame = frame;

			var isHdrEnabled = graphicsDevice.IsCurrentRenderTargetHdr();
			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var light = lightNode.Light as AmbientLight;
				if (light == null)
					continue;

				// LightNode is visible in current frame.
				lightNode.LastFrame = frame;

				float hdrScale = isHdrEnabled ? light.HdrScale : 1;
				effect.LightColor.SetValue(light.Color.ToVector3() * light.Intensity * hdrScale);
				effect.HemisphericAttenuation.SetValue(light.HemisphericAttenuation);

				Vector3 upWorld = lightNode.PoseWorld.ToWorldDirection(Vector3.Up);
				effect.Up.SetValue(upWorld);

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

				context.DrawFullScreenQuad(effect.PassLight);
			}
		}
		#endregion
	}
}
