// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Shadows;
using DigitalRise.Mathematics;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	internal class CascadedShadowMaskEffectBinding : EffectWrapper
	{
		public EffectParameter ViewInverse { get; private set; }
		public EffectParameter GBuffer0 { get; private set; }
		public EffectParameter Parameters0 { get; private set; }
		public EffectParameter Parameters1 { get; private set; }
		public EffectParameter Parameters2 { get; private set; }
		public EffectParameter Distances { get; private set; }
		public EffectParameter ShadowMatrices { get; private set; }
		public EffectParameter DepthBias { get; private set; }
		public EffectParameter NormalOffset { get; private set; }
		public EffectParameter LightDirection { get; private set; }
		public EffectParameter NumberOfCascades { get; private set; }
		public EffectParameter JitterMap { get; private set; }
		public EffectParameter ShadowMap { get; private set; }
		public EffectParameter Samples { get; private set; }

		public static CascadedShadowMaskEffectBinding Instance { get; } = new CascadedShadowMaskEffectBinding();

		private CascadedShadowMaskEffectBinding() : base("Deferred/CascadedShadowMask")
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
			Distances = effect.Parameters["Distances"];
			ShadowMatrices = effect.Parameters["ShadowMatrices"];
			DepthBias = effect.Parameters["DepthBias"];
			NormalOffset = effect.Parameters["NormalOffset"];
			LightDirection = effect.Parameters["LightDirection"];
			NumberOfCascades = effect.Parameters["NumberOfCascades"];
			JitterMap = effect.Parameters["JitterMap"];
			ShadowMap = effect.Parameters["ShadowMap"];
			Samples = effect.Parameters["Samples"];
		}
	}

	/// <summary>
	/// Creates the shadow mask from the shadow map of a light node with a 
	/// <see cref="CascadedShadow"/>.
	/// </summary>
	public static class CascadedShadowMaskRenderer
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private static readonly Vector3[] _frustumFarCorners = new Vector3[4];

		private static Texture2D _jitterMap;

		// Temp. array for 4 matrices.
		private static readonly Matrix[] _matrices = new Matrix[4];

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

			var effect = CascadedShadowMaskEffectBinding.Instance;
			effect.Validate();
			context.ThrowIfCameraMissing();

			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();
			graphicsDevice.DepthStencilState = DepthStencilState.None;
			graphicsDevice.RasterizerState = RasterizerState.CullNone;

			// Set camera properties.
			var cameraNode = context.CameraNode;
			var cameraPose = cameraNode.PoseWorld;
			Matrix viewInverse = cameraPose;
			effect.ViewInverse.SetValue(viewInverse);
			effect.GBuffer0.SetValue(context.GBuffer0);

			var viewport = graphicsDevice.Viewport;
			effect.Parameters0.SetValue(new Vector2(viewport.Width, viewport.Height));

			// Set jitter map.
			if (_jitterMap == null)
				_jitterMap = NoiseHelper.GetGrainTexture(NoiseHelper.DefaultJitterMapWidth);

			effect.JitterMap.SetValue(_jitterMap);

			float cameraFar = context.CameraNode.Camera.Projection.Far;

			for (int i = 0; i < numberOfNodes; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null)
					continue;

				var shadow = lightNode.Shadow as CascadedShadow;
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
					shadow.FadeOutRange,
					shadow.Distances.GetComponentByIndex(shadow.NumberOfCascades - 1),
					shadow.VisualizeCascades ? 1 : 0,
					shadow.ShadowFog));

				float filterRadius = shadow.FilterRadius;

				// If we use a subset of the Poisson kernel, we have to normalize the scale.
				int numberOfSamples = Math.Min(shadow.NumberOfSamples, StandardShadowMaskRenderer.PoissonKernel.Length);

				// Not all shader passes support cascade visualization. Use a similar pass instead.
				if (shadow.VisualizeCascades)
				{
					if (numberOfSamples < 0)
					{
						numberOfSamples = 4;
					}
					else if (numberOfSamples == 0)
					{
						numberOfSamples = 1;
						filterRadius = 0;
					}
				}

				// The best dithered CSM supports max 22 samples.
				if (shadow.CascadeSelection == ShadowCascadeSelection.BestDithered && numberOfSamples > 22)
					numberOfSamples = 22;

				if (numberOfSamples > 0)
					filterRadius /= StandardShadowMaskRenderer.PoissonKernel[numberOfSamples - 1].Length();

				effect.Parameters2.SetValue(new Vector4(
					shadow.ShadowMap.Width,
					shadow.ShadowMap.Height,
					filterRadius,
					// The StandardShadow.JitterResolution is the number of texels per world unit.
					// In the shader the parameter JitterResolution contains the division by the jitter map size.
					shadow.JitterResolution / _jitterMap.Width));

				// Split distances.
				if (effect.Distances != null)
				{
					// Set not used entries to large values.
					Vector4 distances = shadow.Distances;
					for (int j = shadow.NumberOfCascades; j < 4; j++)
						distances.SetComponentByIndex(j, 10 * cameraFar);

					effect.Distances.SetValue(distances);
				}

				Debug.Assert(shadow.ViewProjections.Length == 4);
				for (int j = 0; j < _matrices.Length; j++)
					_matrices[j] = viewInverse * shadow.ViewProjections[j];

				effect.ShadowMatrices.SetValue(_matrices);

				effect.DepthBias.SetValue(shadow.EffectiveDepthBias);
				effect.NormalOffset.SetValue(shadow.EffectiveNormalOffset);

				Vector3 lightBackwardWorld = lightNode.PoseWorld.Orientation.GetColumn(2);
				effect.LightDirection.SetValue(cameraPose.ToLocalDirection(lightBackwardWorld));
				effect.NumberOfCascades.SetValue(shadow.NumberOfCascades);
				effect.ShadowMap.SetValue(shadow.ShadowMap);

				var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
				Vector2 texCoordTopLeft = new Vector2(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
				Vector2 texCoordBottomRight = new Vector2(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
				GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);

				var pass = GetPass(numberOfSamples, shadow.CascadeSelection, shadow.VisualizeCascades);

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
		private static EffectPass GetPass(int numberOfSamples, ShadowCascadeSelection cascadeSelection, bool visualizeCascades)
		{
			if (visualizeCascades)
				numberOfSamples = 32;    // Only these passes support cascade visualization.

			var effect = CascadedShadowMaskEffectBinding.Instance.Effect;
			if (numberOfSamples < 0)
			{
				return effect.Techniques[0].Passes[(int)cascadeSelection];
			}
			if (numberOfSamples == 0)
			{
				return effect.Techniques[0].Passes[3 + (int)cascadeSelection];
			}
			if (numberOfSamples == 1)
			{
				return effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5];
			}
			if (numberOfSamples <= 4)
			{
				return effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 1];
			}
			if (numberOfSamples <= 8)
			{
				return effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 2];
			}
			if (numberOfSamples <= 16)
			{
				return effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 3];
			}

			return effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 4];
		}
		#endregion
	}
}
