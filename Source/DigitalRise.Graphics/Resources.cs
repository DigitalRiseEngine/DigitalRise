using AssetManagementBase;
using DigitalRise.Misc;
using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace DigitalRise
{

	internal static partial class Resources
	{
		private static readonly AssetManager _assetManager = AssetManager.CreateResourceAssetManager(Assembly, "Resources");

		private static Texture2D _normalsFittingTexture;
		private static FontSystem _fontSystem;
		private static SpriteBatch _spriteBatch;

		public static SpriteBatch SpriteBatch
		{
			get
			{
				if (_spriteBatch == null)
				{
					_spriteBatch = new SpriteBatch(DR.GraphicsDevice);
				}

				_spriteBatch.Validate();
				return _spriteBatch;
			}
		}

		private static Assembly Assembly
		{
			get
			{
				return typeof(Resources).Assembly;
			}
		}

		public static FontSystem DefaultFontSystem
		{
			get
			{
				if (_fontSystem == null)
				{
					_fontSystem = _assetManager.LoadFontSystem("Fonts/Inter-Regular.ttf");
				}

				return _fontSystem;
			}
		}

		public static SpriteFontBase DefaultFont => DefaultFontSystem.GetFont(32);

		public static Texture2D NormalsFittingTexture
		{
			get
			{
				if (_normalsFittingTexture == null)
				{
					_normalsFittingTexture = _assetManager.LoadTexture2D(DR.GraphicsDevice, "Images.NormalsFittingTexture.png");
				}

				return _normalsFittingTexture;
			}
		}
	}
}