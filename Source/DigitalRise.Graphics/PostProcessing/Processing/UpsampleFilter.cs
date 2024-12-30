// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Data.Materials;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Misc;
using DigitalRise.Rendering;
using DigitalRise.Rendering.Deferred;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.PostProcessing.Processing
{
	/// <summary>
	/// Defines the texture filtering that is used when combining a low-resolution image with the
	/// full-resolution scene.
	/// </summary>
	/// <remarks>
	/// <see cref="Bilateral"/> and <see cref="NearestDepth"/> are "edge-aware" filtering modes that
	/// try to maintain the original geometry and avoid blurring over edges.
	/// </remarks>
	public enum UpsamplingMode
	{
		/// <summary>
		/// Nearest-neighbor interpolation. Fastest, low quality.
		/// </summary>
		Point,

		/// <summary>
		/// Bilinear interpolation. Fast, good quality.
		/// </summary>
		Linear,

		/// <summary>
		/// Joint (cross) bilateral upsampling. Slow, best quality for surfaces.
		/// </summary>
		Bilateral,

		/// <summary>
		/// Nearest-depth upsampling. Slow, best quality for particles and volumetric effects.
		/// </summary>
		NearestDepth
	}

	internal class UpsampleFilterEffectBinding : EffectWrapper
	{
		public EffectParameter SourceSize { get; private set; }
		public EffectParameter TargetSize { get; private set; }
		public EffectParameter Projection { get; private set; }
		public EffectParameter CameraFar { get; private set; }
		public EffectParameter DepthSensitivity { get; private set; }
		public EffectParameter DepthThreshold { get; private set; }

		public EffectParameter SourceTexture { get; private set; }
		public EffectParameter SceneTexture { get; private set; }
		public EffectParameter DepthBuffer { get; private set; }
		public EffectParameter DepthBufferLow { get; private set; }

		public UpsampleFilterEffectBinding() : base("PostProcessing/UpsampleFilter")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			SourceSize = effect.Parameters["SourceSize"];
			TargetSize = effect.Parameters["TargetSize"];
			Projection = effect.Parameters["Projection"];
			CameraFar = effect.Parameters["CameraFar"];
			DepthSensitivity = effect.Parameters["DepthSensitivity"];
			DepthThreshold = effect.Parameters["DepthThreshold"];
			SourceTexture = effect.Parameters["SourceTexture"];
			SceneTexture = effect.Parameters["SceneTexture"];
			DepthBuffer = effect.Parameters["DepthBuffer"];
			DepthBufferLow = effect.Parameters["DepthBufferLow"];
		}
	}


	/// <summary>
	/// Upscales an input texture.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This post-processor reads the low-resolution <see cref="RenderContext.SourceTexture"/> and
	/// increases the resolution to match the <see cref="RenderContext.RenderTarget"/>.
	/// </para>
	/// <para>
	/// The <see cref="UpsampleFilter"/> supports different modes (see property <see cref="Mode"/>).
	/// <see cref="UpsamplingMode.Point"/> and <see cref="UpsamplingMode.Linear"/> are basic
	/// upsampling modes that do not take the original geometry in account.
	/// <see cref="UpsamplingMode.Bilateral"/>and <see cref="UpsamplingMode.NearestDepth"/> are
	/// "edge-aware" modes that try to maintain the original geometry and prevent blurred edges. The
	/// "edge-aware" modes require that the depth buffer is set in the render context (see property 
	/// <see cref="RenderContext.GBuffer0"/>) and the low-resolution copy of the depth buffer needs to
	/// be stored in <c>renderContext.Data[RenderContextKey.DepthBufferHalf]</c>.
	/// </para>
	/// <para>
	/// Optionally, a <see cref="RenderContext.SceneTexture"/> can be set in the 
	/// <see cref="RenderContext"/>. In this case the input texture is combined (alpha-blended) to the 
	/// scene texture and the combined result is output by the pixel shader.
	/// </para>
	/// <para>
	/// Optionally, the property <see cref="RebuildZBuffer"/> can be set. In this case the depth
	/// information of the original scene is output together with the color information in the pixel
	/// shader.
	/// </para>
	/// </remarks>
	public class UpsampleFilter : PostProcessor
	{
		#region Fields

		private readonly UpsampleFilterEffectBinding _effect = new UpsampleFilterEffectBinding();

		#endregion

		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the mode that is used for upsampling the low-resolution image.
		/// </summary>
		/// <value>
		/// The mode for upsampling the low-resolution image. The default value is 
		/// <see cref="UpsamplingMode.Linear"/>.
		/// </value>
		public UpsamplingMode Mode { get; set; }


		/// <summary>
		/// Bilateral Upsampling: Gets or sets the depth sensitivity.
		/// </summary>
		/// <value>The depth sensitivity for bilateral upsampling. The default value is 1000.</value>
		/// <remarks>
		/// <para>
		/// This property is only relevant when <see cref="Mode"/> is set to
		/// <see cref="UpsamplingMode.Bilateral"/>.
		/// </para>
		/// <para>
		/// Joint (cross) bilateral upsampling: The filter uses bilinear interpolation when upsampling
		/// the low-resolution image, except at edges. The depth of the low-resolution pixels is
		/// compared with the depth of the original full-resolution image. Low-resolution pixels that
		/// have a small depth difference have more weight than pixels with are large depth difference.
		/// </para>
		/// <para>
		/// A small depth sensitivity creates smooth images, but edges may be blurred. Use a large depth
		/// sensitivity to maintain hard edges, but the image quality at non-edges may be reduced.
		/// </para>
		/// <para>
		/// Setting <see cref="DepthSensitivity"/> to 0 disables joint bilateral upsampling. (The result
		/// will be the equivalent to bilinear interpolation.)
		/// </para>
		/// </remarks>
		public float DepthSensitivity { get; set; }


		/// <summary>
		/// Nearest-Depth Upsampling: Gets or sets the depth threshold used for edge detection.
		/// </summary>
		/// <value>The depth threshold in world space units. The default value is 1 unit.</value>
		/// <remarks>
		/// <para>
		/// This property is only relevant when <see cref="Mode"/> is set to
		/// <see cref="UpsamplingMode.NearestDepth"/>.
		/// </para>
		/// <para>
		/// Nearest-depth upsampling: The filter uses bilinear interpolation when upsampling the
		/// low-resolution image, except for edges where nearest-depth upsampling is used. The
		/// <see cref="DepthThreshold"/> is the threshold value used for edge detection.
		/// </para>
		/// <para>
		/// A large depth threshold creates smooth images, but edges may be blurred. Use a small depth
		/// threshold to maintain hard edges, but the image quality at non-edges may be reduced.
		/// </para>
		/// </remarks>
		public float DepthThreshold { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether to rebuild the Z-buffer.
		/// </summary>
		/// <value>
		/// <see langword="true"/> to rebuild the Z-buffer; otherwise, <see langword="false"/>.
		/// The default value is <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// When <see cref="RebuildZBuffer"/> is set the depth information of the original scene is 
		/// output together with the color information in the pixel shader.
		/// </remarks>
		public bool RebuildZBuffer { get; set; }
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="UpsampleFilter"/> class.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		public UpsampleFilter()
		{
			Mode = UpsamplingMode.Linear;
			DepthSensitivity = 1000;
			DepthThreshold = 1;
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <inheritdoc/>
		protected override void OnProcess(RenderContext context)
		{
			ViewVolume volume = null;
			if (RebuildZBuffer || Mode == UpsamplingMode.NearestDepth)
			{
				context.ThrowIfCameraMissing();
				volume = context.CameraNode.ViewVolume;
			}

			var sourceTexture = context.SourceTexture;
			_effect.SourceTexture.SetValue(sourceTexture);
			_effect.SourceSize.SetValue(new Vector2(sourceTexture.Width, sourceTexture.Height));

			var viewport = context.Viewport;
			_effect.TargetSize.SetValue(new Vector2(viewport.Width, viewport.Height));

			if (Mode == UpsamplingMode.Bilateral)
				_effect.DepthSensitivity.SetValue(DepthSensitivity);
			else if (Mode == UpsamplingMode.NearestDepth)
				_effect.DepthThreshold.SetValue(DepthThreshold / volume.Far);

			int techniqueIndex = (int)Mode;
			int passIndex = 0;
			if (context.SceneTexture != null)
			{
				_effect.SceneTexture.SetValue(context.SceneTexture);
				passIndex |= 1;
			}

			if (RebuildZBuffer)
			{
				passIndex |= 2;

				var nearBias = RebuildZBufferRenderer.NearBias;
				var farBias = RebuildZBufferRenderer.FarBias;

				// Compute biased projection for restoring the z-buffer.
				var rect = volume.Rectangle;
				var biasedProjection = Matrix44F.CreatePerspectiveOffCenter(
				  rect.Left,
				  rect.Right,
				  rect.Bottom,
				  rect.Top,
				  volume.Near * nearBias,
				  volume.Far * farBias);
				_effect.Projection.SetValue((Matrix)biasedProjection);
				_effect.CameraFar.SetValue(volume.Far);

				// PostProcessor.ProcessInternal sets the DepthStencilState to None.
				// --> Enable depth writes.
				DR.GraphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateAlways;
			}

			if (RebuildZBuffer || Mode >= UpsamplingMode.Bilateral)
			{
				context.ThrowIfGBuffer0Missing();
				_effect.DepthBuffer.SetValue(context.GBuffer0);
			}

			if (Mode >= UpsamplingMode.Bilateral)
			{
				// Render at half resolution into off-screen buffer.
				object dummy = null;
				var depthBufferHalf = dummy as Texture2D;
				if (depthBufferHalf == null)
				{
					string message = "Downsampled depth buffer is not set in render context. (The downsampled "
									 + "depth buffer (half width and height) is required by the UpsampleFilter."
									 + "It needs to be stored in RenderContext.Data[RenderContextKeys.DepthBufferHalf].)";
					throw new GraphicsException(message);
				}

				_effect.DepthBufferLow.SetValue(depthBufferHalf);
			}

			_effect.CurrentTechnique = _effect.Techniques[techniqueIndex];
			var pass = _effect.CurrentTechnique.Passes[passIndex];

			context.DrawFullScreenQuad(pass);

			_effect.SourceTexture.SetValue((Texture2D)null);
			_effect.SceneTexture.SetValue((Texture2D)null);
			_effect.DepthBuffer.SetValue((Texture2D)null);
			_effect.DepthBufferLow.SetValue((Texture2D)null);
		}
		#endregion
	}
}
