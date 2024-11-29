using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using DigitalRise.Editor.UI;
using System.Diagnostics;
using System;
using DigitalRise.SceneGraph;

namespace DigitalRise.Editor
{
	public class StudioGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Desktop _desktop = null;
		private MainForm _mainForm;
		private int _numberOfUpdates;
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private readonly State _state;


		public static StudioGame Instance { get; private set; }
		public GameTime LastRenderGameTime { get; private set; }
		public float FPS { get; private set; }
		public static MainForm MainForm => Instance._mainForm;

		public StudioGame()
		{
			Instance = this;

			// Restore state
			_state = State.Load();
			if (_state != null)
			{
				DigitalRiseEditorOptions.ShowGrid = _state.ShowGrid;
				/*				DebugSettings.DrawBoundingBoxes = _state.DrawBoundingBoxes;
								DebugSettings.DrawLightViewFrustrum = _state.DrawLightViewFrustum;*/
				DRDebugOptions.VisualizeBuffers = _state.DrawRenderBuffers;
			}

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}

			if (_state != null)
			{
				_graphics.PreferredBackBufferWidth = _state.Size.X;
				_graphics.PreferredBackBufferHeight = _state.Size.Y;
			}
			else
			{
				_graphics.PreferredBackBufferWidth = 1280;
				_graphics.PreferredBackBufferHeight = 800;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

			// UI
			MyraEnvironment.Game = this;
			DR.Game = this;

			/*			Nrs.ExternalEffects = new FolderWatcher(@"D:\Projects\DigitalRise\DigitalRise\Effects")
						{
							BinaryFolder = @"D:\Projects\DigitalRise\DigitalRise\Effects\FNA\bin"
						};*/

			_mainForm = new MainForm();

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			if (_state != null)
			{
				_mainForm._topSplitPane.SetSplitterPosition(0, _state != null ? _state.TopSplitterPosition : 0.75f);
				_mainForm._leftSplitPane.SetSplitterPosition(0, _state != null ? _state.LeftSplitterPosition : 0.5f);

				if (!string.IsNullOrEmpty(_state.Folder))
				{
					_mainForm.LoadFolder(_state.Folder);
				}
			}

			/*			DebugSettings.DrawCamerasFrustums = true;
						DebugSettings.DrawLights = true;*/

			NodesRegistry.AddAssembly(typeof(SceneNode).Assembly);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_numberOfUpdates++;
			if (_stopwatch.Elapsed.TotalSeconds > 0.5)
			{
				FPS = (float)Math.Round(_numberOfUpdates / _stopwatch.Elapsed.TotalSeconds);

				_numberOfUpdates = 0;
				_stopwatch.Reset();
				_stopwatch.Start();
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			LastRenderGameTime = gameTime;

			GraphicsDevice.Clear(Color.Black);

			_desktop.Render();
		}

		protected override void EndRun()
		{
			base.EndRun();

			var state = new State
			{
				Size = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
					GraphicsDevice.PresentationParameters.BackBufferHeight),
				TopSplitterPosition = _mainForm._topSplitPane.GetSplitterPosition(0),
				LeftSplitterPosition = _mainForm._leftSplitPane.GetSplitterPosition(0),
				Folder = _mainForm.Folder,
				ShowGrid = DigitalRiseEditorOptions.ShowGrid,
				/*				DrawBoundingBoxes = DebugSettings.DrawBoundingBoxes,
								DrawLightViewFrustum = DebugSettings.DrawLightViewFrustrum,*/
				DrawRenderBuffers = DRDebugOptions.VisualizeBuffers,
			};

			state.Save();
		}
	}
}