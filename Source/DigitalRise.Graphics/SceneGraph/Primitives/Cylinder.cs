using DigitalRise.Attributes;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Mathematics;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Cylinder : PrimitiveMeshNode
	{
		private float _height = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 32;

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

		protected override Mesh CreateMesh() => MeshPrimitives.CreateCylinderMesh(Height, Radius, Tessellation, UScale, VScale, IsLeftHanded);
	}
}
