using AssetManagementBase;
using DigitalRise.Attributes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DigitalRise.SceneGraph
{
	[EditorInfo("Prefab")]
	public class PrefabNode: SceneNode
	{
		[Browsable(false)]
		public string PrefabPath { get; set; }

		[JsonIgnore]
		public SceneNode Prefab
		{
			get
			{
				if (Children.Count == 0)
				{
					return null;
				}

				return Children[0];
			}

			set
			{
				Children.Clear();

				if (value != null)
				{
					Children.Add(value);
					Shape = value.Shape;
				} else
				{
					Shape = Geometry.Shapes.Shape.Empty;
				}
			}
		}

		[Browsable(false)]
		[JsonIgnore]
		public override ObservableCollection<SceneNode> Children => base.Children;

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			Prefab = assetManager.LoadSceneNode(PrefabPath).Clone();
		}
	}
}
