using DigitalRise.SceneGraph;
using Myra.Graphics2D.UI;
using System;

namespace DigitalRise.Editor.UI
{
	public partial class EditMaterialsDialog
	{
		private readonly TreeView _treeMeshes;

		public DrModelNode ModelNode { get; }

		public EditMaterialsDialog(DrModelNode modelNode)
		{
			ModelNode = modelNode ?? throw new ArgumentNullException(nameof(modelNode));

			BuildUI();

			_treeMeshes = new TreeView();

			foreach (var mesh in ModelNode.MeshMaterials)
			{
				var meshTreeNode = _treeMeshes.AddSubNode(new Label
				{
					Text = mesh.MeshName
				});

				if (mesh.Materials.Length == 1)
				{
					meshTreeNode.Tag = mesh.Materials[0];
				}
				else
				{
					for (var i = 0; i < mesh.Materials.Length; ++i)
					{
						var subMeshTreeNode = meshTreeNode.AddSubNode(new Label
						{
							Text = $"{i}"
						});

						subMeshTreeNode.Tag = mesh.Materials[i];
					}

					meshTreeNode.IsExpanded = true;
				}
			}

			_treeMeshes.SelectionChanged += _treeMeshes_SelectionChanged;

			_panelLeft.Widgets.Add(_treeMeshes);
		}

		private void _treeMeshes_SelectionChanged(object sender, EventArgs e)
		{
			_properties.Object = _treeMeshes.SelectedNode.Tag;
		}
	}
}