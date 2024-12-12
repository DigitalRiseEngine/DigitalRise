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
using Microsoft.Xna.Framework.Graphics;
using DigitalRise.Data.Modelling;
using DigitalRise.Utility;
using Myra.Events;
using info.lundin.math;

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

				try
				{
					if (!string.IsNullOrEmpty(value))
					{
						// DR.EffectsSource = new DynamicEffectsSource(Path.GetDirectoryName(path));
						RefreshSolution();
						AssetManager = AssetManager.CreateFileAssetManager(value);
					}
					else
					{
						AssetManager = null;
					}


					UpdateTitle();
				}
				catch (Exception ex)
				{
					var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
					dialog.ShowModal(Desktop);
				}
			}
		}

		private AssetManager AssetManager { get; set; }

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


		private readonly List<InstrumentButton> _allButtons = new List<InstrumentButton>();

		public CameraNode CurrentCamera
		{
			get
			{
				var camera = CurrentSceneWidget.Camera;

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

			_menuItemNewScene.Selected += (s, a) => NewScene();
			_menuItemNewPrefab.Selected += (s, a) => NewPrefab();

			_menuItemReload.Selected += (s, a) => Reload();

			_menuItemOpenFolder.Selected += (s, a) =>
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
					Folder = dialog.FilePath;
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

			_treeFileExplorer.SelectionChanged += OnExplorerNodeChanged;
			_treeFileSolution.TouchDoubleClick += OnOpenCurrentSolutionItem;
			_propertyGrid.ObjectChanged += OnObjectChanged;
			_propertyGrid.PropertyChanged += OnPropertyChanged;
			_propertyGrid.CustomWidgetProvider = CreateCustomEditor;

			_tabControlScenes.Items.Clear();
			_tabControlScenes.SelectedIndexChanged += (s, a) => RefreshExplorer(null);
			_tabControlScenes.ItemsCollectionChanged += (s, a) => UpdateStackPanelEditor();

			_buttonGrid.IsToggled = DigitalRiseEditorOptions.ShowGrid;
			_buttonVisualizeBuffers.IsToggled = DRDebugOptions.VisualizeBuffers;

			_buttonGrid.IsToggledChanged += (s, a) => UpdateDebugOptions();
			_buttonVisualizeBuffers.IsToggledChanged += (s, a) => UpdateDebugOptions();

			UpdateStackPanelEditor();
		}

		private Widget InternalCreateCustomEditor(Record record, object obj, string[] extensions, Func<string, object> loader)
		{
			var propertyType = record.Type;

			var pathProperty = obj.GetType().GetProperty(record.Name + "Path");
			var texturePath = (string)pathProperty.GetValue(obj);

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

							var value = loader(path);

							pathProperty.SetValue(obj, path);
							record.SetValue(obj, value);
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

			return button;
		}

		public Widget CreateCustomEditor(Record record, object obj)
		{
			if (obj is DefaultMaterial && record.Name.EndsWith("Texture"))
			{
				return InternalCreateCustomEditor(record, obj,
					new[] { "dds", "png", "jpg", "gif", "bmp", "tga" },
					path => AssetManager.LoadTexture2D(DR.GraphicsDevice, path));
			}
			else if (record.Type == typeof(TextureCube))
			{
				return InternalCreateCustomEditor(record, obj, new[] { "dds" },
					path => AssetManager.LoadTextureCube(DR.GraphicsDevice, path));

			}
			else if (record.Type == typeof(DrModel))
			{
				return InternalCreateCustomEditor(record, obj, new[] { "jdrm" },
					path => AssetManager.LoadJDRM(path));
			}
			else if (record.Type == typeof(SceneNode))
			{
				return InternalCreateCustomEditor(record, obj, new[] { "prefab" },
					path => AssetManager.LoadSceneNode(path).Clone());
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

		private void OnExplorerNodeChanged(object sender, EventArgs args)
		{
			_propertyGrid.Object = _treeFileExplorer.SelectedNode?.Tag;
		}

		private void EditMaterials(DrModelNode modelNode)
		{
			var dialog = new EditMaterialsDialog(modelNode);

			dialog.Closed += (s, a) =>
			 {
				 if (!dialog.Result)
				 {
					 // "Cancel" or Escape
					 return;
				 }

			 };

			dialog.ShowModal(Desktop);
		}

		private void OnObjectChanged(object sender, EventArgs args)
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

			_panelObjectButtons.Widgets.Clear();
			var asModel = _propertyGrid.Object as DrModelNode;
			if (asModel != null)
			{
				var buttonMaterials = new Button
				{
					Content = new Label
					{
						Text = "Materials..."
					}
				};

				buttonMaterials.Click += (s, a) => EditMaterials(asModel);
				_panelObjectButtons.Widgets.Add(buttonMaterials);
			}
		}

		private void OpenTab(SceneNode node, string file, bool isPrefab)
		{
			var sceneWidget = new SceneWidget
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
				SceneNode = node,
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

			var tabInfo = new TabInfo(file, isPrefab);

			var tabItem = new TabItem
			{
				Text = tabInfo.Title,
				Content = panel,
				Tag = tabInfo
			};

			tabInfo.TitleChanged += (s, a) => tabItem.Text = tabInfo.Title;

			_tabControlScenes.Items.Add(tabItem);
			_tabControlScenes.SelectedIndex = _tabControlScenes.Items.Count - 1;

			SelectNodeByTag(node);
		}

		private void OnOpenCurrentSolutionItem(object sender, EventArgs args)
		{
			try
			{
				var treeNode = _treeFileSolution.SelectedNode;
				if (treeNode == null || treeNode.Tag == null || !(treeNode.Tag is string))
				{
					return;
				}

				var file = (string)treeNode.Tag;
				if (file.EndsWith(".scene") || file.EndsWith(".prefab"))
				{
					if (SetTabByName(_tabControlScenes, file))
					{
						return;
					}

					// Load scene
					var sceneNode = AssetManager.LoadSceneNode(file);
					OpenTab(sceneNode, file, file.EndsWith(".prefab"));
				}
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
				return;
			}
		}

		private void OnPropertyChanged(object sender, GenericEventArgs<string> args)
		{
			InvalidateCurrentItem();

			switch (args.Data)
			{
				case "Id":
					UpdateTreeNodeId(_treeFileExplorer.SelectedNode);
					break;

				case "PrimitiveMeshType":
					_propertyGrid.Rebuild();
					break;
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

				var newNode = ot.CreateInstance();
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

		private void OnAddExternalResource<T>(SceneNode parent, string[] extensions, Func<string, T> creator) where T : SceneNode
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

						var node = creator(path);

						node.Load(AssetManager);

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

		private void OnAddModel(SceneNode parent)
		{
			OnAddExternalResource(parent, new[] { "jdrm" }, path => new DrModelNode
			{
				ModelPath = path
			});
		}

		private void OnAddPrefab(SceneNode parent)
		{
			OnAddExternalResource(parent, new[] { "prefab" }, path =>
			{
				var prefabNode = new PrefabNode
				{
					PrefabPath = path
				};

				return prefabNode;
			});
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

			// Prefab
			contextMenuOptions.Add(new Tuple<string, Action>($"Insert Prefab...", () => OnAddPrefab(sceneNode)));

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

		private void ReloadCurrentItem()
		{
			try
			{
				if (_tabControlScenes.SelectedItem == null)
				{
					return;
				}
				var tab = _tabControlScenes.SelectedItem;

				var tabInfo = (TabInfo)tab.Tag;


				var sceneNode = AssetManager.LoadSceneNode(tabInfo.FilePath);

				var sceneWidget = tab.Content.FindChild<SceneWidget>();
				var camera = sceneWidget.Camera.Clone();
				sceneWidget.SceneNode = sceneNode;
				sceneWidget.Camera = camera;

				RefreshExplorer(null);
			}
			catch (Exception ex)
			{
				var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
				dialog.ShowModal(Desktop);
				return;
			}
		}

		private void Reload()
		{
			RemoveScenesFromCache();
			ReloadCurrentItem();
		}

		private void NewScene()
		{
			var scene = new Scene();
			scene.SetDefaultCamera();

			OpenTab(scene, string.Empty, false);
		}

		private void NewPrefab()
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
					var newNode = dialog.NodeTypeInfo.CreateInstance();

					OpenTab(newNode, string.Empty, true);
				}
				catch (Exception ex)
				{
					var dialog = Dialog.CreateMessageBox("Error", ex.ToString());
					dialog.ShowModal(Desktop);
				}
			};

			dialog.ShowModal(Desktop);
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
				if (Directory.GetFiles(subFolder, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".scene") || s.EndsWith(".prefab")).ToArray().Length == 0)
				{
					continue;
				}

				ProcessNode(projectNode, subFolder);
			}

			// Add scene files
			var sceneFiles = Directory.GetFiles(folder, "*.*").Where(s => s.EndsWith(".scene") || s.EndsWith(".prefab")).ToArray();
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

		private void RefreshSolution()
		{
			_treeFileSolution.RemoveAllSubNodes();
			ProcessNode(_treeFileSolution, _folder);
		}

		private void UpdateTreeNodeId(TreeViewNode node)
		{
			var sceneNode = (SceneNode)node.Tag;
			var label = (Label)node.Content;

			var id = $"{sceneNode.GetType().Name} (#{sceneNode.Name})";
			label.Text = id;
		}

		private TreeViewNode RecursiveAddToExplorer(ITreeViewNode treeViewNode, SceneNode sceneNode)
		{
			var label = new Label();
			var newNode = treeViewNode.AddSubNode(label);
			newNode.IsExpanded = true;
			newNode.Tag = sceneNode;

			UpdateTreeNodeId(newNode);

			if (!(sceneNode is PrefabNode) && sceneNode.Children != null)
			{
				foreach (var child in sceneNode.Children)
				{
					RecursiveAddToExplorer(newNode, child);
				}
			}

			return newNode;
		}

		private void SelectNodeByTag(object obj)
		{
			_treeFileExplorer.SelectedNode = _treeFileExplorer.FindNode(n => n.Tag == obj);
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
			RecursiveAddToExplorer(_treeFileExplorer, sceneWidget.SceneNode);
			SelectNodeByTag(selectedNode);
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
				sceneWidget.SceneNode.SaveToFile(tabInfo.FilePath);
				tabInfo.Dirty = false;
			}
			else
			{
				var isPrefab = !(sceneWidget.SceneNode is Scene);

				var dlg = new FileDialog(FileDialogMode.SaveFile)
				{
					Filter = isPrefab ? "*.prefab" : "*.scene",
					Folder = Folder
				};

				dlg.ShowModal(Desktop);

				dlg.Closed += (s, a) =>
				{
					if (dlg.Result)
					{
						tabInfo.FilePath = dlg.FilePath;
						sceneWidget.SceneNode.SaveToFile(tabInfo.FilePath);
						tabInfo.Dirty = false;

						RefreshSolution();
					}
				};
			}
		}

		private void UpdateDebugOptions()
		{
			DigitalRiseEditorOptions.ShowGrid = _buttonGrid.IsToggled;
			DRDebugOptions.VisualizeBuffers = _buttonVisualizeBuffers.IsToggled;
		}

		public override void InternalRender(Myra.Graphics2D.RenderContext context)
		{
			if (_panelStatistics.Visible && CurrentSceneWidget != null)
			{
				var stats = CurrentSceneWidget.RenderStatistics;

				_labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
				_labelDrawCalls.Text = stats.DrawCalls.ToString();
				_labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
				_labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			}

			base.InternalRender(context);
		}

		public void RemoveScenesFromCache()
		{
			var toRemove = new List<string>();

			foreach (var pair in AssetManager.Cache)
			{
				if (pair.Value is SceneNode)
				{
					toRemove.Add(pair.Key);
				}
			}

			foreach (var key in toRemove)
			{
				AssetManager.Cache.Remove(key);
			}
		}
	}
}