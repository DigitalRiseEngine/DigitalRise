using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DigitalRise.TextureConverter
{
	public class AsciiGrid
	{
		private readonly Dictionary<int, string> _values = new Dictionary<int, string>();
		private readonly Dictionary<int, int> _maximumWidths = new Dictionary<int, int>();

		public int ColSpace { get; set; } = 2;

		private int GetKey(int x, int y) => y << 16 | x;

		private static Point KeyToPosition(int key) => new Point(key & 0xffff, key >> 16);

		public void SetMaximumWidth(int x, int maximumWidth)
		{
			_maximumWidths[x] = maximumWidth;
		}

		public int? GetMaximumWidth(int x)
		{
			int result;
			if (!_maximumWidths.TryGetValue(x, out result))
			{
				return null;
			}

			return result;
		}

		public void SetValue(int x, int y, string value)
		{
			var key = GetKey(x, y);
			_values[key] = value;
		}

		public string GetValue(int x, int y)
		{
			var key = GetKey(x, y);
			string result;
			if (!_values.TryGetValue(key, out result) || result == null)
			{
				return string.Empty;
			}

			return result;
		}

		public override string ToString()
		{
			// Determine max x and y
			var max = new Point(0, 0);
			foreach (var pair in _values)
			{
				var pos = KeyToPosition(pair.Key);
				if (pos.X > max.X)
				{
					max.X = pos.X;
				}

				if (pos.Y > max.Y)
				{
					max.Y = pos.Y;
				}
			}

			// Determine column widths
			var colWidths = new int[max.X + 1];
			foreach (var pair in _values)
			{
				var pos = KeyToPosition(pair.Key);
				var size = pair.Value != null ? pair.Value.Length : 0;

				var maximumWidth = GetMaximumWidth(pos.X);
				if (maximumWidth != null && size > maximumWidth.Value)
				{
					size = maximumWidth.Value;
				}

				if (size > colWidths[pos.X])
				{
					colWidths[pos.X] = size;
				}
			}

			var spaceString = string.Empty.PadRight(ColSpace);
			var sb = new StringBuilder();
			for (var y = 0; y <= max.Y; ++y)
			{
				for (var x = 0; x <= max.X; ++x)
				{
					var value = GetValue(x, y);
					var width = string.IsNullOrEmpty(value) ? 0 : value.Length;
					value = value.PadRight(colWidths[x]);
					sb.Append(value);
					if (x < max.X)
					{
						sb.Append(spaceString);
					}

					var maximumWidth = GetMaximumWidth(x);
					if (maximumWidth != null && width > maximumWidth.Value)
					{
						sb.AppendLine();
						sb.Append("\t");
					}
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
