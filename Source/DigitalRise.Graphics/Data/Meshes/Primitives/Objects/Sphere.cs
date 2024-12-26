using DigitalRise.Mathematics;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public class Sphere : BasePrimitive
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

		protected override BasePrimitive CreateInstanceCore() => new Sphere();

		protected override void CloneCore(BasePrimitive source)
		{
			base.CloneCore(source);

			var src = (Sphere)source;

			Radius = src.Radius;
			Tessellation = src.Tessellation;
		}
	}
}