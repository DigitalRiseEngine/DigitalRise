/* Generated by MyraPad at 12/3/2024 2:14:38 AM */
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
	partial class ChooseAssetDialog: Dialog
	{
		private void BuildUI()
		{
			var label1 = new Label();
			label1.Text = "Filter:";

			_textFilter = new TextBox();
			_textFilter.Id = "_textFilter";
			StackPanel.SetProportionType(_textFilter, Myra.Graphics2D.UI.ProportionType.Fill);

			var horizontalStackPanel1 = new HorizontalStackPanel();
			horizontalStackPanel1.Spacing = 8;
			horizontalStackPanel1.Widgets.Add(label1);
			horizontalStackPanel1.Widgets.Add(_textFilter);

			var label2 = new Label();
			label2.Text = "dude.glb";

			var label3 = new Label();
			label3.Text = "maximo.gltf";

			_listAssets = new ListView();
			_listAssets.HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment.Stretch;
			_listAssets.VerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment.Stretch;
			_listAssets.Id = "_listAssets";
			StackPanel.SetProportionType(_listAssets, Myra.Graphics2D.UI.ProportionType.Fill);
			_listAssets.Widgets.Add(label2);
			_listAssets.Widgets.Add(label3);

			var verticalStackPanel1 = new VerticalStackPanel();
			verticalStackPanel1.Spacing = 8;
			verticalStackPanel1.Widgets.Add(horizontalStackPanel1);
			verticalStackPanel1.Widgets.Add(_listAssets);

			
			Title = "Choose Asset";
			Left = 531;
			Top = 78;
			Width = 500;
			Height = 500;
			Content = verticalStackPanel1;
		}

		
		public TextBox _textFilter;
		public ListView _listAssets;
	}
}
