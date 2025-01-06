// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Data.Lights;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Shadows;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	internal class CubeMapShadowMaskEffectBinding : EffectWrapper
	{
		public EffectParameter ViewInverse { get; private set; }
		public EffectParameter GBuffer0 { get; private set; }
		public EffectParameter Parameters0 { get; private set; }
		public EffectParameter Parameters1 { get; private set; }
		public EffectParameter Parameters2 { get; private set; }
		public EffectParameter LightPosition { get; private set; }
		public EffectParameter ShadowView { get; private set; }
		public EffectParameter JitterMap { get; private set; }
		public EffectParameter ShadowMap { get; private set; }
		public EffectParameter Samples { get; private set; }

		public static CubeMapShadowMaskEffectBinding Instance { get; } = new CubeMapShadowMaskEffectBinding();

		private CubeMapShadowMaskEffectBinding() : base("Deferred/CubeMapShadowMask")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			ViewInverse = effect.Parameters["ViewInverse"];
			GBuffer0 = effect.Parameters["GBuffer0"];
			Parameters0 = effect.Parameters["Parameters0"];
			Parameters1 = effect.Parameters["Parameters1"];
			Parameters2 = effect.Parameters["Parameters2"];
			LightPosition = effect.Parameters["LightPosition"];
			ShadowView = effect.Parameters["ShadowView"];
			JitterMap = effect.Parameters["JitterMap"];
			ShadowMap = effect.Parameters["ShadowMap"];
			Samples = effect.Parameters["Samples"];
		}
	}

	/// <summary>
	/// Creates the shadow mask from the shadow map of a light node with a
	/// <see cref="CubeMapShadow"/>.
	/// </summary>
	/// <inheritdoc cref="ShadowMaskRenderer"/>
	public static class CubeMapShadowMaskRenderer
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly Vector3[] _frustumFarCorners = new Vector3[4];

		private static Texture2D _jitterMap;

		private static readonly Vector3[] _samples = new Vector3[StandardShadowMaskRenderer.PoissonKernel.Length];
		private static int _lastNumberOfSamples;
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

			var effect = CubeMapShadowMaskEffectBinding.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;

			var cameraNode = context.CameraNode;
			effect.ViewInverse.SetValue(cameraNode.PoseWorld);
			effect.GBuffer0.SetValue(context.GBuffer0);

			var viewport = context.Viewport;
			effect.Parameters0.SetValue(new Vector2(viewport.Width, viewport.Height));

			if (_jitterMap == null)
				_jitterMap = NoiseHelper.GetGrainTexture(NoiseHelper.DefaultJitterMapWidth);

			effect.JitterMap.SetValue(_jitterMap);

			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var light = lightNode.Light as PointLight;
				if (light == null)
					continue;

				var shadow = light.Shadow;
				if (shadow.ShadowMap == null || shadow.ShadowMask == null)
					continue;

				// The effect must only render in a specific channel.
				// Do not change blend state if the correct write channels is already set, e.g. if this
				// shadow is part of a CompositeShadow, the correct blend state is already set.
				if ((int)graphicsDevice.BlendState.ColorWriteChannels != (1 << shadow.ShadowMaskChannel))
					graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteSingleChannel[shadow.ShadowMaskChannel];

				effect.Parameters1.SetValue(new Vector4(
					shadow.Near,
					light.Range,
					shadow.EffectiveDepthBias,
					shadow.EffectiveNormalOffset));

				// If we use a subset of the Poisson kernel, we have to normalize the scale.
				int numberOfSamples = Math.Min(shadow.NumberOfSamples, StandardShadowMaskRenderer.PoissonKernel.Length);
				float filterRadius = shadow.FilterRadius;
				if (numberOfSamples > 0)
					filterRadius /= StandardShadowMaskRenderer.PoissonKernel[numberOfSamples - 1].Length();

				effect.Parameters2.SetValue(new Vector3(
					shadow.ShadowMap.Size,
					filterRadius,
					// The StandardShadow.JitterResolution is the number of texels per world unit.
					// In the shader the parameter JitterResolution contains the division by the jitter map size.
					shadow.JitterResolution / _jitterMap.Width));

				effect.LightPosition.SetValue(cameraNode.PoseWorld.ToLocalPosition(lightNode.PoseWorld.Position));

				effect.ShadowView.SetValue(lightNode.PoseWorld.Inverse * cameraNode.PoseWorld);
				effect.ShadowMap.SetValue(shadow.ShadowMap);

				var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
				Vector2 texCoordTopLeft = new Vector2(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
				Vector2 texCoordBottomRight = new Vector2(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
				GraphicsHelper.GetFrustumFarCorners(cameraNode.ViewVolume, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);

				var pass = GetPass(numberOfSamples);

				if (numberOfSamples > 0)
				{
					if (_lastNumberOfSamples != numberOfSamples)
					{
						// Create an array with the first n samples and the rest set to 0.
						_lastNumberOfSamples = numberOfSamples;
						for (int j = 0; j < numberOfSamples; j++)
						{
							_samples[j].Y = StandardShadowMaskRenderer.PoissonKernel[j].Y;
							_samples[j].X = StandardShadowMaskRenderer.PoissonKernel[j].X;
							_samples[j].Z = 1.0f / numberOfSamples;
						}

						// Set the rest to zero.
						for (int j = numberOfSamples; j < _samples.Length; j++)
							_samples[j] = Vector3.Zero;

						effect.Samples.SetValue(_samples);
					}
					else if (i == 0)
					{
						// Apply offsets in the first loop.
						effect.Samples.SetValue(_samples);
					}
				}

				context.DrawQuadFrustumRay(pass, rectangle, _frustumFarCorners);
			}

			effect.GBuffer0.SetValue((Texture2D)null);
			effect.JitterMap.SetValue((Texture2D)null);
			effect.ShadowMap.SetValue((Texture2D)null);
			savedRenderState.Restore();
		}


		// Chooses a suitable effect pass. See the list of available passes in the effect.
		private static EffectPass GetPass(int numberOfSamples)
		{
			var effect = CubeMapShadowMaskEffectBinding.Instance.Effect;
			if (numberOfSamples < 0)
			{
				var pass = effect.Techniques[0].Passes[0];
				Debug.Assert(pass.Name == "Optimized");
				return pass;
			}
			if (numberOfSamples == 0)
			{
				var pass = effect.Techniques[0].Passes[1];
				Debug.Assert(pass.Name == "Unfiltered");
				return pass;
			}
			if (numberOfSamples == 1)
			{
				var pass = effect.Techniques[0].Passes[2];
				Debug.Assert(pass.Name == "Pcf1");
				return pass;
			}
			if (numberOfSamples <= 4)
			{
				var pass = effect.Techniques[0].Passes[3];
				Debug.Assert(pass.Name == "Pcf4");
				return pass;
			}
			if (numberOfSamples <= 8)
			{
				var pass = effect.Techniques[0].Passes[4];
				Debug.Assert(pass.Name == "Pcf8");
				return pass;
			}
			if (numberOfSamples <= 16)
			{
				var pass = effect.Techniques[0].Passes[5];
				Debug.Assert(pass.Name == "Pcf16");
				return pass;
			}
			else
			{
				var pass = effect.Techniques[0].Passes[6];
				Debug.Assert(pass.Name == "Pcf32");
				return pass;
			}
		}
		#endregion
	}
}
