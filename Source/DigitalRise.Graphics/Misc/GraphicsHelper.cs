// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Misc
{
	/// <summary>
	/// Provides helper methods for graphics-related tasks.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class provides several default render state objects (e.g. <see cref="BlendStateAdd"/>).
	/// These default render state objects are only created once per graphics device and are reused.
	/// This objects must not be modified.
	/// </para>
	/// </remarks>
	public static partial class GraphicsHelper
	{
		/// <summary>
		/// The maximum number of textures that can be simultaneously bound to the fixed-function 
		/// pipeline sampler stages.
		/// </summary>
		/// <remarks>
		/// Shader model 2.0 - 4.0 support 16 samplers.
		/// </remarks>
		private static int _maxSimultaneousTextures = 16;  // Initialized and used in ResetTextures().


		/// <summary>
		/// A bias matrix that converts a vector from clip space to texture space.
		/// </summary>
		/// <remarks>
		/// (x, y) coordinates in clip space range from (-1, -1) at the bottom left to (1, 1) at the top
		/// right. For texturing the top left should be (0, 0) and the bottom right should be (1, 1).
		/// </remarks>
		public static readonly Matrix44F ProjectorBiasMatrix = new Matrix44F(0.5f, 0, 0, 0.5f,
																				0, -0.5f, 0, 0.5f,
																				0, 0, 1, 0,
																				0, 0, 0, 1);

		private static readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[4];
		private static readonly RenderTargetBinding[] _getRenderTargetBindings = new RenderTargetBinding[16];

		/// <summary>
		/// Gets the max number of primitives per draw call.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <returns>The max number of primitives per draw call.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsDevice"/> is <see langword="null"/>.
		/// </exception>
		internal static int GetMaxPrimitivesPerCall(this GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException("graphicsDevice");

			if (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
				return 1048575;

			return 65535;
		}


		/// <summary>
		/// The gamma value.
		/// </summary>
		/// <remarks>
		/// Use 2.0 for approximate gamma (default) and 2.2 for exact gamma.
		/// </remarks>
		internal const float Gamma = 2.0f; // Needs to be kept in sync with Common.fxh.


		/// <overloads>
		/// <summary>
		/// Converts a color value from gamma space to linear space.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Converts a color value from gamma space to linear space.
		/// </summary>
		/// <param name="color">The color in gamma space.</param>
		/// <returns>The color value in linear space.</returns>
		internal static float FromGamma(float color)
		{
			return (float)Math.Pow(color, Gamma);
		}


		/// <summary>
		/// Converts a color value from gamma space to linear space.
		/// </summary>
		/// <param name="color">The color in gamma space.</param>
		/// <returns>The color value in linear space.</returns>
		internal static Vector3 FromGamma(Vector3 color)
		{
			color.X = FromGamma(color.X);
			color.Y = FromGamma(color.Y);
			color.Z = FromGamma(color.Z);
			return color;
		}


		/// <summary>
		/// Converts a color value from gamma space to linear space.
		/// </summary>
		/// <param name="color">The color in gamma space.</param>
		/// <returns>The color value in linear space.</returns>
		internal static Vector4 FromGamma(Vector4 color)
		{
			color.X = FromGamma(color.X);
			color.Y = FromGamma(color.Y);
			color.Z = FromGamma(color.Z);
			return color;
		}


		/// <overloads>
		/// <summary>
		/// Converts a color value from linear space to gamma space.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Converts a color value from linear space to gamma space.
		/// </summary>
		/// <param name="color">The color in linear space.</param>
		/// <returns>The color value in gamma space.</returns>
		internal static float ToGamma(float color)
		{
			return (float)Math.Pow(color, 1.0f / Gamma);
		}


		/// <summary>
		/// Converts a color value from linear space to gamma space.
		/// </summary>
		/// <param name="color">The color in linear space.</param>
		/// <returns>The color value in gamma space.</returns>
		internal static Vector3 ToGamma(Vector3 color)
		{
			color.X = ToGamma(color.X);
			color.Y = ToGamma(color.Y);
			color.Z = ToGamma(color.Z);
			return color;
		}


		/// <summary>
		/// Converts a color value from linear space to gamma space.
		/// </summary>
		/// <param name="color">The color in linear space.</param>
		/// <returns>The color value in gamma space.</returns>
		internal static Vector4 ToGamma(Vector4 color)
		{
			color.X = ToGamma(color.X);
			color.Y = ToGamma(color.Y);
			color.Z = ToGamma(color.Z);
			return color;
		}


		/// <summary>
		/// A matrix which converts colors from the CIE XYZ color space to the sRGB color space.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Case of XYZ is important.")]
		public static readonly Matrix33F XYZToRGB = new Matrix33F(3.240479f, -1.53715f, -0.49853f,
																  -0.969256f, 1.875991f, 0.041556f,
																   0.055648f, -0.204043f, 1.057311f);


		/// <summary>
		/// A matrix which converts colors from the sRGB color space to the CIE XYZ color space.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Case of XYZ is important.")]
		public static readonly Matrix33F RGBToXYZ = new Matrix33F(0.412453f, 0.357579f, 0.180420f,
																  0.212671f, 0.715159f, 0.072167f,
																  0.019333f, 0.119193f, 0.950226f);
		// See also http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html.


		/// <summary>
		/// The weights for red, green and blue to convert a color to a luminance.
		/// </summary>
		/// <remarks>
		/// These weights were chosen according to ITU Rec 709 (HDTV; same as sRGB). To convert a color
		/// to luminance use the dot product: <c>Vector3.Dot(color, LuminanceWeights)</c>
		/// </remarks>
		public static readonly Vector3 LuminanceWeights = new Vector3(0.2126f, 0.7152f, 0.0722f);


		/// <summary>
		/// Converts a color from CIE XYZ color space to CIE Yxy.
		/// </summary>
		/// <param name="XYZ">The XYZ.</param>
		/// <param name="Y">Y.</param>
		/// <param name="x">x.</param>
		/// <param name="y">y.</param>
		internal static void ConvertXYZToYxy(Vector3 XYZ, out float Y, out float x, out float y)
		{
			float X = XYZ.X;
			Y = XYZ.Y;
			float Z = XYZ.Z;

			x = X / (X + Y + Z);
			y = Y / (X + Y + Z);
		}


		/// <summary>
		/// Converts a color from the CIE Yxy color space to CIE XYZ.
		/// </summary>
		/// <param name="Y">The Y.</param>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns>The XYZ color value.</returns>
		internal static Vector3 ConvertYxyToXYZ(float Y, float x, float y)
		{
			Vector3 result;
			result.X = x * Y / y;
			result.Y = Y;
			result.Z = (1 - x - y) * Y / y;
			return result;
		}


		/// <summary>
		/// Gets a unique color for an object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns>A unique color.</returns>
		/// <remarks>
		/// The color is created from the hash code of the object. For most cases this is unique but
		/// this is not guaranteed.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
		public static Color GetUniqueColor(object obj)
		{
			if (obj == null)
				return Color.Black;

			int hashCode = obj.GetHashCode();

			byte r = (byte)((hashCode & 0x00ff0000) >> 16);
			byte g = (byte)((hashCode & 0x0000ff00) >> 8);
			byte b = (byte)((hashCode & 0x000000ff));
			return new Color(r, g, b);
		}


		/// <summary>
		/// Sets the textures of all samplers to <see langword="null"/>.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsDevice"/> is <see langword="null"/>.
		/// </exception>
		public static void ResetTextures(this GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException("graphicsDevice");

			try
			{
				for (int i = 0; i < _maxSimultaneousTextures; i++)
					graphicsDevice.Textures[i] = null;
			}
			catch (IndexOutOfRangeException)
			{
				// MaxSimultaneousTextures is 16 in XNA. On iOS and Android it can be less.
				// --> Find out how many texture we actually have. 
				try
				{
					_maxSimultaneousTextures = 0;
					while (true)
					{
						graphicsDevice.Textures[_maxSimultaneousTextures] = null;
						_maxSimultaneousTextures++;
					}
				}
				catch (IndexOutOfRangeException)
				{
				}
			}
		}


		/// <summary>
		/// Sets the textures of all samplers to <see cref="SamplerState.PointWrap"/>
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <remarks>
		/// This method can be used if the sampler states must be reset. For example, in XNA if an 
		/// effect change the MipMapLodBias, then the same bias will be used by the next effects
		/// unless they explicitly set the bias back to 0. In this case, this method helps to reset
		/// the sampler states.
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsDevice"/> is <see langword="null"/>.
		/// </exception>
		internal static void ResetSamplerStates(this GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException("graphicsDevice");

			try
			{
				for (int i = 0; i < _maxSimultaneousTextures; i++)
					graphicsDevice.SamplerStates[i] = SamplerState.PointWrap;
			}
			catch (IndexOutOfRangeException)
			{
				// MaxSimultaneousTextures is 16 in XNA. On iOS and Android it can be less.
				// --> Find out how many texture we actually have. 
				try
				{
					_maxSimultaneousTextures = 0;
					while (true)
					{
						graphicsDevice.SamplerStates[_maxSimultaneousTextures] = SamplerState.PointWrap;
						_maxSimultaneousTextures++;
					}
				}
				catch (IndexOutOfRangeException)
				{
				}
			}
		}


		/// <summary>
		/// Creates a texture containing the content of the current back buffer.
		/// (Only available in the HiDef profile.)
		/// </summary>
		/// <param name="graphicsDevice">The graphics device.</param>
		/// <returns>A texture with content of the back buffer.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsDevice"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The current graphics profile is "Reach".
		/// </exception>
		public static Texture2D TakeScreenshot(this GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
				throw new ArgumentNullException("graphicsDevice");

			if (graphicsDevice.GraphicsProfile != GraphicsProfile.HiDef)
				throw new NotSupportedException("TakeScreenshot() is not supported in the Reach profile.");

			int width = graphicsDevice.PresentationParameters.BackBufferWidth;
			int height = graphicsDevice.PresentationParameters.BackBufferHeight;

			// Get current back buffer data.
			Color[] backBuffer = new Color[width * height];
			graphicsDevice.GetBackBufferData(backBuffer);

			// Create a texture with the back buffer data.
			Texture2D texture = new Texture2D(graphicsDevice, width, height, false, graphicsDevice.PresentationParameters.BackBufferFormat);
			texture.SetData(backBuffer);

			return texture;
		}

		public static int GetSize(this VertexElementFormat elementFormat)
		{
			switch (elementFormat)
			{
				case VertexElementFormat.Single:
					return 4;
				case VertexElementFormat.Vector2:
					return 8;
				case VertexElementFormat.Vector3:
					return 12;
				case VertexElementFormat.Vector4:
					return 16;
				case VertexElementFormat.Color:
					return 4;
				case VertexElementFormat.Byte4:
					return 4;
				case VertexElementFormat.Short2:
					return 4;
				case VertexElementFormat.Short4:
					return 8;
				case VertexElementFormat.NormalizedShort2:
					return 4;
				case VertexElementFormat.NormalizedShort4:
					return 8;
				case VertexElementFormat.HalfVector2:
					return 4;
				case VertexElementFormat.HalfVector4:
					return 8;
			}

			return 0;
		}

		public static VertexBuffer CreateVertexBuffer<T>(this T[] vertices) where T : struct, IVertexType
		{
			var device = DR.GraphicsDevice;
			var result = new VertexBuffer(device, new T().VertexDeclaration, vertices.Length, BufferUsage.None);
			result.SetData(vertices);

			return result;
		}

		public static VertexBuffer CreateVertexBuffer(this Vector3[] vertices)
		{
			var device = DR.GraphicsDevice;
			var result = new VertexBuffer(device, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);
			result.SetData(vertices);

			return result;
		}

		public static IndexBuffer CreateIndexBuffer(this ushort[] indices)
		{
			var device = DR.GraphicsDevice;
			var result = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
			result.SetData(indices);

			return result;
		}

		public static IndexBuffer CreateIndexBuffer(this int[] indices)
		{
			var device = DR.GraphicsDevice;
			var result = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
			result.SetData(indices);

			return result;
		}

		public static IEnumerable<Vector3> GetPositions(this VertexPositionNormalTexture[] vertices) => (from v in vertices select v.Position);
		public static IEnumerable<Vector3> GetPositions(this VertexPositionTexture[] vertices) => (from v in vertices select v.Position);
		public static IEnumerable<Vector3> GetPositions(this VertexPositionNormal[] vertices) => (from v in vertices select v.Position);
		public static IEnumerable<Vector3> GetPositions(this VertexPosition[] vertices) => (from v in vertices select v.Position);

		public static BoundingBox BuildBoundingBox(this VertexPositionNormalTexture[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this VertexPositionTexture[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this VertexPositionNormal[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this VertexPosition[] vertices) => BoundingBox.CreateFromPoints(vertices.GetPositions());
		public static BoundingBox BuildBoundingBox(this Vector3[] vertices) => BoundingBox.CreateFromPoints(vertices);

		public static void SetRenderTargets(this GraphicsDevice device, RenderTarget2D target1,  RenderTarget2D target2)
		{
			_renderTargetBindings[0] = new RenderTargetBinding(target1);
			_renderTargetBindings[1] = new RenderTargetBinding(target2);
			
			device.SetRenderTargets(_renderTargetBindings);
		}

		public static void ResetRenderTargets(this GraphicsDevice device)
		{
			_renderTargetBindings[0] = new RenderTargetBinding();
			_renderTargetBindings[1] = new RenderTargetBinding();
		}

		public static Texture GetCurrentRenderTarget(this GraphicsDevice device)
		{
#if FNA
			var count = device.GetRenderTargetsNoAllocEXT(null);
			if (count == 0)
			{
				return null;
			}
			
			device.GetRenderTargetsNoAllocEXT(_getRenderTargetBindings);

			var result = _getRenderTargetBindings[0].RenderTarget;

			// Reset temporary array
			for(var i = 0; i < count; ++i)
			{
				_getRenderTargetBindings[i] = new RenderTargetBinding();
			}
#else
			device.GetRenderTargets(_getRenderTargetBindings);

			var result = _getRenderTargetBindings[0].RenderTarget;

			// Reset temporary array
			for (var i = 0; i < device.RenderTargetCount; ++i)
			{
				_getRenderTargetBindings[i] = new RenderTargetBinding();
			}
#endif

			return result;
		}

		public static bool IsCurrentRenderTargetHdr(this GraphicsDevice device)
		{
			var target = device.GetCurrentRenderTarget();

			return target != null && target.Format == SurfaceFormat.HdrBlendable;
		}

		public static Vector2 GetViewportSize(this GraphicsDevice device)
		{
			var viewport = device.Viewport;

			return new Vector2(viewport.Width, viewport.Height);
		}
	}
}
