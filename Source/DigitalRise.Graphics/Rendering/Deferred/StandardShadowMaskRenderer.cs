// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Shadows;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	internal class StandardShadowMaskEffectBinding : EffectWrapper
	{
		public EffectParameter ViewInverse { get; private set; }
		public EffectParameter GBuffer0 { get; private set; }
		public EffectParameter Parameters0 { get; private set; }
		public EffectParameter Parameters1 { get; private set; }
		public EffectParameter Parameters2 { get; private set; }
		public EffectParameter LightPosition { get; private set; }
		public EffectParameter ShadowView { get; private set; }
		public EffectParameter ShadowMatrix { get; private set; }
		public EffectParameter JitterMap { get; private set; }
		public EffectParameter ShadowMap { get; private set; }
		public EffectParameter Samples { get; private set; }

		public static StandardShadowMaskEffectBinding Instance { get; } = new StandardShadowMaskEffectBinding();

		private StandardShadowMaskEffectBinding() : base("Deferred/StandardShadowMask")
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
			ShadowMatrix = effect.Parameters["ShadowMatrix"];
			JitterMap = effect.Parameters["JitterMap"];
			ShadowMap = effect.Parameters["ShadowMap"];
			Samples = effect.Parameters["Samples"];
		}
	}


	/// <summary>
	/// Creates the shadow mask from the shadow map of a light node with a 
	/// <see cref="StandardShadow"/>.
	/// </summary>
	public static class StandardShadowMaskRenderer
	{
		//--------------------------------------------------------------
		#region Constants
		//--------------------------------------------------------------

		// Poisson disk kernel (n = 32, min distance = 0.28, sorted by radius):
		internal static readonly Vector2[] PoissonKernel =
		{
			new Vector2(-0.1799547f, -0.1070055f),
			new Vector2(0.09824847f, 0.211835f),
			new Vector2(0.2322298f, -0.07181169f),
			new Vector2(-0.2205104f, 0.1904023f),
			new Vector2(0.2997971f, -0.3547534f),
			new Vector2(-0.1691635f, -0.4555008f),
			new Vector2(0.4309701f, 0.2550637f),
			new Vector2(0.210612f, 0.4862425f),
			new Vector2(0.09707758f, -0.5718418f),
			new Vector2(-0.4771754f, -0.3439123f),
			new Vector2(-0.5416231f, 0.2631182f),
			new Vector2(-0.1619952f, 0.6209877f),
			new Vector2(-0.6892721f, -0.02496444f),
			new Vector2(0.6895862f, -0.1383066f),
			new Vector2(0.7064654f, 0.1432759f),
			new Vector2(0.48727f, 0.5617883f),
			new Vector2(0.5379964f, -0.5262181f),
			new Vector2(0.1531894f, 0.7650354f),
			new Vector2(-0.2465617f, -0.7410074f),
			new Vector2(-0.521504f, -0.6304727f),
			new Vector2(-0.4443332f, 0.7210394f),
			new Vector2(0.2232238f, -0.8430021f),
			new Vector2(0.8676031f, 0.3737303f),
			new Vector2(-0.8400609f, -0.4440856f),
			new Vector2(-0.8283941f, 0.4660586f),
			new Vector2(0.8210237f, -0.479335f),
			new Vector2(-0.9654546f, 0.0565909f),
			new Vector2(0.4637565f, 0.8531818f),
			new Vector2(0.9780589f, 0.04445554f),
			new Vector2(0.5709926f, -0.8048507f),
			new Vector2(-0.07513653f, -0.9907708f),
			new Vector2(-0.1376491f, 0.9876857f),
		};
		#endregion


		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly Vector3[] _frustumFarCorners = new Vector3[4];

		private static Texture2D _jitterMap;

		private static readonly Vector3[] _samples = new Vector3[PoissonKernel.Length];
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

			var effect = StandardShadowMaskEffectBinding.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;

			var cameraNode = context.CameraNode;
			effect.ViewInverse.SetValue(cameraNode.PoseWorld);
			effect.GBuffer0.SetValue(context.GBuffer0);

			var viewport = graphicsDevice.Viewport;
			effect.Parameters0.SetValue(new Vector2(viewport.Width, viewport.Height));

			if (_jitterMap == null)
				_jitterMap = NoiseHelper.GetGrainTexture(NoiseHelper.DefaultJitterMapWidth);

			effect.JitterMap.SetValue(_jitterMap);

			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var shadow = lightNode.Shadow as StandardShadow;
				if (shadow == null)
					continue;

				if (shadow.ShadowMap == null || shadow.ShadowMask == null)
					continue;

				// The effect must only render in a specific channel.
				// Do not change blend state if the correct write channels is already set, e.g. if this
				// shadow is part of a CompositeShadow, the correct blend state is already set.
				if ((int)graphicsDevice.BlendState.ColorWriteChannels != (1 << shadow.ShadowMaskChannel))
					graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteSingleChannel[shadow.ShadowMaskChannel];

				effect.Parameters1.SetValue(new Vector4(
				  shadow.Near,
				  shadow.Far,
				  shadow.EffectiveDepthBias,
				  shadow.EffectiveNormalOffset));

				// If we use a subset of the Poisson kernel, we have to normalize the scale.
				int numberOfSamples = Math.Min(shadow.NumberOfSamples, PoissonKernel.Length);
				float filterRadius = shadow.FilterRadius;
				if (numberOfSamples > 0)
					filterRadius /= PoissonKernel[numberOfSamples - 1].Length();

				effect.Parameters2.SetValue(new Vector3(
				  shadow.ShadowMap.Width,
				  filterRadius,
				  // The StandardShadow.JitterResolution is the number of texels per world unit.
				  // In the shader the parameter JitterResolution contains the division by the jitter map size.
				  shadow.JitterResolution / _jitterMap.Width));

				effect.LightPosition.SetValue(cameraNode.PoseWorld.ToLocalPosition(lightNode.PoseWorld.Position));

				Matrix cameraViewToShadowView = cameraNode.PoseWorld * shadow.View;
				effect.ShadowView.SetValue(cameraViewToShadowView);
				effect.ShadowMatrix.SetValue(cameraViewToShadowView * shadow.Projection);
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
							_samples[j].X = PoissonKernel[j].X;
							_samples[j].Y = PoissonKernel[j].Y;
							_samples[j].Z = 1.0f / numberOfSamples;

							// Note [HelmutG]: I have tried weights decreasing with distance but that did not
							// look better.
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
			var effect = StandardShadowMaskEffectBinding.Instance.Effect;
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
