using DigitalRise.Attributes;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Mathematics;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Capsule : PrimitiveMeshNode
	{
		private float _length = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 8;

		public float Length
		{
			get => _length;

			set
			{
				if (Numeric.AreEqual(value, _length))
				{
					return;
				}

				_length = value;
				InvalidateMesh();
			}
		}

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

		protected override Mesh CreateMesh() => MeshPrimitives.CreateCapsuleMesh(Length, Radius, Tessellation, UScale, VScale, IsLeftHanded);

		public new Capsule Clone() => (Capsule)base.Clone();

		protected override SceneNode CreateInstanceCore() => new Capsule();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (Capsule)source;

			Length = src.Length;
			Radius = src.Radius;
			Tessellation = src.Tessellation;
		}
	}
}