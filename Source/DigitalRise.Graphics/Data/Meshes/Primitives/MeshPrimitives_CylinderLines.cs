﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Data.Meshes.Primitives
{
	partial class MeshPrimitives
	{
		/// <summary>
		/// Creates a new submesh that represents a cylinder using lines.
		/// (The cylinder is centered at the origin. Radius = 1. Height = 2 (along the y axis).) 
		/// </summary>
		/// <param name="numberOfSegments">
		/// The number of segments. This parameter controls the detail of the mesh.</param>
		/// <returns>A new <see cref="Submesh"/> that represents a cylinder line list.</returns>
		/// <remarks>
		/// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
		/// to call <see cref="GetCylinderLines"/> to retrieve a shared <see cref="Submesh"/> instance.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="numberOfSegments"/> is less than or equal to 2.
		/// </exception>
		public static Submesh CreateCylinderLinesSubmesh(int numberOfSegments = 32)
		{
			if (numberOfSegments < 3)
				throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

			var vertices = new List<Vector3>();

			// Top circle.
			for (int i = 0; i < numberOfSegments; i++)
			{
				float angle = i * ConstantsF.TwoPi / numberOfSegments;
				vertices.Add(new Vector3((float)Math.Cos(angle), 1, -(float)Math.Sin(angle)));
			}

			// Bottom circle.
			for (int i = 0; i < numberOfSegments; i++)
			{
				Vector3 p = vertices[i];
				vertices.Add(new Vector3(p.X, -1, p.Z));
			}

			var indices = new List<ushort>();

			// Top circle.
			for (int i = 0; i < numberOfSegments - 1; i++)
			{
				indices.Add((ushort)i);          // Line start (= same as previous line end)
				indices.Add((ushort)(i + 1));    // Line end
			}

			// Last line of top circle.
			indices.Add((ushort)(numberOfSegments - 1));
			indices.Add(0);

			// Bottom circle.
			for (int i = 0; i < numberOfSegments - 1; i++)
			{
				indices.Add((ushort)(numberOfSegments + i));      // Line start (= same as previous line end)
				indices.Add((ushort)(numberOfSegments + i + 1));  // Line end
			}

			// Last line of bottom circle.
			indices.Add((ushort)(numberOfSegments + numberOfSegments - 1));
			indices.Add((ushort)(numberOfSegments));

			// Side (represented by 4 lines).
			for (int i = 0; i < 4; i++)
			{
				indices.Add((ushort)(i * numberOfSegments / 4));
				indices.Add((ushort)(numberOfSegments + i * numberOfSegments / 4));
			}

			return new Submesh(vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);
		}
	}
}
