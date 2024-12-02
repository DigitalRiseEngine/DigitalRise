using DigitalRise.Editor.Utility;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace DigitalRise.Editor.UI
{
	public partial class ChooseAssetDialog
	{
		private readonly List<string> _files = new List<string>();

		private string AssetFolder { get; }

		public string FilePath
		{
			get
			{
				if (_listAssets.SelectedItem == null)
				{
					return null;
				}

				return (string)_listAssets.SelectedItem.Tag;
			}
		}

		public ChooseAssetDialog(string assetFolder, string[] assetExtensions)
		{
			BuildUI();

			if (!Directory.Exists(assetFolder))
			{
				throw new Exception($"Could not find folder {assetFolder} that is supposed to have project's assets.");
			}

			AssetFolder = assetFolder;

			var allFiles = Directory.GetFiles(assetFolder, "*.*", SearchOption.AllDirectories);

			foreach (var f in allFiles)
			{
				foreach (var ae in assetExtensions)
				{
					if (f.EndsWith("." + ae, StringComparison.OrdinalIgnoreCase))
					{
						_files.Add(f);
						break;
					}
				}
			}

			if (_files.Count == 0)
			{
				var ext = string.Join('/', assetExtensions);
				throw new Exception($"Folder {assetFolder} contains no asset({ext}) files.");
			}

			_listAssets.SelectedIndexChanged += (s, a) => UpdateEnabled();
			_textFilter.TextChanged += (s, a) => UpdateList();

			UpdateList();
		}

		private void UpdateList()
		{
			_listAssets.Widgets.Clear();
			foreach (var model in _files)
			{
				var path = PathUtils.TryToMakePathRelativeTo(model, AssetFolder);

				if (!string.IsNullOrEmpty(_textFilter.Text) &&
					!path.Contains(_textFilter.Text, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				var label = new Label
				{
					Text = path,
					Tag = model
				};

				_listAssets.Widgets.Add(label);
			}

			UpdateEnabled();
		}

		private void UpdateEnabled()
		{
			ButtonOk.Enabled = _listAssets.SelectedIndex != null;
		}
	}
}