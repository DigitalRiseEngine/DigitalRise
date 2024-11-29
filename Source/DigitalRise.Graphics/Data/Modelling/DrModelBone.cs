using DigitalRise.Data.Meshes;
using Microsoft.Xna.Framework;

namespace DigitalRise.Data.Modelling
{
	public class DrModelBone
	{
		private DrModelBone[] _children;

		public int Index { get; }
		public string Name { get; }
		public DrModelBone Parent { get; private set; }

		public DrModelBone[] Children
		{
			get => _children;

			internal set
			{
				if (value != null)
				{
					foreach (var b in value)
					{
						b.Parent = this;
					}
				}

				_children = value;
			}

		}

		public Mesh Mesh { get; internal set; }

		public SrtTransform DefaultPose = SrtTransform.Identity;

		public Skin Skin { get; internal set; }

		internal DrModelBone(int index, string name)
		{
			Index = index;
			Name = name;
		}
		public override string ToString() => Name;

		public Matrix CalculateDefaultLocalTransform() => DefaultPose.ToMatrix();
		public Matrix CalculateDefaultAbsoluteTransform()
		{
			if (Parent == null)
			{
				return CalculateDefaultLocalTransform();
			}

			return CalculateDefaultLocalTransform() * Parent.CalculateDefaultAbsoluteTransform();
		}
	}
}
