using DigitalRise.Attributes;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Mathematics;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Teapot : PrimitiveMeshNode
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

		protected override Mesh CreateMesh() => MeshPrimitives.CreateTeapotMesh(Size, Tessellation, UScale, VScale, IsLeftHanded);
	}
}