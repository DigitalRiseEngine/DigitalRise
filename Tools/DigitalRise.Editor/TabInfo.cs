using System;
using System.IO;

namespace DigitalRise.Editor
{
	internal class TabInfo
	{
		private bool _dirty;

		public string FilePath { get; set; }
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
				var title = !string.IsNullOrEmpty(FilePath) ? Path.GetFileName(FilePath) : "New Scene";

				if (Dirty)
				{
					title += " *";
				}

				return title;
			}
		}

		public event EventHandler TitleChanged;

		public TabInfo(string filePath)
		{
			FilePath = filePath;
		}
	}
}
