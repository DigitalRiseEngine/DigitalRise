using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DigitalRise;
using DigitalRise.Rendering;
using System;
using System.IO;
using System.Reflection;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Scenes;
using System.Linq;
using DigitalRise.Animation;

namespace SimpleScene
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.05f;

		private readonly GraphicsDeviceManager _graphics;
		private Scene _scene;
		private SceneNode _cameraMount;
		private CameraNode _mainCamera;
		private PrefabNode _model;
		private AnimationController _player;
		private readonly Renderer _renderer = new Renderer();
		//		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SpriteBatch _spriteBatch;
		private InputService _inputService;

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public ViewerGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800,
				GraphicsProfile = GraphicsProfile.HiDef
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = false;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// DigitalRise
			DR.Game = this;

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory, "Assets"));
			_scene = (Scene)assetManager.LoadSceneNode("Scenes/Main.scene");

			_model = _scene.GetSubtree().OfType<PrefabNode>().First();
			_player = new AnimationController((DrModelNode)_model.Prefab);
			_player.StartClip("idle");

			_cameraMount = _scene.GetSceneNode("_cameraMount");
			_mainCamera = _scene.GetSubtree().OfType<CameraNode>().First();

			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// DRDebugOptions.VisualizeBuffers = true;
		}

		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			var playerRotation = _model.RotationLocal;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			_model.RotationLocal = playerRotation;

			var cameraRotation = _cameraMount.RotationLocal;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.RotationLocal = cameraRotation;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			var movement = 0;
			if (_inputService.IsKeyDown(Keys.W))
			{
				movement = -1;
			}
			else if (_inputService.IsKeyDown(Keys.S))
			{
				movement = 1;
			}

			if (_inputService.IsKeyDown(Keys.LeftShift) || _inputService.IsKeyDown(Keys.RightShift))
			{
				movement *= 2;
			}

			// Set animation
			switch (movement)
			{
				case 0:
					if (_player.AnimationClip.Name != "idle")
					{
						_player.StartClip("idle");
					}
					break;

				case 1:
				case -1:
					if (_player.AnimationClip.Name != "walking")
					{
						_player.StartClip("walking");
					}
					break;

				case 2:
				case -2:
					if (_player.AnimationClip.Name != "running")
					{
						_player.StartClip("running");
					}
					break;
			}

			// Perform the movement
			var velocity = _model.PoseWorld.Orientation.Forward * movement * MovementSpeed;
			var pose = _model.PoseLocal;
			pose.Position += velocity;
			_model.PoseLocal = pose;

			//			_fpsCounter.Update(gameTime);
			_player.Update(gameTime.ElapsedGameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			var result = _renderer.Render(_scene, _mainCamera, gameTime);

			_spriteBatch.Begin();
			_spriteBatch.Draw(result, Vector2.Zero, Color.White);
			_spriteBatch.End();

			//			_fpsCounter.Draw(gameTime);
		}
	}
}
