using Myra.Graphics2D.UI;

namespace DigitalRise.Editor.UI
{
	public partial class AddNewItemDialog : Dialog
	{
		public int? SelectedIndex
		{
			get
			{
				int? result = null;
				for(var i = 0; i < _itemsPanel.Widgets.Count; ++i)
				{
					var radio = (RadioButton)_itemsPanel.Widgets[i];
					if (radio.IsPressed)
					{
						result = i;
						break;
					}
				}

				return result;
			}
		}

		public AddNewItemDialog()
		{
			BuildUI();

			_itemsPanel.Widgets.Clear();
		}

		public int AddItem(string text)
		{
			var radio = new RadioButton
			{
				Content = new Label
				{
					Text = text
				}
			};

			var result = _itemsPanel.Widgets.Count;
			_itemsPanel.Widgets.Add(radio);

			if (result == 0)
			{
				radio.IsPressed = true;
			}

			return result;
		}
	}
}