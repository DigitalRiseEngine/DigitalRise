using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetManagementBase;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Myra.Graphics2D.UI.Properties;
using DigitalRise.Editor.Utility;
using DigitalRise.SceneGraph;
using DigitalRise.SceneGraph.Scenes;
using DigitalRise.Data.Materials;
using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Editor.UI
{
	public partial class MainForm
	{
		private const string ButtonsPanelId = "_buttonsPanel";
		private const string ButtonCameraViewId = "_buttonCameraView";

		private string _folder;
		private bool _explorerTouchDown = false;
		private readonly TreeView _treeFileExplorer, _treeFileSolution;

		public string Folder
		{
			get => _folder;

			set
			{
				if (value == _folder)
				{
					return;
				}

				_folder = value;
				UpdateTitle();
			}
		}

		public object SelectedObject
		{
			get => _propertyGrid.Object;
			set => _propertyGrid.Object = value;
		}

		public SceneWidget CurrentSceneWidget
		{
			get
			{
				if (_tabControlScenes.SelectedIndex == null)
				{
					return null;
				}

				return _tabControlScenes.SelectedItem.Content.FindChild<SceneWidget>();
			}
		}

		public Scene CurrentScene => CurrentSceneWidget.Scene;
		private readonly List<InstrumentButton> _allButtons = new List<InstrumentButton>();

		public CameraNode CurrentCamera
		{
			get
			{
				var camera = CurrentScene.Camera;

				var asCamera = SelectedObject as CameraNode;
				if (asCamera != null)
				{
					var buttonCameraView = _tabControlScenes.SelectedItem.Content.FindChildById<ToggleButton>(ButtonCameraViewId);
					if (buttonCameraView != null && buttonCameraView.IsToggled)
					{
						camera = asCamera;
					}
				}

				return camera;
			}
		}

		public event EventHandler SelectedObjectChanged
		{
			add
			{
				_treeFileExplorer.SelectionChanged += value;
			}

			remove
			{
				_treeFileExplorer.SelectionChanged -= value;
			}
		}


		public MainForm()
		{
			BuildUI();

			_treeFileSolution = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};
			_panelSolution.Content = _treeFileSolution;

			_treeFileExplorer = new TreeView
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				ClipToBounds = true
			};
			_panelSceneExplorer.Content = _treeFileExplorer;

			/*			_propertyGrid.Settings.ImagePropertyValueGetter = name =>
						{
							switch (name)
							{
								case "TextureBase":
									return Scene.Terrain.TextureBaseName;
								case "TexturePaint1":
									return Scene.Terrain.TexturePaintName1;
								case "TexturePaint2":
									return Scene.Terrain.TexturePaintName2;
								case "TexturePaint3":
									return Scene.Terrain.TexturePaintName3;
								case "TexturePaint4":
									return Scene.Terrain.TexturePaintName4;
							}

							throw new Exception($"Unknown property {name}");
						};

						_propertyGrid.Settings.ImagePropertyValueSetter = (name, value) =>
						{
							switch (name)
							{
								case "TextureBase":
									Scene.Terrain.TextureBaseName = value;
									break;
								case "TexturePaint1":
									Scene.Terrain.TexturePaintName1 = value;
									RefreshLibrary();
									break;
								case "TexturePaint2":
									Scene.Terrain.TexturePaintName2 = value;
									RefreshLibrary();
									break;
								case "TexturePaint3":
									Scene.Terrain.TexturePaintName3 = value;
									RefreshLibrary();
									break;
								case "TexturePaint4":
									Scene.Terrain.TexturePaintName4 = value;
									RefreshLibrary();
									break;
								default:
									throw new Exception($"Unknown property {name}");
							}
						};*/

			_topSplitPane.SetSplitterPosition(0, 0.2f);
			_topSplitPane.SetSplitterPosition(1, 0.6f);

			_menuItemNew.Selected += (s, a) => NewScene();

			_menuItemOpenSolution.Selected += (s, a) =>
			{
				FileDialog dialog = new FileDialog(FileDialogMode.ChooseFolder);

				if (!string.IsNullOrEmpty(_folder))
				{
					dialog.Folder = Path.GetDirectoryName(_folder);
				}

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						// "Cancel" or Escape
						return;
					}

					// "Ok" or Enter
					LoadFolder(dialog.FilePath);
				};

				dialog.ShowModal(Desktop);
			};

			_menuItemSaveCurrentItem.Selected += (s, a) => SaveCurrentItem();

			_treeFileExplorer.TouchDown += (s, e) =>
			{
				var mouseState = Mouse.GetState();
				if (mouseState.RightButton == ButtonState.Pressed)
				{
					_explorerTouchDown = true;
				}
			};
			_treeFileExplorer.TouchUp += _treeFileExplorer_TouchUp;

			_treeFileExplorer.SelectionChanged += (s, a) =>
			{
				_propertyGrid.Object = _treeFileExplorer.SelectedNode?.Tag;
			};

			_treeFileSolution.TouchDoubleClick += (s, a) => OpenCurrentSolutionItem();

			_propertyGrid.ObjectChanged += (s, a) =>
			{
				var tab = _tabControlScenes.SelectedItem;
				if (tab == null)
				{
					return;
				}

				var buttonsGrid = _tabControlScenes.SelectedItem.Content.FindChildById<StackPanel>(ButtonsPanelId);
				var buttonShowCamera = tab.Content.FindChildById<ToggleButton>(ButtonCameraViewId);
				var asCamera = _propertyGrid.Object as CameraNode;

				if (asCamera == null && buttonShowCamera != null)
				{
					// Remove button from the grid
					buttonsGrid.Widgets.Remove(buttonShowCamera);
				}
				else if (asCamera != null && buttonShowCamera == null)
				{
					// Add button to the grid
					var label = new Label
					{
						Text = "Camera View"
					};

					buttonShowCamera = new ToggleButton
					{
						Id = ButtonCameraViewId,
						Content = label,
					};

					buttonsGrid.Widgets.Add(buttonShowCamera);
				}
			};

			_propertyGrid.PropertyChanged += (s, a) =>
			{
				InvalidateCurrentItem();

				switch (a.Data)
				{
					case "Id":
						UpdateTreeNodeId(_treeFileExplorer.SelectedNode);
						break;

					case "PrimitiveMeshType":
						_propertyGrid.Rebuild();
						break;
				}
			};

			_propertyGrid.CustomWidgetProvider = CreateCustomEditor;

			_tabControlScenes.Items.Clear();
			_tabControlScenes.SelectedIndexChanged += (s, a) => RefreshExplorer(null);
			_tabControlScenes.ItemsCollectionChanged += (s, a) => UpdateStackPanelEditor();

			_buttonGrid.IsToggled = DigitalRiseEditorOptions.ShowGrid;
			/*			_buttonBoundingBoxes.IsToggled = DebugSettings.DrawBoundingBoxes;
						_buttonLightViewFrustum.IsToggled = DebugSettings.DrawLightViewFrustrum;*/
			_buttonVisualizeBuffers.IsToggled = DRDebugOptions.VisualizeBuffers;

			_buttonGrid.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonBoundingBoxes.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonLightViewFrustum.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonVisualizeBuffers.IsToggledChanged += (s, a) => UpdateDebugOptions();

			UpdateStackPanelEditor();
		}

		private Widget InternalCreateCustomEditor(Record record, object obj, string[] extensions, Func<AssetManager, string, object> loader)
		{
			var propertyType = record.Type;

			var result = new HorizontalStackPanel
			{
				Spacing = 8
			};

			var pathProperty = obj.GetType().GetProperty(record.Name + "Path");
			var texturePath = (string)pathProperty.GetValue(obj);

			var path = new TextBox
			{
				Readonly = true,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Text = texturePath
			};

			StackPanel.SetProportionType(path, ProportionType.Fill);
			result.Widgets.Add(path);

			var button = new Button
			{
				Tag = obj,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Content = new Label
				{
					Text = "Change...",
					HorizontalAlignment = HorizontalAlignment.Center,
				}
			};
			Grid.SetColumn(button, 1);

			button.Click += (sender, args) =>
			{
				try
				{
					var dialog = new ChooseAssetDialog(Folder, extensions);

					dialog.Closed += (s, a) =>
					{
						if (!dialog.Result)
						{
							// "Cancel" or Escape
							return;
						}

						// "Ok" or Enter
						try
						{
							var path = dialog.FilePath;
							var assetManager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(path));


							var value = loader(assetManager, path);

							record.SetValue(obj, value);
							pathProperty.SetValue(obj, path);
						}
						catch (Exception ex)
						{
							var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
							dialog.ShowModal(Desktop);
						}
					};

					dialog.ShowModal(Desktop);
				}
				catch (Exception ex)
				{
					var dialog = Dialog.CreateMessageBox("Error", ex.Message);
					dialog.ShowModal(Desktop);
				}
			};

			result.Widgets.Add(button);

			return result;
		}

		private Widget CreateCustomEditor(Record record, object obj)
		{
			if (obj is DefaultMaterial && record.Name.EndsWith("Texture"))
			{
				return InternalCreateCustomEditor(record, obj,
					new[] { "dds", "png", "jpg", "gif", "bmp", "tga" },
					(assetManager, path) => assetManager.LoadTexture2D(DR.GraphicsDevice, path));
			}

			if (record.Type == typeof(TextureCube))
			{
				return InternalCreateCustomEditor(record, obj, new[] { "dds" },
					(assetManager, path) => assetManager.LoadTextureCube(DR.GraphicsDevice, path));

			}

			return null;
		}

		private bool SetTabByName(TabControl tabControl, string filePath)
		{
			for (var i = 0; i < tabControl.Items.Count; ++i)
			{
				var tabItem = tabControl.Items[i];
				var tabInfo = (TabInfo)tabItem.Tag;
				if (tabInfo.FilePath == filePath)
				{
					tabControl.SelectedIndex = i;
					return true;
				}
			}

			return false;
		}

		private void UpdateStackPanelEditor()
		{
			_panelScenes.Visible = _tabControlScenes.Items.Count > 0;
		}

		private void OpenTab(Scene scene, string file)
		{
			var sceneWidget = new SceneWidget
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				Scene = scene,
			};

			var panel = new Panel();
			var buttonsPanel = new HorizontalStackPanel
			{
				Id = ButtonsPanelId,
				Left = 2,
				Top = 2,
			};

			panel.Widgets.Add(sceneWidget);
			panel.Widgets.Add(buttonsPanel);

			var tabInfo = new TabInfo(file);

			var tabItem = new TabItem
			{
				Text = tabInfo.Title,
				Content = panel,
				Tag = tabInfo
			};

			tabInfo.TitleChanged += (s, a) => tabItem.Text = tabInfo.Title;

			_tabControlScenes.Items.Add(tabItem);
			_tabControlScenes.SelectedIndex = _tabControlScenes.Items.Count - 1;
		}

		private void OpenCurrentSolutionItem()
		{
			try
			{
				var node = _treeFileSolution.SelectedNode;
				if (node == null || node.Tag == null || !(node.Tag is string))
				{
					return;
				}

				var file = (string)node.Tag;
				if (file.EndsWith(".scene"))
				{
					if (SetTabByName(_tabControlScenes, file))
					{
						return;
					}

					// Load scene
					Scene scene;
					var folder = Path.GetDirectoryName(file);
					var assetManager = AssetManager.CreateFileAssetManager(folder);

					scene = assetManager.LoadScene(file);
					OpenTab(scene, file);
				}
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
				return;
			}
		}

		private void AddNewNode(SceneNode parent, SceneNode child)
		{
			parent.Children.Add(child);
			InvalidateCurrentItem();
			RefreshExplorer(child);
		}

		private void OnAddGenericNode(SceneNode parent, string category, List<NodeTypeInfo> types)
		{
			var dialog = new AddNewItemDialog
			{
				Title = $"Add New {category}"
			};

			var orderedTypes = (from t in types orderby t.Type.Name select t).ToArray();
			foreach (var ot in orderedTypes)
			{
				if (ot.SubType == null)
				{
					dialog.AddItem(ot.Type.Name);
				}
				else
				{
					dialog.AddItem(ot.SubType.Name);
				}
			}

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				var ot = orderedTypes[dialog.SelectedIndex.Value];

				SceneNode newNode;
				if (ot.SubType == null)
				{
					newNode = (SceneNode)Activator.CreateInstance(ot.Type);
				}
				else
				{
					var par = Activator.CreateInstance(ot.SubType);
					newNode = (SceneNode)Activator.CreateInstance(ot.Type, par);
				}

				newNode.Name = dialog.ItemName;

				/*				var asMeshNodeBase = newNode as MeshNodeBase;
								if (asMeshNodeBase != null)
								{
									asMeshNodeBase.Material = new DefaultMaterial();
								}*/

				AddNewNode(parent, newNode);
			};

			dialog.ShowModal(Desktop);
		}

		private void OnAddModel(SceneNode parent)
		{
			try
			{
				var dialog = new ChooseAssetDialog(Folder, new[] { "glb", "gltf" });

				dialog.Closed += (s, a) =>
				{
					if (!dialog.Result)
					{
						// "Cancel" or Escape
						return;
					}

					// "Ok" or Enter
					try
					{
						var path = dialog.FilePath;

						var node = new DrModelNode
						{
							Name = dialog.SelectedId,
							ModelPath = dialog.FilePath
						};

						var assetManager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(path));
						node.Load(assetManager);

						AddNewNode(parent, node);
					}
					catch (Exception ex)
					{
						var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
						dialog.ShowModal(Desktop);
					}
				};

				dialog.ShowModal(Desktop);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.Message);
				dialog.ShowModal(Desktop);
			}
		}

		private void _treeFileExplorer_TouchUp(object sender, EventArgs e)
		{
			if (!_explorerTouchDown)
			{
				return;
			}

			_explorerTouchDown = false;

			var treeNode = _treeFileExplorer.SelectedNode;
			if (treeNode == null || treeNode.Tag == null)
			{
				return;
			}

			var sceneNode = (SceneNode)treeNode.Tag;

			var contextMenuOptions = new List<Tuple<string, Action>>();
			foreach (var pair in NodesRegistry.NodesByCategories)
			{
				Action action = null;

				if (pair.Value.Count == 1 && pair.Value[0].Type == typeof(DrModelNode))
				{
					// Special case
					action = () => OnAddModel(sceneNode);
				}
				else
				{
					// Ordinary case
					action = () => OnAddGenericNode(sceneNode, pair.Key, pair.Value);
				}

				contextMenuOptions.Add(new Tuple<string, Action>($"Insert {pair.Key}...", action));
			}

			if (treeNode != _treeFileExplorer.GetSubNode(0))
			{
				// Not root, add delete
				contextMenuOptions.Add(new Tuple<string, Action>("Delete Current Node", () =>
				{
					sceneNode.RemoveFromParent();
					InvalidateCurrentItem();
					RefreshExplorer(null);
				}));
			}

			Desktop.BuildContextMenu(contextMenuOptions);
		}

		private void NewScene()
		{
			var dialog = new ChooseNodeDialog();

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				try
				{
				}
				catch (Exception ex)
				{
					var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
					dialog.ShowModal(Desktop);
				}
			};

			dialog.ShowModal(Desktop);

/*			var scene = new Scene
			{
				Name = "Root"
			};

			var cameraNode = new CameraNode(new PerspectiveViewVolume());
			scene.Camera = cameraNode;

			OpenTab(scene, string.Empty);*/
		}

		private void UpdateTitle()
		{
			var title = string.IsNullOrEmpty(_folder) ? "DigitalRise.Editor" : _folder;
			StudioGame.Instance.Window.Title = title;
		}

		private static TreeViewNode ProcessNode(ITreeViewNode root, string folder)
		{
			var projectNode = root.AddSubNode(new Label
			{
				Text = Path.GetFileName(folder)
			});

			projectNode.IsExpanded = true;
			projectNode.Tag = folder;

			// Add folders
			var subfolders = Directory.GetDirectories(folder);
			foreach (var subFolder in subfolders)
			{
				// Ignore subfolders without scenes
				if (Directory.GetFiles(subFolder, "*.scene", SearchOption.AllDirectories).Length == 0)
				{
					continue;
				}

				ProcessNode(projectNode, subFolder);
			}

			// Add scene files
			var sceneFiles = Directory.GetFiles(folder, "*.scene");
			foreach (var file in sceneFiles)
			{
				var node = projectNode.AddSubNode(new Label
				{
					Text = Path.GetFileName(file),
				});

				node.Tag = file;
			}

			return projectNode;
		}

		public void LoadFolder(string path)
		{
			try
			{
				if (!string.IsNullOrEmpty(path))
				{
					// DR.EffectsSource = new DynamicEffectsSource(Path.GetDirectoryName(path));

					_treeFileSolution.RemoveAllSubNodes();
					ProcessNode(_treeFileSolution, path);
				}

				_folder = path;
				UpdateTitle();
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		private void ProcessSave(Scene scene, string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				return;
			}

			try
			{
				scene.SaveToFile(filePath);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
			}
		}

		/*		private void Save(bool setFileName)
				{
					if (string.IsNullOrEmpty(FilePath) || setFileName)
					{
						var dlg = new FileDialog(FileDialogMode.SaveFile)
						{
							Filter = "*.scene"
						};

						if (!string.IsNullOrEmpty(FilePath))
						{
							dlg.FilePath = FilePath;
						}

						dlg.ShowModal(Desktop);

						dlg.Closed += (s, a) =>
						{
							if (dlg.Result)
							{
								ProcessSave(dlg.FilePath);
							}
						};
					}
					else
					{
						ProcessSave(FilePath);
					}
				}*/

		private void UpdateTreeNodeId(TreeViewNode node)
		{
			var sceneNode = (SceneNode)node.Tag;
			var label = (Label)node.Content;
			label.Text = $"{sceneNode.GetType().Name} (#{sceneNode.Name})";
		}

		private TreeViewNode RecursiveAddToExplorer(ITreeViewNode treeViewNode, SceneNode sceneNode)
		{
			var label = new Label();
			var newNode = treeViewNode.AddSubNode(label);
			newNode.IsExpanded = true;
			newNode.Tag = sceneNode;

			UpdateTreeNodeId(newNode);

			if (sceneNode.Children != null)
			{
				foreach (var child in sceneNode.Children)
				{
					RecursiveAddToExplorer(newNode, child);
				}
			}

			return newNode;
		}

		private void RefreshExplorer(SceneNode selectedNode)
		{
			_treeFileExplorer.RemoveAllSubNodes();
			if (_tabControlScenes.SelectedIndex == null ||
				_tabControlScenes.SelectedIndex < 0 ||
				_tabControlScenes.SelectedIndex >= _tabControlScenes.Items.Count)
			{
				return;
			}

			var sceneWidget = _tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Content.FindChild<SceneWidget>();
			RecursiveAddToExplorer(_treeFileExplorer, sceneWidget.Scene);
			_treeFileExplorer.SelectedNode = _treeFileExplorer.FindNode(n => n.Tag == selectedNode);
		}

		public void RefreshLibrary()
		{
		}

		private void InvalidateCurrentItem()
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			var tabInfo = (TabInfo)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Tag;
			tabInfo.Dirty = true;
		}

		private void SaveCurrentItem()
		{
			if (_tabControlScenes.SelectedIndex == null)
			{
				return;
			}

			var tab = _tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value];
			var sceneWidget = tab.Content.FindChild<SceneWidget>();

			var tabInfo = (TabInfo)_tabControlScenes.Items[_tabControlScenes.SelectedIndex.Value].Tag;

			if (!string.IsNullOrEmpty(tabInfo.FilePath))
			{
				sceneWidget.Scene.SaveToFile(tabInfo.FilePath);
				tabInfo.Dirty = false;
			}
			else
			{
				var dlg = new FileDialog(FileDialogMode.SaveFile)
				{
					Filter = "*.scene"
				};

				dlg.ShowModal(Desktop);

				dlg.Closed += (s, a) =>
				{
					if (dlg.Result)
					{
						tabInfo.FilePath = dlg.FilePath;
						sceneWidget.Scene.SaveToFile(tabInfo.FilePath);
						tabInfo.Dirty = false;
					}
				};
			}
		}

		private void UpdateDebugOptions()
		{
			DigitalRiseEditorOptions.ShowGrid = _buttonGrid.IsToggled;
			/*			DebugSettings.DrawBoundingBoxes = _buttonBoundingBoxes.IsToggled;
						DebugSettings.DrawLightViewFrustrum = _buttonLightViewFrustum.IsToggled;*/
			DRDebugOptions.VisualizeBuffers = _buttonVisualizeBuffers.IsToggled;
		}

		public override void InternalRender(Myra.Graphics2D.RenderContext context)
		{
			base.InternalRender(context);

			if (_panelStatistics.Visible == false || CurrentSceneWidget == null)
			{
				return;
			}

			var stats = CurrentSceneWidget.RenderStatistics;

			_labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_labelDrawCalls.Text = stats.DrawCalls.ToString();
			_labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
			_labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
		}
	}
}