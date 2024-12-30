// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise;
using DigitalRise.Data.Materials;
using DigitalRise.Data.Meshes;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Misc;
using DigitalRise.Misc.Encodings;
using DigitalRise.Rendering;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Sky;
using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Rendering.Sky
{
	internal class SkyboxEffectBinding : EffectWrapper
	{
		public EffectParameter TextureParameter { get; private set; }
		public EffectParameter WorldViewProjection { get; private set; }
		public EffectParameter Color { get; private set; }
		public EffectParameter RgbmMaxValue { get; private set; }
		public EffectParameter TextureSize { get; private set; }
		public EffectPass PassRgbToRgb { get; private set; }
		public EffectPass PassSRgbToRgb { get; private set; }
		public EffectPass PassRgbmToRgb { get; private set; }
		public EffectPass PassRgbToSRgb { get; private set; }
		public EffectPass PassSRgbToSRgb { get; private set; }
		public EffectPass PassRgbmToSRgb { get; private set; }

		public static SkyboxEffectBinding Instance { get; } = new SkyboxEffectBinding();

		public SkyboxEffectBinding() : base("Sky/Skybox")
		{
		}

		protected override void BindParameters(Effect effect)
		{
			base.BindParameters(effect);

			WorldViewProjection = effect.Parameters["WorldViewProjection"];
			Color = effect.Parameters["Color"];
			RgbmMaxValue = effect.Parameters["RgbmMax"];
			TextureSize = effect.Parameters["TextureSize"];
			TextureParameter = effect.Parameters["Texture"];
			PassRgbToRgb = effect.CurrentTechnique.Passes["RgbToRgb"];
			PassSRgbToRgb = effect.CurrentTechnique.Passes["SRgbToRgb"];
			PassRgbmToRgb = effect.CurrentTechnique.Passes["RgbmToRgb"];
			PassRgbToSRgb = effect.CurrentTechnique.Passes["RgbToSRgb"];
			PassSRgbToSRgb = effect.CurrentTechnique.Passes["SRgbToSRgb"];
			PassRgbmToSRgb = effect.CurrentTechnique.Passes["RgbmToSRgb"];
		}
	}

	/// <summary>
	/// Renders a cube map ("skybox") into the background of the current render target.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A "skybox" is a cube map that is used as the background of a scene. A skybox is usually drawn 
	/// after all opaque objects to fill the background.
	/// </para>
	/// <para>
	/// <strong>Render Target and Viewport:</strong><br/>
	/// This renderer renders into the current render target and viewport of the graphics device.
	/// </para>
	/// </remarks>
	public static class SkyboxRendererInternal
	{
		// TODO: Remove previous SkyboxRenderer and rename this class to SkyboxRenderer.

		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		#region ----- Reach -----
		private static BasicEffect _effectReach;
		private static VertexPositionTexture[] _faceVertices;
		#endregion

		#region ----- HiDef -----

		private static Submesh _submesh;
		#endregion

		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="SkyboxRendererInternal"/> class.
		/// </summary>
		/// </exception>
		static SkyboxRendererInternal()
		{

			if (DR.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
				InitializeReach();
			else
				InitializeHiDef();
		}
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
			if (nodes.Count == 0)
				return;

			bool reach = (DR.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach);
			var _effect = reach ? _effectReach : SkyboxEffectBinding.Instance.Effect;
			_effect.Validate();
			context.ThrowIfCameraMissing();

			// Update SceneNode.LastFrame for all visible nodes.
			int frame = context.Frame;
			var cameraNode = context.CameraNode;
			cameraNode.LastFrame = frame;

			for (int i = 0; i < numberOfNodes; i++)
			{
				var node = nodes[i] as SkyboxNode;
				if (node == null)
					continue;

				// SkyboxNode is visible in current frame.
				node.LastFrame = frame;

				if (node.Texture != null)
				{
					if (reach)
						RenderReach(node, context);
					else
						RenderHiDef(node, context);
				}
			}
		}


		#region ----- Reach -----

		public static void InitializeReach()
		{
			// Use BasicEffect for rendering.
			var graphicsDevice = DR.GraphicsDevice;
			_effectReach = new BasicEffect(graphicsDevice)
			{
				FogEnabled = false,
				LightingEnabled = false,
				TextureEnabled = true,
				VertexColorEnabled = false
			};

			// Create single face of skybox.
			_faceVertices = new[]
			{
				new VertexPositionTexture(new Vector3(1, -1, -1), new Vector2(0, 1)),
				new VertexPositionTexture(new Vector3(1, 1, -1), new Vector2(0, 0)),
				new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1)),
				new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0)),
			};
		}


		public static void RenderReach(SkyboxNode node, RenderContext context)
		{
			var graphicsDevice = DR.GraphicsDevice;

			var savedRenderState = new RenderStateSnapshot();
			graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			graphicsDevice.BlendState = node.EnableAlphaBlending ? BlendState.AlphaBlend : BlendState.Opaque;
			graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

			// Change viewport to render all pixels at max z.
			var originalViewport = graphicsDevice.Viewport;
			var viewport = originalViewport;
			viewport.MinDepth = viewport.MaxDepth;
			graphicsDevice.Viewport = viewport;

			var cameraNode = context.CameraNode;
			var view = cameraNode.View;
			view.Translation = Vector3.Zero;
			var projection = cameraNode.ViewVolume.Projection;

			var basicEffect = _effectReach;
			basicEffect.View = (Matrix)view;
			basicEffect.Projection = (Matrix)projection;
			basicEffect.DiffuseColor = node.Color.ToVector3();
			basicEffect.Alpha = node.EnableAlphaBlending ? node.Alpha : 1;

			// Scale skybox such that it lies within view frustum:
			//   distance of a skybox corner = √3
			//   √3 * scale = far 
			//   => scale = far / √3
			// (Note: If  near > far / √3  then the skybox will be clipped.)
			float scale = cameraNode.ViewVolume.Far * 0.577f;

			var orientation = node.PoseWorld.Orientation;

			// Positive X
			basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.PositiveX);
			basicEffect.World = (Matrix)new Matrix44F(orientation * scale, Vector3.Zero);
			basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

			// Negative X      
			// transform = scale * rotY(180°)
			var transform = new Matrix33F(-scale, 0, 0, 0, scale, 0, 0, 0, -scale);
			basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.NegativeX);
			basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3.Zero);
			basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

			// Positive Y
			// transform = scale * rotX(90°) * rotY(90°)
			transform = new Matrix33F(0, 0, scale, scale, 0, 0, 0, scale, 0);
			basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.PositiveY);
			basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3.Zero);
			basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

			// Negative Y
			// transform = scale * rotX(-90°) * rotY(90°)
			transform = new Matrix33F(0, 0, scale, -scale, 0, 0, 0, -scale, 0);
			basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.NegativeY);
			basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3.Zero);
			basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

			// Cube maps are left-handed, where as the world is right-handed!

			// Positive Z (= negative Z in world space)
			// transform = scale * rotY(90°)
			transform = new Matrix33F(0, 0, scale, 0, scale, 0, -scale, 0, 0);
			basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.PositiveZ);
			basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3.Zero);
			basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

			// Negative Z (= positive Z in world space)
			// transform = scale * rotY(-90°)
			transform = new Matrix33F(0, 0, -scale, 0, scale, 0, scale, 0, 0);
			basicEffect.Texture = GetTexture2D(graphicsDevice, node.Texture, CubeMapFace.NegativeZ);
			basicEffect.World = (Matrix)new Matrix44F(orientation * transform, Vector3.Zero);
			basicEffect.CurrentTechnique.Passes[0].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _faceVertices, 0, 2);

			graphicsDevice.Viewport = originalViewport;
			savedRenderState.Restore();
		}


		public static Texture2D GetTexture2D(GraphicsDevice graphicsDevice, TextureCube textureCube, CubeMapFace cubeMapFace)
		{
			// Unfortunately, we cannot treat the TextureCube as Texture2D[] in XNA.
			// We could try to copy all faces into a 3x2 Texture2D, but this is problematic:
			//  - Extracting DXT compressed faces is difficult.
			//  - Additional texel border required for correct texture filtering at edges.
			//  + The skybox could be rendered with a single draw call.
			//
			// --> Manually convert TextureCube to Texture2D[6] and store array in Tag.

			var faces = textureCube.Tag as Texture2D[];
			if (faces == null || faces.Length != 6)
			{
				if (textureCube.Tag != null)
					throw new GraphicsException("The SkyboxRenderer (Reach profile) needs to store information in Tag property of the skybox texture, but the Tag property is already in use.");

				faces = new Texture2D[6];
				var size = textureCube.Size;

				int numberOfBytes;
				switch (textureCube.Format)
				{
					case SurfaceFormat.Color:
						numberOfBytes = size * size * 4;
						break;
					case SurfaceFormat.Dxt1:
						numberOfBytes = size * size / 2;
						break;
					default:
						throw new GraphicsException("The SkyboxRenderer (Reach profile) only supports the following surface formats: Color, Dxt1.");
				}

				var face = new byte[numberOfBytes];
				for (int i = 0; i < 6; i++)
				{
					var texture2D = new Texture2D(graphicsDevice, size, size, false, textureCube.Format);
					textureCube.GetData((CubeMapFace)i, face);
					texture2D.SetData(face);
					faces[i] = texture2D;
				}

				textureCube.Tag = faces;
				textureCube.Disposing += OnTextureCubeDisposing;
			}

			return faces[(int)cubeMapFace];
		}


		public static void OnTextureCubeDisposing(object sender, EventArgs eventArgs)
		{
			var textureCube = (TextureCube)sender;
			var faces = textureCube.Tag as Texture2D[];
			if (faces != null)
				foreach (var face in faces)
					face.Dispose();
		}
		#endregion


		#region ----- HiDef -----

		public static void InitializeHiDef()
		{
			_submesh = InternalPrimitives.UntexturedBox;
		}


		public static void RenderHiDef(SkyboxNode node, RenderContext context)
		{
			var graphicsDevice = DR.GraphicsDevice;

			var savedRenderState = new RenderStateSnapshot();
			graphicsDevice.RasterizerState = RasterizerState.CullNone;
			graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
			graphicsDevice.BlendState = node.EnableAlphaBlending ? BlendState.AlphaBlend : BlendState.Opaque;

			bool sourceIsFloatingPoint = TextureHelper.IsFloatingPointFormat(node.Texture.Format);

			// Set sampler state. (Floating-point textures cannot use linear filtering. (XNA would throw an exception.))
			if (sourceIsFloatingPoint)
				graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
			else
				graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

			var cameraNode = context.CameraNode;
			Matrix44F view = cameraNode.View;
			Matrix44F projection = cameraNode.ViewVolume.Projection;

			// Cube maps are left handed --> Sample with inverted z. (Otherwise, the 
			// cube map and objects or texts in it are mirrored.)
			var mirrorZ = Matrix44F.CreateScale(1, 1, -1);
			Matrix33F orientation = node.PoseWorld.Orientation;

			var effect = SkyboxEffectBinding.Instance;
			effect.WorldViewProjection.SetValue((Matrix)(projection * view * new Matrix44F(orientation, Vector3.Zero) * mirrorZ));

			Vector4 color = node.EnableAlphaBlending
							? new Vector4(node.Color.ToVector3() * node.Alpha, node.Alpha) // Premultiplied
							: new Vector4(node.Color.ToVector3(), 1);                      // Opaque
			effect.Color.SetValue(color);
			effect.TextureParameter.SetValue(node.Texture);

			EffectPass pass;
			if (node.Encoding is RgbEncoding)
			{
				effect.TextureSize.SetValue(node.Texture.Size);
				if (context.IsHdr)
				{
					pass = effect.PassRgbToRgb;
				}
				else
				{
					pass = effect.PassRgbToSRgb;
				}
			}
			else if (node.Encoding is SRgbEncoding)
			{
				if (!sourceIsFloatingPoint)
				{
					if (context.IsHdr)
					{
						pass = effect.PassSRgbToRgb;
					}
					else
					{
						pass = effect.PassSRgbToSRgb;
					}
				}
				else
				{
					throw new GraphicsException("sRGB encoded skybox cube maps must not use a floating point format.");
				}
			}
			else if (node.Encoding is RgbmEncoding)
			{
				float max = GraphicsHelper.ToGamma(((RgbmEncoding)node.Encoding).Max);
				effect.RgbmMaxValue.SetValue(max);

				if (context.IsHdr)
				{
					pass = effect.PassRgbmToRgb;
				}
				else
				{
					pass = effect.PassRgbmToSRgb;
				}
			}
			else
			{
				throw new NotSupportedException("The SkyBoxRenderer supports only RgbEncoding, SRgbEncoding and RgbmEncoding.");
			}

			context.Draw(pass, _submesh);
			savedRenderState.Restore();
		}
		#endregion

		#endregion
	}
}
