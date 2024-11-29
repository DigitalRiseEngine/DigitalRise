/* Generated by MyraPad at 11/28/2024 5:33:14 AM */
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI.Properties;
using FontStashSharp.RichText;
using AssetManagementBase;

#if STRIDE
using Stride.Core.Mathematics;
#elif PLATFORM_AGNOSTIC
using System.Drawing;
using System.Numerics;
using Color = FontStashSharp.FSColor;
#else
// MonoGame/FNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

namespace DigitalRise.Editor.UI
{
	partial class MainForm: VerticalStackPanel
	{
		private void BuildUI()
		{
			_menuItemNew = new MenuItem();
			_menuItemNew.Text = "&New";
			_menuItemNew.Id = "_menuItemNew";

			_menuItemOpenSolution = new MenuItem();
			_menuItemOpenSolution.Text = "&Open Solution";
			_menuItemOpenSolution.Id = "_menuItemOpenSolution";

			_menuItemSaveCurrentItem = new MenuItem();
			_menuItemSaveCurrentItem.Text = "&Save Current Item";
			_menuItemSaveCurrentItem.Id = "_menuItemSaveCurrentItem";

			_menuItemSaveEverything = new MenuItem();
			_menuItemSaveEverything.Text = "&Save Everything";
			_menuItemSaveEverything.Id = "_menuItemSaveEverything";

			_menuItemQuit = new MenuItem();
			_menuItemQuit.Text = "&Quit";
			_menuItemQuit.Id = "_menuItemQuit";

			var menuItem1 = new MenuItem();
			menuItem1.Text = "&File";
			menuItem1.Items.Add(_menuItemNew);
			menuItem1.Items.Add(_menuItemOpenSolution);
			menuItem1.Items.Add(_menuItemSaveCurrentItem);
			menuItem1.Items.Add(_menuItemSaveEverything);
			menuItem1.Items.Add(_menuItemQuit);

			_menuItemAbout = new MenuItem();
			_menuItemAbout.Text = "&About";
			_menuItemAbout.Id = "_menuItemAbout";

			var menuItem2 = new MenuItem();
			menuItem2.Text = "&Help";
			menuItem2.Items.Add(_menuItemAbout);

			var horizontalMenu1 = new HorizontalMenu();
			horizontalMenu1.Items.Add(menuItem1);
			horizontalMenu1.Items.Add(menuItem2);

			_panelSceneExplorer = new ScrollViewer();
			_panelSceneExplorer.Id = "_panelSceneExplorer";

			_panelSolution = new ScrollViewer();
			_panelSolution.Id = "_panelSolution";

			_leftSplitPane = new VerticalSplitPane();
			_leftSplitPane.Id = "_leftSplitPane";
			_leftSplitPane.Widgets.Add(_panelSceneExplorer);
			_leftSplitPane.Widgets.Add(_panelSolution);

			var label1 = new Label();
			label1.Text = "Grid";

			_buttonGrid = new ToggleButton();
			_buttonGrid.Id = "_buttonGrid";
			_buttonGrid.Content = label1;

			var label2 = new Label();
			label2.Text = "Bounding Boxes";

			_buttonBoundingBoxes = new ToggleButton();
			_buttonBoundingBoxes.Id = "_buttonBoundingBoxes";
			_buttonBoundingBoxes.Content = label2;

			var label3 = new Label();
			label3.Text = "Light View Frustum";

			_buttonLightViewFrustum = new ToggleButton();
			_buttonLightViewFrustum.Id = "_buttonLightViewFrustum";
			_buttonLightViewFrustum.Content = label3;

			var label4 = new Label();
			label4.Text = "Render Buffers";

			_buttonVisualizeBuffers = new ToggleButton();
			_buttonVisualizeBuffers.Id = "_buttonVisualizeBuffers";
			_buttonVisualizeBuffers.Content = label4;

			var horizontalStackPanel1 = new HorizontalStackPanel();
			horizontalStackPanel1.Spacing = 8;
			horizontalStackPanel1.Widgets.Add(_buttonGrid);
			horizontalStackPanel1.Widgets.Add(_buttonBoundingBoxes);
			horizontalStackPanel1.Widgets.Add(_buttonLightViewFrustum);
			horizontalStackPanel1.Widgets.Add(_buttonVisualizeBuffers);

			var label5 = new Label();
			label5.Text = "Camera View";

			var toggleButton1 = new ToggleButton();
			toggleButton1.Content = label5;

			var horizontalStackPanel2 = new HorizontalStackPanel();
			horizontalStackPanel2.Left = 2;
			horizontalStackPanel2.Top = 2;
			horizontalStackPanel2.Widgets.Add(toggleButton1);

			var tabItem1 = new TabItem();
			tabItem1.Text = "Test";
			tabItem1.Content = horizontalStackPanel2;

			_tabControlScenes = new TabControl();
			_tabControlScenes.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Stretch;
			_tabControlScenes.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Stretch;
			_tabControlScenes.CloseableTabs = true;
			_tabControlScenes.Id = "_tabControlScenes";
			_tabControlScenes.Items.Add(tabItem1);

			var label6 = new Label();
			label6.Text = "Effects Switches:";

			var label7 = new Label();
			label7.Text = "Draw Calls:";
			Grid.SetRow(label7, 1);

			var label8 = new Label();
			label8.Text = "Vertices Drawn:";
			Grid.SetRow(label8, 2);

			var label9 = new Label();
			label9.Text = "Primitives Drawn:";
			Grid.SetRow(label9, 3);

			_labelEffectsSwitches = new Label();
			_labelEffectsSwitches.Text = "10";
			_labelEffectsSwitches.Id = "_labelEffectsSwitches";
			Grid.SetColumn(_labelEffectsSwitches, 1);

			_labelDrawCalls = new Label();
			_labelDrawCalls.Text = "10";
			_labelDrawCalls.Id = "_labelDrawCalls";
			Grid.SetColumn(_labelDrawCalls, 1);
			Grid.SetRow(_labelDrawCalls, 1);

			_labelVerticesDrawn = new Label();
			_labelVerticesDrawn.Text = "10";
			_labelVerticesDrawn.Id = "_labelVerticesDrawn";
			Grid.SetColumn(_labelVerticesDrawn, 1);
			Grid.SetRow(_labelVerticesDrawn, 2);

			_labelPrimitivesDrawn = new Label();
			_labelPrimitivesDrawn.Text = "10";
			_labelPrimitivesDrawn.Id = "_labelPrimitivesDrawn";
			Grid.SetColumn(_labelPrimitivesDrawn, 1);
			Grid.SetRow(_labelPrimitivesDrawn, 3);

			_panelStatistics = new Grid();
			_panelStatistics.ColumnSpacing = 8;
			_panelStatistics.DefaultRowProportion = new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			};
			_panelStatistics.ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Auto,
			});
			_panelStatistics.ColumnsProportions.Add(new Proportion
			{
				Type = Myra.Graphics2D.UI.ProportionType.Fill,
			});
			_panelStatistics.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Right;
			_panelStatistics.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Bottom;
			_panelStatistics.Left = -4;
			_panelStatistics.Id = "_panelStatistics";
			_panelStatistics.Widgets.Add(label6);
			_panelStatistics.Widgets.Add(label7);
			_panelStatistics.Widgets.Add(label8);
			_panelStatistics.Widgets.Add(label9);
			_panelStatistics.Widgets.Add(_labelEffectsSwitches);
			_panelStatistics.Widgets.Add(_labelDrawCalls);
			_panelStatistics.Widgets.Add(_labelVerticesDrawn);
			_panelStatistics.Widgets.Add(_labelPrimitivesDrawn);

			var panel1 = new Panel();
			StackPanel.SetProportionType(panel1, Myra.Graphics2D.UI.ProportionType.Fill);
			panel1.Widgets.Add(_tabControlScenes);
			panel1.Widgets.Add(_panelStatistics);

			_panelScenes = new VerticalStackPanel();
			_panelScenes.Id = "_panelScenes";
			_panelScenes.Widgets.Add(horizontalStackPanel1);
			_panelScenes.Widgets.Add(panel1);

			var panel2 = new Panel();
			panel2.Widgets.Add(_panelScenes);

			_propertyGrid = new PropertyGrid();
			_propertyGrid.Id = "_propertyGrid";

			var scrollViewer1 = new ScrollViewer();
			scrollViewer1.Content = _propertyGrid;

			_topSplitPane = new HorizontalSplitPane();
			_topSplitPane.Id = "_topSplitPane";
			_topSplitPane.Widgets.Add(_leftSplitPane);
			_topSplitPane.Widgets.Add(panel2);
			_topSplitPane.Widgets.Add(scrollViewer1);

			var panel3 = new Panel();
			StackPanel.SetProportionType(panel3, Myra.Graphics2D.UI.ProportionType.Fill);
			panel3.Widgets.Add(_topSplitPane);

			
			Id = "_mainPanel";
			Widgets.Add(horizontalMenu1);
			Widgets.Add(panel3);
		}

		
		public MenuItem _menuItemNew;
		public MenuItem _menuItemOpenSolution;
		public MenuItem _menuItemSaveCurrentItem;
		public MenuItem _menuItemSaveEverything;
		public MenuItem _menuItemQuit;
		public MenuItem _menuItemAbout;
		public ScrollViewer _panelSceneExplorer;
		public ScrollViewer _panelSolution;
		public VerticalSplitPane _leftSplitPane;
		public ToggleButton _buttonGrid;
		public ToggleButton _buttonBoundingBoxes;
		public ToggleButton _buttonLightViewFrustum;
		public ToggleButton _buttonVisualizeBuffers;
		public TabControl _tabControlScenes;
		public Label _labelEffectsSwitches;
		public Label _labelDrawCalls;
		public Label _labelVerticesDrawn;
		public Label _labelPrimitivesDrawn;
		public Grid _panelStatistics;
		public VerticalStackPanel _panelScenes;
		public PropertyGrid _propertyGrid;
		public HorizontalSplitPane _topSplitPane;
	}
}