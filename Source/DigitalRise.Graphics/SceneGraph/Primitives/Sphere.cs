using DigitalRise.Attributes;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Mathematics;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Sphere : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
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

		protected override Mesh CreateMesh() => MeshPrimitives.CreateSphereMesh(Radius, Tessellation, UScale, VScale, IsLeftHanded);

		public new Sphere Clone() => (Sphere)base.Clone();

		protected override SceneNode CreateInstanceCore() => new Sphere();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (Sphere)source;

			Radius = src.Radius;
			Tessellation = src.Tessellation;
		}
	}
}