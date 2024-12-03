using DigitalRise.Attributes;
using DigitalRise.Data.Meshes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Mathematics;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class GeoSphere : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private int _tessellation = 3;

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

		protected override Mesh CreateMesh() => MeshPrimitives.CreateGeoSphereMesh(Radius, Tessellation, UScale, VScale, IsLeftHanded);

		public new GeoSphere Clone() => (GeoSphere)base.Clone();

		protected override SceneNode CreateInstanceCore() => new GeoSphere();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (GeoSphere)source;

			Radius = src.Radius;
			Tessellation = src.Tessellation;
		}
	}
}