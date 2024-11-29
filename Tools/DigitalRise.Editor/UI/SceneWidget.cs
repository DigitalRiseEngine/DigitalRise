﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
using DigitalRise.Utility;
using DigitalRise.Rendering;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Scenes;
using DigitalRise.Misc;
using DigitalRise.Rendering.Debugging;
using DigitalRise.Data.Lights;
using DirectionalLight = DigitalRise.Data.Lights.DirectionalLight;
using DigitalRise.Rendering.Billboards;
using DigitalRise.Data.Billboards;
using DigitalRise.Misc.TextureAtlas;

namespace DigitalRise.Editor.UI
{
	public class SceneWidget : Widget
	{
		private const int GridSize = 200;

		private static readonly Dictionary<Type, Texture2D> _typesIcons = new Dictionary<Type, Texture2D>
		{
			[typeof(LightNode)] = Editor.Resources.IconDirectionalLight,
			[typeof(CameraNode)] = Editor.Resources.IconCamera
		};

		private Scene _scene;
		private CameraInputController _controller;
		private MeshNode _gridMesh;
		// private Modelling.DigitalRiseModelMesh _waterMarker;
		// private DigitalRiseModel _modelMarker;
		private Vector3? _touchDownStart;
		private readonly Renderer _renderer = new Renderer();
		private readonly DebugRenderer _debugRenderer = new DebugRenderer();
		private readonly BillboardRenderer _billboardRenderer = new BillboardRenderer(2048);
		private readonly List<SceneNode> _gizmos = new List<SceneNode>();

		public Scene Scene
		{
			get => _scene;
			set
			{
				if (_scene == value) return;

				_scene = value;
				_controller = _scene == null ? null : new CameraInputController(_scene.Camera);
			}
		}

		public RenderStatistics RenderStatistics { get; private set; }
		public Instrument Instrument { get; } = new Instrument();

		private MeshNode GridMesh
		{
			get
			{
				return _gridMesh;
			}
		}


		private static bool IsMouseLeftButtonDown
		{
			get
			{
				var mouseState = Mouse.GetState();
				return (mouseState.LeftButton == ButtonState.Pressed);
			}
		}

		public SceneWidget()
		{
			ClipToBounds = true;
			AcceptsKeyboardFocus = true;
		}

		/*		private Vector3? CalculateMarkerPosition()
				{
					// Build viewport
					var bounds = ActualBounds;
					var p = ToGlobal(bounds.Location);
					bounds.X = p.X;
					bounds.Y = p.Y;
					var viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

					// Determine marker position
					var nearPoint = new Vector3(Desktop.MousePosition.X, Desktop.MousePosition.Y, 0);
					var farPoint = new Vector3(Desktop.MousePosition.X, Desktop.MousePosition.Y, 1);

					nearPoint = viewport.Unproject(nearPoint, Renderer.Projection, Renderer.View, Matrix.Identity);
					farPoint = viewport.Unproject(farPoint, Renderer.Projection, Renderer.View, Matrix.Identity);

					var direction = (farPoint - nearPoint);
					direction.Normalize();

					var ray = new Ray(nearPoint, direction);

					// Firstly determine whether we intersect zero height terrain rectangle
					var bb = MathUtils.CreateBoundingBox(0, Scene.Terrain.Size.X, 0, 0, 0, Scene.Terrain.Size.Y);
					var intersectDist = ray.Intersects(bb);
					if (intersectDist == null)
					{
						return null;
					}

					var markerPosition = nearPoint + direction * intersectDist.Value;

					// Now determine where we intersect terrain rectangle with real height
					var height = Scene.Terrain.GetHeight(markerPosition.X, markerPosition.Z);
					bb = MathUtils.CreateBoundingBox(0, Scene.Terrain.Size.X, height, height, 0, Scene.Terrain.Size.Y);
					intersectDist = ray.Intersects(bb);
					if (intersectDist == null)
					{
						return null;
					}

					markerPosition = nearPoint + direction * intersectDist.Value;

					return markerPosition;
				}

				private void UpdateMarker()
				{
					if (Scene == null || Scene.Terrain == null || Instrument.Type == InstrumentType.None)
					{
						return;
					}

					Scene.Marker.Position = CalculateMarkerPosition();
					Scene.Marker.Radius = Instrument.Radius;
				}*/


		public override void OnGotKeyboardFocus()
		{
			base.OnGotKeyboardFocus();

			_controller.IsEnabled = true;
		}

		public override void OnLostKeyboardFocus()
		{
			base.OnLostKeyboardFocus();

			_controller.IsEnabled = false;
		}

		private void RenderGizmos(Myra.Graphics2D.RenderContext myraContext, SceneNode node)
		{
			var lightNode = node as LightNode;
			if (lightNode != null)
			{
				var asDirectionalLight = lightNode.Light as DirectionalLight;
				if (asDirectionalLight != null)
				{
					var p = lightNode.PoseWorld.Position;

					var wd = lightNode.PoseWorld.WorldDirection;
					wd.Normalize();
					var p2 = p + wd * 10;

					_debugRenderer.DrawLine(p, p2, Color.Yellow, true);
				}
			}

			var modelNode = node as DrModelNode;
			if(modelNode != null)
			{
//				_debugRenderer.DrawSkeleton(modelNode, modelNode.PoseWorld, modelNode.ScaleWorld, 1, Color.White, false);
			}
		}

		private void PostRender(RenderContext drContext, Myra.Graphics2D.RenderContext myraContext, CameraNode camera)
		{
			_debugRenderer.Clear();
			Scene.RecursiveProcess(n => RenderGizmos(myraContext, n));

			// Selected object
			var selectedNode = StudioGame.MainForm.SelectedObject as SceneNode;
			var selectionShape = _3DUtils.GetPickBox(selectedNode);
			if (selectedNode != null && selectionShape != null && camera == Scene.Camera)
			{
				_debugRenderer.DrawShape(selectionShape, selectedNode.PoseWorld, selectedNode.ScaleWorld, Color.Orange, true, false);
				//_debugRenderer.DrawAxes(selectedNode.PoseWorld, 10, false);
			}

			// Axises
			_debugRenderer.Render(Scene.Camera);

			// Icons'
			_gizmos.Clear();
			Scene.RecursiveProcess(n =>
			{
				var asLightNode = n as LightNode;
				if (asLightNode != null && asLightNode.Light is AmbientLight)
				{
					return;
				}

				foreach (var pair in _typesIcons)
				{
					if (pair.Key.IsAssignableFrom(n.GetType()))
					{
						BillboardNode node;
						if (n.UserData == null)
						{
							var imageBillboard = new ImageBillboard();
							imageBillboard.Texture = new PackedTexture(pair.Value);

							node = new BillboardNode(imageBillboard);

							n.UserData = node;
						}
						else
						{
							node = (BillboardNode)n.UserData;
						}

						var pose = node.PoseLocal;
						pose.Position = n.PoseWorld.Position;
						node.PoseLocal = pose;

						_gizmos.Add(node);

						break;
					}
				}
			});

			_billboardRenderer.Render(drContext, _gizmos, RenderOrder.BackToFront);
		}

		public override void InternalRender(Myra.Graphics2D.RenderContext context)
		{
			base.InternalRender(context);

			if (Scene == null)
			{
				return;
			}

			_controller.Update();

			var bounds = ActualBounds;

			// Save scissor as it would be destroyed on exception
			var device = DR.GraphicsDevice;
			var oldViewport = device.Viewport;

			var p = ToGlobal(bounds.Location);
			bounds.X = p.X;
			bounds.Y = p.Y;

			// Save scissor as it would be destroyed on exception
			var scissor = device.ScissorRectangle;

			try
			{
				device.Viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

				var camera = StudioGame.MainForm.CurrentCamera;

				/*				if (DigitalRiseEditorOptions.ShowGrid)
								{
									_renderer.AddNode(GridMesh);
								}*/

				// Render scene
				var result = _renderer.Render(Scene,
					StudioGame.Instance.LastRenderGameTime,
					(ctx) => PostRender(ctx, context, camera));

				RenderStatistics = _renderer.Statistics;
				device.Viewport = oldViewport;

				context.Draw(result, ActualBounds, Color.White);
				context.Flush();

				/*				var m = Editor.Resources.ModelAxises;
								var c = Scene.Camera.Clone();

								// Make the gizmo placed always in front of the camera
								c.Translation = Vector3.Zero;
								m.Translation = c.Direction * 2;

								_renderer.AddNode(m);
								result = _renderer.Render(c, new Point(160, 160));
								context.Draw(result,
									new Rectangle(bounds.Right - 160, bounds.Y, 160, 160),
									Color.White);

								UpdateMarker();
								_renderer.Begin();
								_renderer.DrawScene(Scene);

								if (_waterMarker != null)
								{
									_renderer.DrawMesh(_waterMarker, Scene.Camera);
								}

								if (_modelMarker != null)
								{
									_renderer.DrawModel(_modelMarker, Scene.Camera);
								}

								_renderer.End();*/
			}
			catch (Exception ex)
			{
				device.Viewport = oldViewport;
				DR.GraphicsDevice.ScissorRectangle = scissor;
				var font = Editor.Resources.ErrorFont;
				var message = ex.ToString();
				var sz = font.MeasureString(message);

				bounds = ActualBounds;
				var pos = new Vector2(bounds.X + (bounds.Width - sz.X) / 2,
					bounds.Y + (bounds.Height - sz.Y) / 2);

				pos.X = (int)pos.X;
				pos.Y = (int)pos.Y;
				context.DrawString(font, message, pos, Color.Red);
			}
			finally
			{
				device.Viewport = oldViewport;
			}
		}

		protected override void OnPlacedChanged()
		{
			base.OnPlacedChanged();

			if (Desktop == null)
			{
				return;
			}

			Desktop.TouchUp += Desktop_TouchUp;
		}

		private void Desktop_TouchUp(object sender, EventArgs e)
		{
			/*			if (Instrument.Type == InstrumentType.Water && _touchDownStart != null && Scene.Marker.Position != null)
						{
							GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ);

							if (sizeX > 0 && sizeZ > 0)
							{
								var waterTile = new WaterTile(startPos.X, startPos.Z, Scene.DefaultWaterLevel, sizeX, sizeZ);
								Scene.WaterTiles.Add(waterTile);
							}

							_touchDownStart = null;
							_waterMarker = null;
						}*/
		}

		/*		private void UpdateTerrainHeight(Point pos, float power)
				{
					var height = Scene.Terrain.GetHeightByHeightPos(pos);
					height += power;
					Scene.Terrain.SetHeightByHeightPos(pos, height);
				}

				private void UpdateTerrainSplatMap(Point splatPos, SplatManChannel channel, float power)
				{
					var splatValue = Scene.Terrain.GetSplatValue(splatPos, channel);
					splatValue += power * 0.5f;
					Scene.Terrain.SetSplatValue(splatPos, channel, splatValue);
				}

				private void ApplyLowerRaise()
				{
					var power = Instrument.Power;
					var radius = Scene.Marker.Radius;
					var markerPos = Scene.Marker.Position.Value;

					var topLeft = Scene.Terrain.ToHeightPosition(markerPos.X - radius, markerPos.Z - radius);
					var bottomRight = Scene.Terrain.ToHeightPosition(markerPos.X + radius, markerPos.Z + radius);

					for (var x = topLeft.X; x <= bottomRight.X; ++x)
					{
						for (var y = topLeft.Y; y <= bottomRight.Y; ++y)
						{
							var heightPos = new Point(x, y);
							var terrainPos = Scene.Terrain.HeightToTerrainPosition(heightPos);
							var dist = Vector2.Distance(new Vector2(markerPos.X, markerPos.Z), terrainPos);

							if (dist > radius)
							{
								continue;
							}

							switch (Instrument.Type)
							{
								case InstrumentType.None:
									break;
								case InstrumentType.RaiseTerrain:
									UpdateTerrainHeight(heightPos, power);
									break;
								case InstrumentType.LowerTerrain:
									UpdateTerrainHeight(heightPos, -power);
									break;
							}
						}
					}
				}

				private void ApplyTerrainPaint()
				{
					var power = Instrument.Power;
					var radius = Scene.Marker.Radius;
					var markerPos = Scene.Marker.Position.Value;

					var topLeft = Scene.Terrain.ToSplatPosition(markerPos.X - radius, markerPos.Z - radius);
					var bottomRight = Scene.Terrain.ToSplatPosition(markerPos.X + radius, markerPos.Z + radius);

					for (var x = topLeft.X; x <= bottomRight.X; ++x)
					{
						for (var y = topLeft.Y; y <= bottomRight.Y; ++y)
						{
							var splatPos = new Point(x, y);
							var terrainPos = Scene.Terrain.SplatToTerrainPosition(splatPos);
							var dist = Vector2.Distance(new Vector2(markerPos.X, markerPos.Z), terrainPos);

							if (dist > radius)
							{
								continue;
							}

							switch (Instrument.Type)
							{
								case InstrumentType.PaintTexture1:
									UpdateTerrainSplatMap(splatPos, SplatManChannel.First, power);
									break;
								case InstrumentType.PaintTexture2:
									UpdateTerrainSplatMap(splatPos, SplatManChannel.Second, power);
									break;
								case InstrumentType.PaintTexture3:
									UpdateTerrainSplatMap(splatPos, SplatManChannel.Third, power);
									break;
								case InstrumentType.PaintTexture4:
									UpdateTerrainSplatMap(splatPos, SplatManChannel.Fourth, power);
									break;
							}
						}
					}
				}

				private void ApplyPaintInstrument()
				{
					if (Instrument.Type == InstrumentType.RaiseTerrain || Instrument.Type == InstrumentType.LowerTerrain)
					{
						ApplyLowerRaise();
					}
					else
					{
						ApplyTerrainPaint();
					}
				}*/

		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			var mouseState = Mouse.GetState();

			/*if (Instrument.Type == InstrumentType.Model)
			{
				if (Scene.Marker.Position != null)
				{
					if (_modelMarker == null || _modelMarker != Instrument.Model)
					{
						_modelMarker = Instrument.Model;
					}

					var pos = Scene.Marker.Position.Value;
					pos.Y = -_modelMarker.BoundingBox.Min.Y;
					pos.Y += Scene.Terrain.GetHeight(pos.X, pos.Z);

					_modelMarker.Transform = Matrix.CreateTranslation(pos);
				} else
				{
					_modelMarker = null;
				}
			}*/
		}

		public override void OnMouseLeft()
		{
			base.OnMouseLeft();

			// _modelMarker = null;
		}

		/*		public override void OnTouchDown()
				{
					base.OnTouchDown();

					if (!IsMouseLeftButtonDown || Scene.Marker.Position == null)
					{
						return;
					}

					if (Instrument.IsPaintInstrument)
					{
						ApplyPaintInstrument();
					}
					else if (Instrument.Type == InstrumentType.Water)
					{
						_touchDownStart = Scene.Marker.Position.Value;
					}
					else if (Instrument.Type == InstrumentType.Model)
					{
						var pos = Scene.Marker.Position.Value;

						var model = Instrument.Model;
						pos.Y = -model.BoundingBox.Min.Y;
						pos.Y += Scene.Terrain.GetHeight(pos.X, pos.Z);

						model.Transform = Matrix.CreateTranslation(pos);

						Scene.Models.Add(model);
					}
				}

				private void GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ)
				{
					var markerPos = Scene.Marker.Position.Value;

					startPos = new Vector3(Math.Min(markerPos.X, _touchDownStart.Value.X),
						Scene.DefaultWaterLevel,
						Math.Min(markerPos.Z, _touchDownStart.Value.Z));

					sizeX = Math.Abs(markerPos.X - _touchDownStart.Value.X);
					sizeZ = Math.Abs(markerPos.Z - _touchDownStart.Value.Z);
				}*/

		public override void OnTouchMoved()
		{
			base.OnTouchMoved();

			/*if (!IsMouseLeftButtonDown || Scene.Marker.Position == null)
			{
				return;
			}

			if (Instrument.IsPaintInstrument)
			{
				ApplyPaintInstrument();
			}
			else if (Instrument.Type == InstrumentType.Water)
			{
				if (_touchDownStart != null)
				{
					GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ);
					if (sizeX > 0 && sizeZ > 0)
					{
						if (_waterMarker == null)
						{
							_waterMarker = new Mesh(PrimitiveMeshes.SquarePositionFromZeroToOne, Material.CreateSolidMaterial(Color.Green));
						}

						_waterMarker.Transform = Matrix.CreateScale(sizeX, 0, sizeZ) * Matrix.CreateTranslation(startPos);
					}
				}
			}*/
		}
	}
}