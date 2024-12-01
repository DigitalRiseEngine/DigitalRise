// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics;
using DigitalRise.Misc;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace DigitalRise.Rendering
{
	/// <summary>
	/// Provides information about the current render states.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The <see cref="RenderContext"/> is passed to every Render method. It is used to pass 
	/// information to a renderer, and it should contain all information that is required to render 
	/// an object or to perform a rendering step.
	/// </para>
	/// <para>
	/// Additional information can be stored in the <see cref="Data"/> dictionary.
	/// </para>
	/// <para>
	/// <strong>Cloning:</strong><br/>
	/// The render context is cloneable. <see cref="Clone"/> makes a copy of the current render 
	/// context. The new instance contains a new <see cref="Data"/> dictionary. The properties and the
	/// contents of the <see cref="Data"/> dictionary are copied by reference (shallow copy).
	/// </para>
	/// </remarks>
	public class RenderContext
	{
		private readonly RenderTarget2DViewportSized _gBuffer0Wrapper = new RenderTarget2DViewportSized(true, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
		private readonly RenderTarget2DViewportSized _gBuffer1Wrapper = new RenderTarget2DViewportSized(false, SurfaceFormat.Color, DepthFormat.None);
		private readonly RenderTarget2DViewportSized _lightBuffer0Wrapper = new RenderTarget2DViewportSized(false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
		private readonly RenderTarget2DViewportSized _lightBuffer1Wrapper = new RenderTarget2DViewportSized(false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
		private readonly RenderTarget2DViewportSized _outputBufferWrapper = new RenderTarget2DViewportSized(false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
		private readonly RenderTarget2DViewportSized _depthBufferHalfWrapper = new RenderTarget2DViewportSized(false, SurfaceFormat.Single, DepthFormat.None, type: RenderTarget2DViewportSizedType.Half);


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		#region ----- General -----


		/// <summary>
		/// Gets the total elapsed time.
		/// </summary>
		/// <value>The total elapsed time.</value>
		/// <inheritdoc cref="IGraphicsService.Time"/>
		public TimeSpan Time { get; set; }


		/// <summary>
		/// Gets the elapsed time since the last frame.
		/// </summary>
		/// <value>The elapsed time since the last frame.</value>
		/// <inheritdoc cref="IGraphicsService.DeltaTime"/>
		public TimeSpan DeltaTime { get; set; }


		/// <summary>
		/// Gets or sets the number of the current frame.
		/// </summary>
		/// <value>The number of the current frame.</value>
		/// <inheritdoc cref="IGraphicsService.Frame"/>
		public int Frame { get; set; }


		/// <summary>
		/// Gets or sets the current data object.
		/// </summary>
		/// <value>The current data object.</value>
		public object Object { get; set; }

		public RenderStatistics Statistics = new RenderStatistics();

		#endregion


		#region ----- Render State -----

		/// <summary>
		/// Gets or sets the source texture that contains the source image for the current render
		/// operation. 
		/// </summary>
		/// <value>
		/// The source texture; or <see langword="null"/> if there is no source texture.
		/// </value>
		/// <remarks>
		/// This property is used by <see cref="GraphicsScreen"/>s and <see cref="PostProcessor"/>s.
		/// The source texture is usually the content of the last render operation, e.g. the result
		/// of the last graphics screen or the last post-processor.
		/// </remarks>
		public Texture2D SourceTexture { get; set; }


		/// <summary>
		/// Gets or sets the texture that contains the rendered scene.
		/// </summary>
		/// <value>
		/// The scene texture; or <see langword="null"/> if there is no scene texture available.
		/// </value>
		/// <remarks>
		/// This property is usually <see langword="null"/>. However, in operations like off-screen
		/// rendering you need to combine an off-screen texture with the last scene texture. In this
		/// case <see cref="SourceTexture"/> will specify the off-screen texture and 
		/// <see cref="SceneTexture"/> will specify the last scene texture. 
		/// </remarks>
		public Texture2D SceneTexture { get; set; }
		#endregion


		#region ----- Deferred Rendering Buffers -----

		public RenderTarget2D GBuffer0 => _gBuffer0Wrapper.Target;
		public RenderTarget2D GBuffer1 => _gBuffer1Wrapper.Target;
		public RenderTarget2D DepthBufferHalf => _depthBufferHalfWrapper.Target;

		/// <summary>
		/// Diffuse light accumulation
		/// </summary>
		public RenderTarget2D LightBuffer0 => _lightBuffer0Wrapper.Target;

		/// <summary>
		/// Specular light accumulation
		/// </summary>
		public RenderTarget2D LightBuffer1 => _lightBuffer1Wrapper.Target;

		public RenderTarget2D Output => _outputBufferWrapper.Target;

		#endregion

		#region ----- Effect -----


		/// <summary>
		/// Gets or sets a string that identifies the current technique.
		/// </summary>
		/// <value>The string that identifies the current technique.</value>
		public string Technique { get; set; }

		#endregion

		#region ----- Scene -----

		/// <summary>
		/// Gets or sets the scene.
		/// </summary>
		/// <value>The scene.</value>
		public IScene Scene { get; set; }


		/// <summary>
		/// Gets or sets the active camera.
		/// </summary>
		/// <value>The active camera.</value>
		public CameraNode CameraNode { get; set; }


		/// <summary>
		/// Gets or sets the currently rendered scene node.
		/// </summary>
		/// <value>The currently rendered scene node.</value>
		public SceneNode SceneNode { get; set; }


		/// <summary>
		/// Gets or sets a scene node that provides additional context for the current render operation.
		/// </summary>
		/// <value>A scene node that provides additional information.</value>
		/// <remarks>
		/// <para>
		/// The purpose of the reference node depends on the current render operation. In most cases
		/// it will be <see langword="null"/>. Here are some examples where a reference node is useful:
		/// </para>
		/// <para>
		/// Shadow map rendering: When an object is rendered into the shadow map, the render context
		/// stores the currently rendered object in <see cref="SceneNode"/>. <see cref="ReferenceNode"/>
		/// contains the <see cref="LightNode"/> which owns the shadow map. This allows effect parameter
		/// bindings to find information about the light and the shadow.
		/// </para>
		/// <para>
		/// Render-to-texture: When an object is rendered into a texture of an 
		/// <see cref="RenderToTextureNode"/>, the render context stores the currently rendered object 
		/// in <see cref="SceneNode"/>. <see cref="ReferenceNode"/> contains the 
		/// <see cref="RenderToTextureNode"/>.
		/// </para>
		/// </remarks>
		public SceneNode ReferenceNode { get; set; }
		#endregion


		#region ----- Level of Detail (LOD) -----

		/// <summary>
		/// Gets or sets the global LOD bias.
		/// </summary>
		/// <value>The global LOD bias in the range [0, ∞[. The default value is 1.</value>
		/// <remarks>
		/// <para>
		/// The LOD bias is a factor that is multiplied to the distance of a scene node. It can be used 
		/// to increase or decrease the level of detail based on scene, performance, platform, or other 
		/// criteria.
		/// </para>
		/// <para>
		/// <strong>Performance Tips:</strong>
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// Increase the LOD bias during computationally intensive scenes (e.g. large number of 
		/// objects or characters on screen).
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Increase the LOD bias of fast moving cameras.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Increase/decrease LOD bias based on the games quality settings (e.g. minimal details vs.
		/// maximal details).
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Increase/decrease LOD bias based on platform (e.g. PC vs. mobile platforms).
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Increase/decrease LOD bias based on screen resolution. (Note: The LOD metric 
		/// "view-normalized distance" does not account for resolution changes.)
		/// </description>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// A <see cref="LodBias"/> of 0 forces all objects to be rendered with the highest level of 
		/// detail. A large <see cref="LodBias"/>, such as <see cref="float.PositiveInfinity"/>, forces
		/// all objects to be drawn with the lowest level of detail.
		/// </para>
		/// </remarks>
		/// <seealso cref="CameraNode.LodBias"/>
		public float LodBias
		{
			get { return _lodBias; }
			set
			{
				if (!(value >= 0))
					throw new ArgumentOutOfRangeException("value", "The LOD bias must be in the range [0, ∞[");

				_lodBias = value;
				ScaledLodHysteresis = _lodHysteresis * value;
			}
		}
		private float _lodBias;


		/// <summary>
		/// Gets or sets a value indicating whether smooth LOD transitions are enabled.
		/// </summary>
		/// <value>
		/// <see langword="true"/> to enable smooth LOD transitions; otherwise, <see langword="false"/>.
		/// The default value is <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// <para>
		/// When <see cref="LodBlendingEnabled"/> is <see langword="false"/> the renderer instantly 
		/// switches LODs, which can result in apparent "popping" of the geometry in the scene. The 
		/// property can be set to <see langword="true"/> to enable smooth transitions: The renderer 
		/// draws both LODs and blends them using screen-door transparency (stipple patterns).
		/// </para>
		/// <para>
		/// The length of the transition phase is determined by the <see cref="LodHysteresis"/>. If the
		/// LOD hysteresis is 0, blending is also disabled.
		/// </para>
		/// <para>
		/// Blending of LODs is expensive and increases the workload during LOD transitions. It is 
		/// therefore recommended to keep the LOD hysteresis small and to disable LOD blending during 
		/// computationally intensive scenes.
		/// </para>
		/// </remarks>
		public bool LodBlendingEnabled { get; set; }


		/// <summary>
		/// Gets or sets the camera that is used as reference for LOD calculations.
		/// </summary>
		/// <value>
		/// The camera that is used as reference for LOD calculations. 
		/// </value>
		/// <remarks>
		/// <para>
		/// LOD selection depends on the current camera (field-of-view) and the distance of the object 
		/// to the camera. The <see cref="LodCameraNode"/> references the camera that is used for LOD 
		/// computations.
		/// </para>
		/// <para>
		/// In most cases the same camera is used for rendering as well as LOD calculations. In this 
		/// case the same <see cref="SceneGraph.CameraNode"/> instance needs to be assigned to 
		/// <see cref="CameraNode"/> and <see cref="LodCameraNode"/>. LOD calculations will fail if the
		/// <see cref="LodCameraNode"/> is not set.
		/// </para>
		/// </remarks>
		public CameraNode LodCameraNode { get; set; }


		/// <summary>
		/// Gets or sets the LOD hysteresis, which is the distance over which an object transitions from
		/// on level of detail to the next level. (Needs to be normalized - see remarks.)
		/// </summary>
		/// <value>
		/// The LOD hysteresis. The value needs to be normalized - see remarks. The default value is 0.
		/// </value>
		/// <remarks>
		/// <para>
		/// The <i>LOD hysteresis</i> introduces a lag into the LOD transitions. Instead of switching 
		/// between LODs at a certain threshold distance, the distance for switching to the lower LOD is
		/// further away than the threshold distance and the distance for switching to the higher LOD is
		/// closer.
		/// </para>
		/// <para>
		/// Example: The LOD distance for LOD2 is 100. With an LOD hysteresis of 10, the object 
		/// transitions from LOD1 to LOD2 at distance 105, and from LOD2 to LOD1 at distance 95.
		/// </para>
		/// <para>
		/// The LOD hysteresis can be set to avoid flickering when the camera is near a threshold 
		/// distance.
		/// </para>
		/// <para>
		/// The value stored in this property is a <i>view-normalized distance</i> as described here: 
		/// <see cref="GraphicsHelper.GetViewNormalizedDistance(SceneGraph.SceneNode,SceneGraph.CameraNode)"/>. 
		/// The method <see cref="GraphicsHelper.GetViewNormalizedDistance(float, Matrix44F)"/> can be 
		/// used to convert a distance to a view-normalized distance. The resulting value is independent
		/// of the current field-of-view.
		/// </para>
		/// <para>
		/// <strong>Tips:</strong>
		/// It is recommended to keep the LOD hysteresis tight: When LOD blending (see 
		/// <see cref="LodBlendingEnabled"/>) is set, the renderer has to render both LODs during 
		/// transitions and blend them using screen-door transparency (stipple patterns).
		/// </para>
		/// <para>
		/// In most games the transition range depends on the average speed of the camera. A fast moving
		/// player (e.g. in a racing game) requires a larger LOD hysteresis than a slow moving player
		/// (e.g. a first-person shooter).
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="value"/> is negative, infinite or NaN.
		/// </exception>
		public float LodHysteresis
		{
			get { return _lodHysteresis; }
			set
			{
				if (!Numeric.IsZeroOrPositiveFinite(value))
					throw new ArgumentOutOfRangeException("value", "The LOD hysteresis must be 0 or a finite positive value.");

				_lodHysteresis = value;
				ScaledLodHysteresis = value * _lodBias;
			}
		}
		private float _lodHysteresis;

		// LOD hysteresis corrected by LOD bias.
		internal float ScaledLodHysteresis;
		#endregion


		#region ----- Shadows -----

		// Obsolete: Only kept for backward compatibility.
		/// <summary>
		/// Gets or sets the distance of the shadow near plane.
		/// </summary>
		/// <value>The distance of the shadow near plane.</value>
		/// <remarks>
		/// <para>
		/// When rendering cascaded shadow maps and a <see cref="CascadedShadow.MinLightDistance"/> is 
		/// set, the shadow projection does not match the camera projection. The shadow projection is a
		/// tight projection around the cascade. But the camera projection has a greater depth to catch
		/// all occluders in front of the cascade. <see cref="ShadowNear"/> specifies the distances from
		/// the camera to the near plane of the shadow projection.
		/// </para>
		/// <para>
		/// The value is temporarily set by the <see cref="ShadowMapRenderer"/>.
		/// </para>
		/// </remarks>
		internal float ShadowNear { get; set; }

		#endregion


		#region ----- Misc -----

		/// <summary>
		/// Gets or sets a user-defined object.
		/// </summary>
		/// <value>The a user-defined object.</value>
		/// <remarks>
		/// <see cref="UserData"/> can be used to store user-defined data with the render context.
		/// Additionally, <see cref="Data"/> can be used to store more custom data that can be accessed 
		/// using a string key.
		/// </remarks>
		public object UserData { get; set; }


		/// <summary>
		/// Gets a generic collection of name/value pairs which can be used to store custom data.
		/// </summary>
		/// <value>
		/// A generic collection of name/value pairs which can be used to store custom data.
		/// </value>
		/// <remarks>
		/// <see cref="UserData"/> can be used to store user-defined data with the render context.
		/// Additionally, <see cref="Data"/> can be used to store more custom data that can be
		/// accessed using a string key.
		/// </remarks>
		/// <seealso cref="RenderContextKeys"/>
		public Dictionary<string, object> Data
		{
			get
			{
				if (_data == null)
					_data = new Dictionary<string, object>();

				return _data;
			}
		}
		private Dictionary<string, object> _data;

		public RenderTargetPool RenderTargetPool { get; } = new RenderTargetPool();

		#endregion

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="RenderContext"/> class.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public RenderContext()
		{
			Reset();
		}

		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		public void Prepare()
		{
			_gBuffer0Wrapper.Update(this);
			_gBuffer1Wrapper.Update(this);
			_lightBuffer0Wrapper.Update(this);
			_lightBuffer1Wrapper.Update(this);
			_outputBufferWrapper.Update(this);
			_depthBufferHalfWrapper.Update(this);

			Technique = string.Empty;

			Statistics.Reset();
		}

		/// <summary>
		/// Resets the render context to default values.
		/// </summary>
		public void Reset()
		{
			SourceTexture = null;
			SceneTexture = null;
			Time = TimeSpan.Zero;
			DeltaTime = TimeSpan.Zero;
			Frame = -1;
			Object = null;
			_gBuffer0Wrapper.Reset(this);
			_gBuffer1Wrapper.Reset(this);
			_lightBuffer0Wrapper.Reset(this);
			_lightBuffer1Wrapper.Reset(this);
			_outputBufferWrapper.Reset(this);
			Technique = null;
			Scene = null;
			CameraNode = null;
			SceneNode = null;
			ReferenceNode = null;
			LodBias = 1;
			LodBlendingEnabled = false;
			LodCameraNode = null;
			LodHysteresis = 0;
			ShadowNear = float.NaN;
			UserData = null;

			if (_data != null)
				_data.Clear();
		}

		private void StatisticsTwoStrips()
		{
			++Statistics.EffectsSwitches;
			++Statistics.DrawCalls;
			Statistics.PrimitivesDrawn += 2;
			Statistics.VerticesDrawn += 4;
		}

		public void DrawQuad(EffectPass pass, VertexPositionTexture topLeft, VertexPositionTexture bottomRight)
		{
			pass.Apply();
			DR.GraphicsDevice.DrawQuad(topLeft, bottomRight);
			StatisticsTwoStrips();
		}

		public void DrawQuad(EffectPass pass, Rectangle rectangle, Vector2 texCoordTopLeft, Vector2 texCoordBottomRight)
		{
			pass.Apply();
			DR.GraphicsDevice.DrawQuad(rectangle, texCoordTopLeft, texCoordBottomRight);
			StatisticsTwoStrips();
		}


		public void DrawQuad(EffectPass pass, Rectangle rectangle)
		{
			DrawQuad(pass, rectangle, new Vector2(0, 0), new Vector2(1, 1));
		}

		public void DrawFullScreenQuad(EffectPass pass)
		{
			var viewport = DR.GraphicsDevice.Viewport;
			DrawQuad(pass, new Rectangle(0, 0, viewport.Width, viewport.Height));
		}

		public void DrawQuadFrustumRay(EffectPass pass, Rectangle rectangle, Vector2 texCoordTopLeft, Vector2 texCoordBottomRight, Vector3[] frustumFarCorners)
		{
			pass.Apply();
			DR.GraphicsDevice.DrawQuadFrustumRay(rectangle, texCoordTopLeft, texCoordBottomRight, frustumFarCorners);
			StatisticsTwoStrips();
		}

		public void DrawQuadFrustumRay(EffectPass pass, Rectangle rectangle, Vector3[] frustumFarCorners)
		{
			DrawQuadFrustumRay(pass, rectangle, new Vector2(0, 0), new Vector2(1, 1), frustumFarCorners);
		}

		public void DrawFullScreenQuadFrustumRay(EffectPass pass, Vector3[] frustumFarCorners)
		{
			var viewport = DR.GraphicsDevice.Viewport;
			DrawQuadFrustumRay(pass, new Rectangle(0, 0, viewport.Width, viewport.Height), frustumFarCorners);
		}

		/// <summary>
		/// Draws the <see cref="Submesh"/> using the currently active shader.
		/// </summary>
		/// <param name="submesh">The submesh.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="submesh"/> is <see langword="null"/>.
		/// </exception>
		/// <remarks>
		/// This method sets the <see cref="VertexDeclaration"/>, <see cref="Submesh.VertexBuffer"/>,
		/// and <see cref="Submesh.IndexBuffer"/> of the submesh and calls
		/// <see cref="GraphicsDevice.DrawIndexedPrimitives"/>. Effects are not handled in this method.
		/// The method assumes that the correct shader effect is already active.
		/// </remarks>
		public void Draw(EffectPass pass, Submesh submesh)
		{
			if (pass == null)
			{
				throw new ArgumentNullException(nameof(pass));
			}

			if (submesh == null)
			{
				throw new ArgumentNullException(nameof(submesh));
			}

			pass.Apply();
			submesh.Draw();

			++Statistics.DrawCalls;
			Statistics.PrimitivesDrawn += submesh.PrimitiveCount;
			Statistics.VerticesDrawn += submesh.VertexCount;
		}

		#endregion
	}
}
