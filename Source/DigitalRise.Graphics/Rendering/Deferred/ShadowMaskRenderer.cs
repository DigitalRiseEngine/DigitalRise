// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRise.Mathematics;
using DigitalRise.Misc;
using DigitalRise.PostProcessing;
using DigitalRise.PostProcessing.Processing;
using DigitalRise.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Rendering.Deferred
{
	/// <summary>
	/// Renders the shadow mask from the shadow map of a <see cref="LightNode"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The shadow mask is an image as seen from the camera where for each pixel the shadow info is
	/// stored. A value of 0 means the pixel is in the shadow. A value of 1 means the pixel is fully
	/// lit. (The shadow mask is rendered into the current render target.)
	/// </para>
	/// <para>
	/// This renderer renders the shadow masks and sets the properties <see cref="Shadow.ShadowMask"/>
	/// and <see cref="Shadow.ShadowMaskChannel"/> of the handled <see cref="Shadow"/> instances. The 
	/// <see cref="ShadowMaskRenderer"/> handles <see cref="StandardShadow"/>s,
	/// <see cref="CubeMapShadow"/>, <see cref="CascadedShadow"/>s, and
	/// <see cref="CompositeShadow"/>s. To handle new shadow types, you need to add a custom
	/// <see cref="SceneNodeRenderer"/> to the <see cref="SceneRenderer.Renderers"/> collection.
	/// </para>
	/// <para>
	/// <see cref="RenderContext.GBuffer0"/> needs to be set in the render context.
	/// </para>
	/// <para>
	/// <see cref="RecycleShadowMasks"/> should be called every frame when shadow masks are not needed
	/// anymore. This method returns all shadow mask render targets to the render target pool and
	/// allows other render operations to reuse the render targets.
	/// </para>
	/// <para>
	/// <strong>Render Target and Viewport:</strong><br/>
	/// This renderer changes the current render target of the graphics device because it uses the
	/// graphics device to render the shadow masks into internal render targets. The render target
	/// and the viewport of the graphics device are undefined after rendering.
	/// </para>
	/// </remarks>
	public static class ShadowMaskRenderer
	{
		private const int MaxNumberOfShadowMasks = 1;

		// Notes:
		// Possible Optimization: Compute GetLightContribution() lazily. If a node has a higher
		// priority than all the rest, then we never need to compute GetLightContribution() for this
		// node. --> Set all SortTags to -1. And initialize it in a custom IComparer when needed. 


		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------
		private static RenderTarget2D[] _shadowMasks;
		private static readonly List<LightNode>[] _shadowMaskBins;      // A list of scene nodes for each channel of the shadow masks.
		private static readonly UpsampleFilter _upsampleFilter = new UpsampleFilter();
		private static readonly List<LightNode> _lightNodes = new List<LightNode>();   // Temporary list for sorting light nodes.

		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets a filter that is applied to the shadow masks as a post-process.
		/// </summary>
		/// <value>
		/// The filter that is applied to the shadow masks as a post-process. The default value is
		/// <see langword="null"/>.
		/// </value>
		/// <remarks>
		/// <para>
		/// The shadow quality can be improved by filtering the resulting shadow mask. For example, an
		/// anisotropic, cross-bilateral Gaussian filter can be applied to create soft shadows.
		/// </para>
		/// <para>
		/// The configured post-process filter needs to support reading from and writing into the same
		/// render target. This is supported by any separable box or Gaussian blur because they filter
		/// the image in two passes. Single pass blurs, e.g. a Poisson blur, cannot be used.
		/// </para>
		/// </remarks>
		public static PostProcessor Filter { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether the shadow mask is created using only the half scene
		/// resolution to improve performance.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the shadow mask is created using only the half scene resolution to
		/// improve performance; otherwise, <see langword="false"/> to use the full resolution for best
		/// quality. The default is <see langword="false" />.
		/// </value>
		public static bool UseHalfResolution { get; set; }


		/// <summary>
		/// Gets or sets a value controlling the bilateral upsampling. (Only used when
		/// <see cref="UseHalfResolution"/> is <see langword="true" />.)
		/// </summary>
		/// <value>
		/// The depth sensitivity for bilateral upsampling. Use 0 to use bilinear upsampling and disable
		/// bilateral upsampling. Use values greater than 0, to enable bilateral upsampling. The default
		/// value is 1000.
		/// </value>
		/// <remarks>
		/// <para>
		/// If <see cref="UseHalfResolution"/> is <see langword="true" />, the shadow mask is created
		/// using the half scene resolution. Creating shadows using the low resolution shadow mask can
		/// create artifacts, e.g. a non-shadowed halo around objects. To avoid these artifacts,
		/// bilateral upsampling can be enabled, by setting <see cref="UpsampleDepthSensitivity"/> to a
		/// value greater than 0.
		/// </para>
		/// <para>
		/// For more information about bilateral upsampling, see <see cref="UpsampleFilter"/> and
		/// <see cref="UpsampleFilter.DepthSensitivity"/>.
		/// </para>
		/// </remarks>
		public static float UpsampleDepthSensitivity { get; set; } = 1000.0f;

		#endregion

		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		static ShadowMaskRenderer()
		{
			_shadowMasks = new RenderTarget2D[MaxNumberOfShadowMasks];
			_shadowMaskBins = new List<LightNode>[MaxNumberOfShadowMasks * 4];

			for (var i = 0; i < 4; ++i)
			{
				_shadowMaskBins[i] = new List<LightNode>();
			}
		}

		#endregion

		#region Methods

		private static int AssignShadowMask(RenderContext context, LightNode lightNode)
		{
			// Each shadow mask has 4 8-bit channels. We must assign a shadow mask channel to 
			// each shadow-casting light. Non-overlapping lights can use the same channel.
			// Overlapping lights must use different channels. If we run out of channels,
			// we remove some lights from the list.

			var scene = context.Scene;

			var graphicsDevice = DR.GraphicsDevice;
			var viewport = graphicsDevice.Viewport;
			int maskWidth = viewport.Width;
			int maskHeight = viewport.Height;
			if (UseHalfResolution && Numeric.IsLessOrEqual(UpsampleDepthSensitivity, 0))
			{
				// Half-res rendering with no upsampling.
				maskWidth /= 2;
				maskHeight /= 2;
			}

			// Loop over all bins until we find one which can be used for this light node.
			int binIndex;
			for (binIndex = 0; binIndex < _shadowMaskBins.Length; binIndex++)
			{
				var bin = _shadowMaskBins[binIndex];

				// Check if the light node touches any other light nodes in this bin.
				bool hasContact = false;
				foreach (var otherLightNode in bin)
				{
					if (scene.HaveContact(lightNode, otherLightNode))
					{
						hasContact = true;
						break;
					}
				}

				// No overlap. Use this bin.
				if (!hasContact)
				{
					bin.Add(lightNode);
					break;
				}
			}

			if (binIndex >= _shadowMaskBins.Length)
				return -1;  // Light node does not fit into any bin.

			int shadowMaskIndex = binIndex / 4;

			if (_shadowMasks[shadowMaskIndex] == null)
			{
				// Create shadow mask.
				var shadowMaskFormat = new RenderTargetFormat(maskWidth, maskHeight, false, SurfaceFormat.Color, DepthFormat.None);
				_shadowMasks[shadowMaskIndex] = context.RenderTargetPool.Obtain2D(shadowMaskFormat);
			}

			// Assign shadow mask to light node.
			lightNode.Shadow.ShadowMask = _shadowMasks[shadowMaskIndex];
			lightNode.Shadow.ShadowMaskChannel = binIndex % 4;

			return shadowMaskIndex;
		}


		public static void Render(RenderContext context, IList<SceneNode> nodes)
		{
			var graphicsDevice = DR.GraphicsDevice;
			var savedRenderState = new RenderStateSnapshot();
			var viewport = graphicsDevice.Viewport;

			RenderTarget2D lowResTarget = null;
			if (UseHalfResolution && Numeric.IsGreater(UpsampleDepthSensitivity, 0))
			{
				// Half-res rendering with upsampling.
				var format = new RenderTargetFormat(_shadowMasks[0]);
				format.Width /= 2;
				format.Height /= 2;
				lowResTarget = context.RenderTargetPool.Obtain2D(format);
			}

			// Assign shadow masks
			_lightNodes.Clear();
			for (int i = 0; i < nodes.Count; i++)
			{
				var lightNode = nodes[i] as LightNode;
				if (lightNode == null || lightNode.Shadow == null || lightNode.Shadow.ShadowMap == null)
					continue;

				if (AssignShadowMask(context, lightNode) != -1)
				{
					_lightNodes.Add(lightNode);
				}
			}

			// Set device render target and clear it to white (= no shadow).
			var shadowMask = _shadowMasks[0];
			graphicsDevice.SetRenderTarget(shadowMask);
			graphicsDevice.Clear(Color.White);

			// Render shadow masks
			CascadedShadowMaskRenderer.Render(context, nodes);
			CubeMapShadowMaskRenderer.Render(context, nodes);
			StandardShadowMaskRenderer.Render(context, nodes);

			// Post process
			for (var i = 0; i < _shadowMasks.Length; i++)
			{
				PostProcess(context, _shadowMasks[i]);
			}

			foreach (var bin in _shadowMaskBins)
			{
				bin.Clear();
			}

			savedRenderState.Restore();
			graphicsDevice.ResetTextures();
			graphicsDevice.SetRenderTarget(null);
			graphicsDevice.Viewport = viewport;

			context.RenderTargetPool.Recycle(lowResTarget);
		}


		private static void PostProcess(RenderContext context, RenderTarget2D target)
		{
			Debug.Assert(target != null);

			var graphicsDevice = DR.GraphicsDevice;
			var originalSourceTexture = context.SourceTexture;
			var source = (RenderTarget2D)graphicsDevice.GetCurrentRenderTarget();
			var originalViewport = graphicsDevice.Viewport;

			if (Filter != null && Filter.Enabled)
			{
				context.SourceTexture = source;
				Filter.Process(context);
			}

			bool doUpsampling = UseHalfResolution && Numeric.IsGreater(UpsampleDepthSensitivity, 0);

			Debug.Assert(doUpsampling && source != target || !doUpsampling && source == target);

			if (doUpsampling)
			{
				var originalBlendState = graphicsDevice.BlendState;
				graphicsDevice.BlendState = BlendState.Opaque;

				// The previous scene render target is bound as texture.
				// --> Switch scene render targets!
				context.SourceTexture = source;
				graphicsDevice.SetRenderTarget(target);
				_upsampleFilter.DepthSensitivity = UpsampleDepthSensitivity;
				_upsampleFilter.Mode = UpsamplingMode.Bilateral;
				_upsampleFilter.RebuildZBuffer = false;
				_upsampleFilter.Process(context);

				if (originalBlendState != null)
					graphicsDevice.BlendState = originalBlendState;
			}

			context.SourceTexture = originalSourceTexture;

			graphicsDevice.SetRenderTarget(source);
			graphicsDevice.Viewport = originalViewport;
		}


		/// <summary>
		/// Recycles the shadow masks.
		/// </summary>
		/// <remarks>
		/// This method also resets the shadow properties <see cref="Shadow.ShadowMask"/> and
		/// <see cref="Shadow.ShadowMaskChannel"/>.
		/// </remarks>
		public static void RecycleShadowMasks(RenderContext context)
		{
			foreach (var lightNode in _lightNodes)
			{
				var shadow = lightNode.Shadow;
				if (shadow != null)
				{
					shadow.ShadowMask = null;
					shadow.ShadowMaskChannel = 0;
				}
			}
			_lightNodes.Clear();

			var renderTargetPool = context.RenderTargetPool;
			for (int i = 0; i < _shadowMasks.Length; i++)
			{
				renderTargetPool.Recycle(_shadowMasks[i]);
				_shadowMasks[i] = null;
			}
		}

		#endregion
	}
}
