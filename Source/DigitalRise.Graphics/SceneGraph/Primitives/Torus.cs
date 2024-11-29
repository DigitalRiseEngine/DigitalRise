using DigitalRise.Attributes;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Mathematics;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Torus : PrimitiveMeshNode
	{
		private float _majorRadius = 0.5f;
		private float _minorRadius = 0.16666f;
		private int _tessellation = 32;

		public float MajorRadius
		{
			get => _majorRadius;

			set
			{
				if (Numeric.AreEqual(value, _majorRadius))
				{
					return;
				}

				_majorRadius = value;
				InvalidateMesh();
			}
		}

		public float MinorRadius
		{
			get => _minorRadius;

			set
			{
				if (Numeric.AreEqual(value, _minorRadius))
				{
					return;
				}

				_minorRadius = value;
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

		protected override Mesh CreateMesh() => MeshPrimitives.CreateTorusMesh(MajorRadius, MinorRadius, Tessellation, UScale, VScale, IsLeftHanded);
	}
}