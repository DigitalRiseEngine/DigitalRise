using Microsoft.Xna.Framework;
using DigitalRise.Attributes;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Data.Meshes;

namespace DigitalRise.SceneGraph.Primitives
{
	[EditorInfo("Primitive")]
	public class Box : PrimitiveMeshNode
	{
		private Vector3 _size = Vector3.One;

		public Vector3 Size
		{
			get => _size;

			set
			{
				if (Mathematics.MathHelper.AreNumericallyEqual(value, _size))
				{
					return;
				}

				_size = value;
				InvalidateMesh();
			}
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreateBoxMesh(Size, UScale, VScale, IsLeftHanded);
	}
}