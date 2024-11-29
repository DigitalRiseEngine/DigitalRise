using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise.Data.Meshes.Primitives
{
	partial class MeshPrimitives
	{
		/// <summary>
		/// Creates a new submesh that represents a box using lines.
		/// (The box is centered at the origin. The side length is 1.)
		/// </summary>
		/// <returns>A new <see cref="Submesh"/> that represents a box line list.</returns>
		/// <remarks>
		/// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
		/// to call <see cref="GetBoxLines"/> to retrieve a shared <see cref="Submesh"/> instance.
		/// </remarks>
		public static Submesh CreateBoxLinesSubmesh()
		{
			var vertices = new[]
			{
				new Vector3(-0.5f, -0.5f, +0.5f),
				new Vector3(+0.5f, -0.5f, +0.5f),
				new Vector3(+0.5f, +0.5f, +0.5f),
				new Vector3(-0.5f, +0.5f, +0.5f),
				new Vector3(-0.5f, -0.5f, -0.5f),
				new Vector3(+0.5f, -0.5f, -0.5f),
				new Vector3(+0.5f, +0.5f, -0.5f),
				new Vector3(-0.5f, +0.5f, -0.5f)
			};

			var graphicsDevice = DR.GraphicsDevice;

			var indices = new ushort[]
			{
				0, 1,
				1, 2,
				2, 3,
				3, 0,

				4, 5,
				5, 6,
				6, 7,
				7, 4,

				0, 4,
				1, 5,
				2, 6,
				3, 7
			};

			return new Submesh(vertices, indices, PrimitiveType.LineList);
		}
	}
}
