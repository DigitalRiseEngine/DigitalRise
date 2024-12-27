using Microsoft.Xna.Framework;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public class Box : BasePrimitive
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
				Invalidate();
			}
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreateBoxMesh(Size, UScale, VScale, IsLeftHanded);

		public new Box Clone() => (Box)base.Clone();

		protected override BasePrimitive CreateInstanceCore() => new Box();

		protected override void CloneCore(BasePrimitive source)
		{
			base.CloneCore(source);

			var src = (Box)source;

			Size = src.Size;
		}
	}
}
