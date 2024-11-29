// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Data.Meshes.Primitives
{
	partial class MeshPrimitives
	{
		/// <summary>
		/// Creates a new submesh that represents a circle using lines.
		/// (The circle lies in the xy plane and is centered at the origin. Radius = 1.)
		/// </summary>
		/// <param name="numberOfSegments">
		/// The number of segments. This parameter controls the detail of the mesh.</param>
		/// <returns>A new <see cref="Submesh"/> that represents a circle line list.</returns>
		/// <remarks>
		/// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
		/// to call <see cref="GetCircleLines"/> to retrieve a shared <see cref="Submesh"/> instance.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="numberOfSegments"/> is less than or equal to 2.
		/// </exception>
		public static Submesh CreateCircleLinesSubmesh(int numberOfSegments = 32)
		{
			if (numberOfSegments < 3)
				throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");


			// Create vertices for a circle on the floor.
			var vertices = new Vector3[numberOfSegments];
			for (int i = 0; i < numberOfSegments; i++)
			{
				float angle = i * ConstantsF.TwoPi / numberOfSegments;

				float x = (float)Math.Cos(angle);
				float y = (float)Math.Sin(angle);
				vertices[i] = new Vector3(x, y, 0);
			}

			// Create indices for base circle.
			var indices = new ushort[2 * numberOfSegments];
			for (int i = 0; i < numberOfSegments; i++)
			{
				indices[2 * i] = (ushort)i;
				indices[2 * i + 1] = (ushort)(i + 1);
			}

			// Correct last index to be 0 to close circle.
			indices[2 * numberOfSegments - 1] = 0;

			return new Submesh(vertices, indices, PrimitiveType.LineList);
		}
	}
}
