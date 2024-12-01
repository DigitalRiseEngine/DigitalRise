using DigitalRise.Data.Meshes;
using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DigitalRise
{
	internal static class InternalPrimitives
	{
		private static Submesh _untexturedBox;

		public static Submesh UntexturedBox
		{
			get
			{
				if (_untexturedBox != null)
				{
					return _untexturedBox;
				}

				var vertices = new List<VertexPositionNormal>();
				var indices = new List<ushort>();

				var p0 = new Vector3(-0.5f, -0.5f, -0.5f);
				var p1 = new Vector3(-0.5f, -0.5f, +0.5f);
				var p2 = new Vector3(-0.5f, +0.5f, -0.5f);
				var p3 = new Vector3(-0.5f, +0.5f, +0.5f);
				var p4 = new Vector3(+0.5f, -0.5f, -0.5f);
				var p5 = new Vector3(+0.5f, -0.5f, +0.5f);
				var p6 = new Vector3(+0.5f, +0.5f, -0.5f);
				var p7 = new Vector3(+0.5f, +0.5f, +0.5f);

				var normal = Vector3.UnitX;
				vertices.Add(new VertexPositionNormal(p4, normal));
				vertices.Add(new VertexPositionNormal(p5, normal));
				vertices.Add(new VertexPositionNormal(p6, normal));
				vertices.Add(new VertexPositionNormal(p7, normal));

				indices.Add(0);
				indices.Add(1);
				indices.Add(2);

				indices.Add(1);
				indices.Add(3);
				indices.Add(2);

				normal = Vector3.UnitY;
				vertices.Add(new VertexPositionNormal(p6, normal));
				vertices.Add(new VertexPositionNormal(p7, normal));
				vertices.Add(new VertexPositionNormal(p2, normal));
				vertices.Add(new VertexPositionNormal(p3, normal));

				indices.Add(4);
				indices.Add(5);
				indices.Add(6);

				indices.Add(5);
				indices.Add(7);
				indices.Add(6);

				normal = Vector3.UnitZ;
				vertices.Add(new VertexPositionNormal(p5, normal));
				vertices.Add(new VertexPositionNormal(p1, normal));
				vertices.Add(new VertexPositionNormal(p7, normal));
				vertices.Add(new VertexPositionNormal(p3, normal));

				indices.Add(8);
				indices.Add(9);
				indices.Add(10);

				indices.Add(9);
				indices.Add(11);
				indices.Add(10);

				normal = -Vector3.UnitX;
				vertices.Add(new VertexPositionNormal(p1, normal));
				vertices.Add(new VertexPositionNormal(p0, normal));
				vertices.Add(new VertexPositionNormal(p3, normal));
				vertices.Add(new VertexPositionNormal(p2, normal));

				indices.Add(12);
				indices.Add(13);
				indices.Add(14);

				indices.Add(13);
				indices.Add(15);
				indices.Add(14);

				normal = -Vector3.UnitY;
				vertices.Add(new VertexPositionNormal(p4, normal));
				vertices.Add(new VertexPositionNormal(p0, normal));
				vertices.Add(new VertexPositionNormal(p5, normal));
				vertices.Add(new VertexPositionNormal(p1, normal));

				indices.Add(16);
				indices.Add(17);
				indices.Add(18);

				indices.Add(17);
				indices.Add(19);
				indices.Add(18);

				normal = -Vector3.UnitZ;
				vertices.Add(new VertexPositionNormal(p0, normal));
				vertices.Add(new VertexPositionNormal(p4, normal));
				vertices.Add(new VertexPositionNormal(p2, normal));
				vertices.Add(new VertexPositionNormal(p6, normal));

				indices.Add(20);
				indices.Add(21);
				indices.Add(22);

				indices.Add(21);
				indices.Add(23);
				indices.Add(22);

				_untexturedBox = new Submesh(vertices.ToArray(), indices.ToArray());

				return _untexturedBox;
			}
		}
	}
}