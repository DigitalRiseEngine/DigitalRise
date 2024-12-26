using DigitalRise.Mathematics;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public class Cone : BasePrimitive
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

		protected override BasePrimitive CreateInstanceCore() => new Cone();

		protected override void CloneCore(BasePrimitive source)
		{
			base.CloneCore(source);

			var src = (Cone)source;

			Radius = src.Radius;
			Height = src.Height;
			Tessellation = src.Tessellation;
		}
	}
}