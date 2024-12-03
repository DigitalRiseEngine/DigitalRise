using DigitalRise.Attributes;
using DigitalRise.Mathematics;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Data.Meshes;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Cone : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private float _height = 1.0f;
		private int _tessellation = 16;

		public float Radius
		{
			get => _radius;

			set
			{
				if (Numeric.AreEqual(value, _radius))
				{
					return;
				}

				_radius = value;
				InvalidateMesh();
			}
		}

		public float Height
		{
			get => _height;

			set
			{
				if (Numeric.AreEqual(value, _height))
				{
					return;
				}

				_height = value;
				InvalidateMesh();
			}
		}

		public int Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreateConeMesh(Radius, Height, Tessellation, UScale, VScale, IsLeftHanded);

		public new Cone Clone() => (Cone)base.Clone();

		protected override SceneNode CreateInstanceCore() => new Cone();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (Cone)source;

			Radius = src.Radius;
			Height = src.Height;
			Tessellation = src.Tessellation;
		}
	}
}