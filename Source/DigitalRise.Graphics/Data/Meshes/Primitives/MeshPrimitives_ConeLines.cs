// DigitalRune Engine - Copyright (C) DigitalRune GmbH
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
		/// Creates a new submesh that represents a cone using lines.
		/// (The cone is standing on the xz plane pointing along the y axis. Radius = 1. Height = 1.) 
		/// </summary>
		/// <param name="numberOfSegments">
		/// The number of segments. This parameter controls the detail of the mesh.</param>
		/// <returns>A new <see cref="Submesh"/> that represents a cone line list.</returns>
		/// <remarks>
		/// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
		/// to call <see cref="GetConeLines"/> to retrieve a shared <see cref="Submesh"/> instance.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="numberOfSegments"/> is less than or equal to 2.
		/// </exception>
		public static Submesh CreateConeLinesSubmesh(int numberOfSegments = 32)
		{
			if (numberOfSegments < 3)
				throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

			var vertices = new List<Vector3>();

			// Base
			for (int i = 0; i < numberOfSegments; i++)
			{
				float angle = i * ConstantsF.TwoPi / numberOfSegments;
				float x = (float)Math.Cos(angle);
				float z = -(float)Math.Sin(angle);
				vertices.Add(new Vector3(x, 0, z));
			}

			// Tip
			vertices.Add(new Vector3(0, 1, 0));

			var indices = new List<ushort>();

			// Base circle.
			for (int i = 0; i < numberOfSegments - 1; i++)
			{
				indices.Add((ushort)i);          // Line start (= same as previous line end)
				indices.Add((ushort)(i + 1));    // Line end
			}

			// Last line of base circle.
			indices.Add((ushort)(numberOfSegments - 1));
			indices.Add(0);

			// Side represented by 4 lines.
			for (int i = 0; i < 4; i++)
			{
				indices.Add((ushort)(i * numberOfSegments / 4));
				indices.Add((ushort)numberOfSegments);
			}

			return new Submesh(vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);
		}
	}
}
