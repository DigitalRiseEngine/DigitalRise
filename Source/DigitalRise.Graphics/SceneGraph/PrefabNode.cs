using AssetManagementBase;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Rendering.Deferred;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace DigitalRise.SceneGraph
{
	public class PrefabNode: SceneNode
	{
		private readonly List<SceneNode> _actualChildren = new List<SceneNode>();
		private SceneNode _prefab;
		private bool _childrenDirty = true;

		[Browsable(false)]
		[JsonIgnore]
		public SceneNode Prefab
		{
			get => _prefab;

			set
			{
				if (value == _prefab)
				{
					return;
				}

				if (_prefab != null)
				{
					_prefab.Parent = null;
				}

				_prefab = value;
				_childrenDirty = true;

				if (value != null)
				{
					value.Parent = this;

					if (value.Shape is InfiniteShape || value.Shape is EmptyShape)
					{
						Shape = value.Shape;
					}
					else
					{
						Shape = new TransformedShape(value.Shape, value.PoseLocal, value.ScaleLocal);
					}
				} else
				{
					Shape = Shape.Empty;
				}
			}
		}


		[Browsable(false)]
		public string PrefabPath { get; set; }

		internal override IReadOnlyCollection<SceneNode> ActualChildren
		{
			get
			{
				UpdateActualChildren();

				return _actualChildren;
			}
		}

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

		private void UpdateActualChildren()
		{
			if (!_childrenDirty)
			{
				return;
			}

			_actualChildren.Clear();

			// Add prefab first
			if (_prefab != null)
			{
				_actualChildren.Add(_prefab);
			}

			// Then other children
			_actualChildren.AddRange(Children);

			_childrenDirty = false;
		}

		protected override void OnChildrenChanged()
		{
			base.OnChildrenChanged();

			_childrenDirty = true;
		}

		public override void BatchJobs(IRenderList list)
		{
			base.BatchJobs(list);
		}
	}
}
