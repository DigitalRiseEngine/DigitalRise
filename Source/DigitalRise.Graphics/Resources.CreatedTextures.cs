using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise
{
	partial class Resources
	{
		public static Texture2D _defaultTexture2DBlack;
		public static Texture2D _defaultTexture2DBlack4F;
		public static Texture2D _defaultTexture2DWhite;
		public static Texture3D _defaultTexture3DBlack;
		public static Texture3D _defaultTexture3DWhite;
		public static TextureCube _defaultTextureCubeBlack;
		public static TextureCube _defaultTextureCubeWhite;
		public static Texture2D _defaultNormalTexture;

		public static Texture2D DefaultTexture2DBlack
		{
			get
			{
				if (_defaultTexture2DBlack == null || _defaultTexture2DBlack.IsDisposed)
				{
					_defaultTexture2DBlack = new Texture2D(DR.GraphicsDevice, 1, 1);
					_defaultTexture2DBlack.SetData(new[] { Color.Black });
				}

				return _defaultTexture2DBlack;
			}
		}


		/// <summary>
		/// Gets a black 2D texture with 1x1 pixels using Vector4 format.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>A black 2D texture with 1x1 pixels.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static Texture2D DefaultTexture2DBlack4F
		{
			get
			{
				if (_defaultTexture2DBlack4F == null || _defaultTexture2DBlack4F.IsDisposed)
				{
					_defaultTexture2DBlack4F = new Texture2D(DR.GraphicsDevice, 1, 1, false, SurfaceFormat.Vector4);
					_defaultTexture2DBlack4F.SetData(new[] { Vector4.Zero });
				}

				return _defaultTexture2DBlack4F;
			}
		}


		/// <summary>
		/// Gets a white 2D texture with 1x1 pixels.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>A white 2D texture with 1x1 pixels.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static Texture2D DefaultTexture2DWhite
		{
			get
			{
				if (_defaultTexture2DWhite == null || _defaultTexture2DWhite.IsDisposed)
				{
					_defaultTexture2DWhite = new Texture2D(DR.GraphicsDevice, 1, 1);
					_defaultTexture2DWhite.SetData(new[] { Color.White });
				}

				return _defaultTexture2DWhite;
			}
		}


		/// <summary>
		/// Gets a 1x1 normal map. The normal vector is (0, 0, 1).
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>
		/// A 1x1 normal map. The normal stored in the map is (0, 0, 1).
		/// The returned normal map can be used for effects which expect an uncompressed normal map
		/// and for effects which expect a DXT5nm normal map.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static Texture2D DefaultNormalTexture
		{
			get
			{
				if (_defaultNormalTexture == null || _defaultNormalTexture.IsDisposed)
				{
					_defaultNormalTexture = new Texture2D(DR.GraphicsDevice, 1, 1);

					// Components of a normal vector are in the range [-1, 1].
					// The components are compressed to the range [0, 1].
					// normal = (0, 0, 1) --> (0.5, 0.5, 1.0)
					// DXT5nm compression stores the x-component in the Alpha channel.
					// The following constant works for most cases (no compression, DXT5nm compression).
					_defaultNormalTexture.SetData(new[] { new Color(128, 128, 255, 128) });
				}

				return _defaultNormalTexture;
			}
		}


		/// <summary>
		/// Gets a black 3D texture with 1x1 pixels.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>A black 3D texture with 1x1 pixels.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static Texture3D DefaultTexture3DBlack
		{
			get
			{
				if (_defaultTexture3DBlack == null || _defaultTexture3DBlack.IsDisposed)
				{
					_defaultTexture3DBlack = new Texture3D(DR.GraphicsDevice, 1, 1, 1, false, SurfaceFormat.Color);
					_defaultTexture3DBlack.SetData(new[] { Color.Black });
				}

				return _defaultTexture3DBlack;
			}
		}


		/// <summary>
		/// Gets a white 3D texture with 1x1 pixels.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>A white 3D texture with 1x1 pixels.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static Texture3D DefaultTexture3DWhite
		{
			get
			{
				if (_defaultTexture3DWhite == null || _defaultTexture3DWhite.IsDisposed)
				{
					_defaultTexture3DWhite = new Texture3D(DR.GraphicsDevice, 1, 1, 1, false, SurfaceFormat.Color);
					_defaultTexture3DWhite.SetData(new[] { Color.White });
				}

				return _defaultTexture3DWhite;
			}
		}


		/// <summary>
		/// Gets a cubemap texture where each face consists of 1 black pixel.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>A cubemap texture where each face consists of 1 black pixel.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static TextureCube DefaultTextureCubeBlack
		{
			get
			{
				if (_defaultTextureCubeBlack == null || _defaultTextureCubeBlack.IsDisposed)
				{
					_defaultTextureCubeBlack = new TextureCube(DR.GraphicsDevice, 1, false, SurfaceFormat.Color);
					var black = new[] { Color.Black };
					_defaultTextureCubeBlack.SetData(CubeMapFace.PositiveX, black);
					_defaultTextureCubeBlack.SetData(CubeMapFace.PositiveY, black);
					_defaultTextureCubeBlack.SetData(CubeMapFace.PositiveZ, black);
					_defaultTextureCubeBlack.SetData(CubeMapFace.NegativeX, black);
					_defaultTextureCubeBlack.SetData(CubeMapFace.NegativeY, black);
					_defaultTextureCubeBlack.SetData(CubeMapFace.NegativeZ, black);
				}

				return _defaultTextureCubeBlack;
			}
		}


		/// <summary>
		/// Gets a cubemap texture where each face consists of 1 white pixel.
		/// </summary>
		/// <param name="graphicsService">The graphics service.</param>
		/// <returns>A cubemap texture where each face consists of 1 white pixel.</returns>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="graphicsService"/> is <see langword="null"/>.
		/// </exception>
		public static TextureCube DefaultTextureCubeWhite
		{
			get
			{
				if (_defaultTextureCubeWhite == null || _defaultTextureCubeWhite.IsDisposed)
				{
					_defaultTextureCubeWhite = new TextureCube(DR.GraphicsDevice, 1, false, SurfaceFormat.Color);
					var white = new[] { Color.White };
					_defaultTextureCubeWhite.SetData(CubeMapFace.PositiveX, white);
					_defaultTextureCubeWhite.SetData(CubeMapFace.PositiveY, white);
					_defaultTextureCubeWhite.SetData(CubeMapFace.PositiveZ, white);
					_defaultTextureCubeWhite.SetData(CubeMapFace.NegativeX, white);
					_defaultTextureCubeWhite.SetData(CubeMapFace.NegativeY, white);
					_defaultTextureCubeWhite.SetData(CubeMapFace.NegativeZ, white);
				}

				return _defaultTextureCubeWhite;
			}
		}
	}
}
