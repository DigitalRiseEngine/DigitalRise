using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

namespace DigitalRise
{
	public static class DR
	{
		private static Game _game;

		public static IEffectsSource EffectsSource = new StaticEffectsSource();

		public static Game Game
		{
			get
			{
				return _game;
			}

			set
			{
				if (_game == value)
				{
					return;
				}

				if (_game != null)
				{
					_game.Disposed -= GameOnDisposed;
				}

				_game = value;

				if (_game != null)
				{
#if MONOGAME
					if (_game.GraphicsDevice.GraphicsProfile != GraphicsProfile.HiDef)
					{
						throw new Exception("DigitalRise for MonoGame requires GraphicsProfile.HiDef.");
					}
#endif

					_game.Disposed += GameOnDisposed;
				}
			}
		}

		public static GraphicsDevice GraphicsDevice
		{
			get
			{
				return Game.GraphicsDevice;
			}
		}

		public static float TotalGameTimeInSeconds { get; set; }

		public static bool DepthBufferEnabled { get; set; } = true;

		public static Action<string> LogInfo = Console.WriteLine;
		public static Action<string> LogWarning = Console.WriteLine;
		public static Action<string> LogError = Console.WriteLine;

		public static string Version
		{
			get
			{
				var assembly = typeof(DR).GetTypeInfo().Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		private static void GameOnDisposed(object sender, EventArgs eventArgs)
		{
		}
	}
}