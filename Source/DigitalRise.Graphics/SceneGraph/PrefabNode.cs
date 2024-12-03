using AssetManagementBase;
using DigitalRise.Geometry.Shapes;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DigitalRise.SceneGraph
{
	public class PrefabNode: SceneNode
	{
		[Browsable(false)]
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

				Children.Add(value);
				Shape = new TransformedShape(value.Shape, value.PoseLocal, value.ScaleLocal);
			}
		}


		[Browsable(false)]
		public string PrefabPath { get; set; }

		[Browsable(false)]
		[JsonIgnore]
		public override ObservableCollection<SceneNode> Children => base.Children;

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (!string.IsNullOrEmpty(PrefabPath))
			{
				Prefab = assetManager.LoadSceneNode(PrefabPath).Clone();
			}
		}

		public new PrefabNode Clone() => (PrefabNode)base.Clone();

		protected override SceneNode CreateInstanceCore() => new PrefabNode();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (PrefabNode)source;

			Prefab = src.Prefab.Clone();
		}
	}
}
