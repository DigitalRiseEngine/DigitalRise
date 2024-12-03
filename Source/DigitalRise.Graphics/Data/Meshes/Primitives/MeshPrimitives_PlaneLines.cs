using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DigitalRise.Data.Meshes.Primitives
{
	partial class MeshPrimitives
	{
		public static Submesh CreatePlaneLinesSubmesh(int size)
		{
			var vertices = new List<VertexPosition>();
			var indices = new List<ushort>();

			ushort idx = 0;
			for (var x = -size; x <= size; ++x)
			{
				vertices.Add(new VertexPosition
				{
					Position = new Vector3(x, 0, -size)
				});

				vertices.Add(new VertexPosition
				{
					Position = new Vector3(x, 0, size)
				});

				indices.Add(idx);
				++idx;
				indices.Add(idx);
				++idx;
			}

			for (var z = -size; z <= size; ++z)
			{
				vertices.Add(new VertexPosition
				{
					Position = new Vector3(-size, 0, z)
				});

				vertices.Add(new VertexPosition
				{
					Position = new Vector3(size, 0, z)
				});

				indices.Add(idx);
				++idx;
				indices.Add(idx);
				++idx;
			}

			return new Submesh(vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);
		}
	}
}
