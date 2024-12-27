using DigitalRise.Mathematics;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public class Teapot : BasePrimitive
	{
		private float _size = 1.0f;
		private int _tessellation = 8;

		public float Size
		{
			get => _size;

			set
			{
				if (Numeric.AreEqual(value, _size))
				{
					return;
				}

				_size = value;
				Invalidate();
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
				Invalidate();
			}
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreateTeapotMesh(Size, Tessellation, UScale, VScale, IsLeftHanded);

		public new Teapot Clone() => (Teapot)base.Clone();

		protected override BasePrimitive CreateInstanceCore() => new Teapot();

		protected override void CloneCore(BasePrimitive source)
		{
			base.CloneCore(source);

			var src = (Teapot)source;

			Size = src.Size;
			Tessellation = src.Tessellation;
		}
	}
}