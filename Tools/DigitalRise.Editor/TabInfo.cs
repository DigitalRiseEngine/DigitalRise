using System;
using System.IO;

namespace DigitalRise.Editor
{
	internal class TabInfo
	{
		private bool IsPrefab;
		private string _filePath;
		private bool _dirty;

		public string FilePath
		{
			get => _filePath;

			set
			{
				if (value == _filePath)
				{
					return;
				}

				_filePath = value;
				TitleChanged?.Invoke(this, EventArgs.Empty);
			}
		}


		public bool Dirty
		{
			get => _dirty;

			set
			{
				if (value == _dirty)
				{
					return;
				}

				_dirty = value;
				TitleChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public string Title
		{
			get
			{
				string title;
				if(string.IsNullOrEmpty(FilePath))
				{
					title = IsPrefab ? "New Prefab" : "New Scene";
				} else
				{
					title = Path.GetFileName(FilePath);
				}

				if (Dirty)
				{
					title += " *";
				}

				return title;
			}
		}

		public event EventHandler TitleChanged;

		public TabInfo(string filePath, bool isPrefab)
		{
			FilePath = filePath;
			IsPrefab = isPrefab;
		}
	}
}
