using AssetManagementBase;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace DigitalRise
{
	internal static partial class StockEffects
	{
#if FNA
		private const string EffectsResourcePath = "Effects.FNA.bin";
#elif MONOGAME
		private const string EffectsResourcePath = "Effects.MonoGameDX11.bin";
#endif
		private static AssetManager _effects;

		static StockEffects()
		{
			var assembly = typeof(StockEffects).Assembly;
			_effects = AssetManager.CreateResourceAssetManager(assembly, EffectsResourcePath);
		}

		public static Effect LoadEffect(string path)
		{
			path = Path.ChangeExtension(path, "efb");

			return _effects.LoadEffect(DR.GraphicsDevice, path);
		}
	}
}
