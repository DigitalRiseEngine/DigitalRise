// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Data.Materials;
using DigitalRise.Mathematics;
using DigitalRise.Vertices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Data.Meshes.Primitives
{
	partial class MeshPrimitives
	{
		/// <summary>
		/// Creates a new submesh that represents a spherical cap using lines.
		/// (The sphere is centered at the origin. Radius = 1. The submesh contains only the 
		/// top half (+y) of the sphere.) 
		/// </summary>
		/// <param name="numberOfSegments">
		/// The number of segments. This parameter controls the detail of the mesh.</param>
		/// <returns>A new <see cref="Submesh"/> that represents a hemisphere line list.</returns>
		/// <remarks>
		/// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
		/// to call <see cref="GetHemisphereLines"/> to retrieve a shared <see cref="Submesh"/> instance.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="numberOfSegments"/> is less than or equal to 2.
		/// </exception>
		public static Submesh CreateHemisphereLinesSubmesh(int numberOfSegments = 32)
		{
			if (numberOfSegments < 3)
				throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

			// Create vertices for a circle on the floor.
			var vertices = new List<Vector3>();
			for (int i = 0; i < numberOfSegments; i++)
			{
				float angle = i * ConstantsF.TwoPi / numberOfSegments;
				vertices.Add(new Vector3((float)Math.Cos(angle), 0, -(float)Math.Sin(angle)));
			}

			// Top vertex of the sphere.
			var topVertexIndex = vertices.Count;
			vertices.Add(new Vector3(0, 1, 0));

			// 4 quarter arcs. Each arc starts at the base circle and ends at the top vertex. We already
			// have the first and last vertex.
			// Arc from +x to top.
			int firstArcIndex = vertices.Count;
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				float angle = (i + 1) * ConstantsF.TwoPi / numberOfSegments;
				vertices.Add(new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0));
			}

			// Arc from -z to top. (Copy from first arc.)
			int secondArcIndex = vertices.Count;
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				Vector3 p = vertices[firstArcIndex + i];
				vertices.Add(new Vector3(0, p.Y, -p.X));
			}

			// Arc from -x to top. (Copy from first arc.)
			int thirdArcIndex = vertices.Count;
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				Vector3 p = vertices[firstArcIndex + i];
				vertices.Add(new Vector3(-p.X, p.Y, 0));
			}

			// Arc from +z to top. (Copy from first arc.)
			int fourthArcIndex = vertices.Count;
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				Vector3 p = vertices[firstArcIndex + i];
				vertices.Add(new Vector3(0, p.Y, p.X));
			}

			var indices = new List<ushort>();

			// Create indices for base circle.
			for (int i = 0; i < numberOfSegments; i++)
			{
				indices.Add((ushort)i);          // Line start (= same as previous line end)
				indices.Add((ushort)(i + 1));    // Line end
			}

			// Correct last index to be 0 to close circle.
			indices[(ushort)(2 * numberOfSegments - 1)] = 0;

			// Indices for first arc.
			indices.Add(0);                             // Line start
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				indices.Add((ushort)(firstArcIndex + i));  // Line end
				indices.Add((ushort)(firstArcIndex + i));  // Line start (= same as previous line end)
			}
			indices.Add((ushort)topVertexIndex);         // Line end

			// Next arcs
			indices.Add((ushort)(numberOfSegments / 4));
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				indices.Add((ushort)(secondArcIndex + i));
				indices.Add((ushort)(secondArcIndex + i));
			}
			indices.Add((ushort)topVertexIndex);

			indices.Add((ushort)(2 * numberOfSegments / 4));
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				indices.Add((ushort)(thirdArcIndex + i));
				indices.Add((ushort)(thirdArcIndex + i));
			}
			indices.Add((ushort)topVertexIndex);

			indices.Add((ushort)(3 * numberOfSegments / 4));
			for (int i = 0; i < numberOfSegments / 4 - 1; i++)
			{
				indices.Add((ushort)(fourthArcIndex + i));
				indices.Add((ushort)(fourthArcIndex + i));
			}
			indices.Add((ushort)topVertexIndex);

			return new Submesh(vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);
		}
	}
}
