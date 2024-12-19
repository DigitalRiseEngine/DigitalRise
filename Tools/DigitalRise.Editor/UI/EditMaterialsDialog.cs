using DigitalRise.SceneGraph;
using Myra.Graphics2D.UI;
using System;

namespace DigitalRise.Editor.UI
{
	public partial class EditMaterialsDialog
	{
		public DrModelNode ModelNode { get; }

		public event EventHandler Changed;

		public EditMaterialsDialog(DrModelNode modelNode)
		{
			ModelNode = modelNode ?? throw new ArgumentNullException(nameof(modelNode));

			BuildUI();

			foreach (var material in ModelNode.Materials)
			{
				var label = new Label
				{
					Text = material.Name,
					Tag = material
				};

				_listMaterials.Widgets.Add(label);
			}

			_listMaterials.SelectedIndexChanged += _listMaterials_SelectedIndexChanged;
			_properties.CustomWidgetProvider = StudioGame.MainForm.CreateCustomEditor;
			_properties.PropertyChanged += (s, a) => Changed?.Invoke(this, EventArgs.Empty);
		}

		private void _listMaterials_SelectedIndexChanged(object sender, EventArgs e)
		{
			_properties.Object = _listMaterials.SelectedItem.Tag;
		}
	}
}