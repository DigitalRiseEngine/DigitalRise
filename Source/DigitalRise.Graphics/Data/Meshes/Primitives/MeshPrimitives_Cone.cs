﻿// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using DigitalRise.Data.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DigitalRise.Data.Meshes.Primitives
{
	partial class MeshPrimitives
	{
		private static MeshBuilder CreateConeMeshBuilder(float radius, float height, int tessellation, float uScale, float vScale)
		{
			if (tessellation < 3)
				tessellation = 3;

			var builder = new MeshBuilder();
			var numberOfSections = tessellation + 1;
			var vertexNumberBySection = tessellation + 1;

			// e == 0 => Cone 
			// e == 1 => Cap
			for (var e = 0; e < 2; e++)
			{
				var topHeight = e == 0 ? height : 0;
				var normalSign = Math.Sign(0.5 - e);
				var slopeLength = Math.Sqrt(radius * radius + topHeight * topHeight);
				var slopeCos = topHeight / slopeLength;
				var slopeSin = radius / slopeLength;

				// the cone sections
				for (var j = 0; j < tessellation; ++j)
				{
					var sectionRatio = j / (float)tessellation;
					var sectionHeight = (sectionRatio * topHeight) - height * 0.5f;
					var sectionRadius = (1 - sectionRatio) * radius;

					for (var i = 0; i <= tessellation; ++i)
					{
						var angle = i / (double)tessellation * 2.0 * Math.PI;
						var textureCoordinate = new Vector2((float)i / tessellation, 1 - sectionRatio);
						textureCoordinate.X *= uScale;
						textureCoordinate.Y *= vScale;
						var position = new Vector3((float)Math.Cos(angle) * sectionRadius, sectionHeight, (float)Math.Sin(angle) * sectionRadius);
						var normal = normalSign * new Vector3((float)(Math.Cos(angle) * slopeCos), (float)slopeSin, (float)(Math.Sin(angle) * slopeCos));

						builder.AddVertex(new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate });
					}
				}

				// the extremity points
				for (var i = 0; i <= tessellation; ++i)
				{
					var position = new Vector3(0, topHeight - height * 0.5f, 0);
					var angle = (i + 0.5) / tessellation * 2.0 * Math.PI;
					var textureCoordinate = new Vector2((i + 0.5f) / tessellation, 0);
					textureCoordinate.X *= uScale;
					textureCoordinate.Y *= vScale;
					var normal = normalSign * new Vector3((float)(Math.Cos(angle) * slopeCos), (float)slopeSin, (float)(Math.Sin(angle) * slopeCos));

					builder.AddVertex(new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate });
				}
			}

			// the indices
			for (var e = 0; e < 2; e++)
			{
				var globalOffset = (e == 0) ? 0 : vertexNumberBySection * numberOfSections;
				var offsetV1 = (e == 0) ? 1 : vertexNumberBySection;
				var offsetV2 = (e == 0) ? vertexNumberBySection : 1;
				var offsetV3 = (e == 0) ? 1 : vertexNumberBySection + 1;
				var offsetV4 = (e == 0) ? vertexNumberBySection + 1 : 1;

				// the sections
				for (var j = 0; j < tessellation - 1; ++j)
				{
					for (int i = 0; i < tessellation; ++i)
					{
						builder.AddIndex(globalOffset + j * vertexNumberBySection + i);
						builder.AddIndex(globalOffset + j * vertexNumberBySection + i + offsetV1);
						builder.AddIndex(globalOffset + j * vertexNumberBySection + i + offsetV2);

						builder.AddIndex(globalOffset + j * vertexNumberBySection + i + vertexNumberBySection);
						builder.AddIndex(globalOffset + j * vertexNumberBySection + i + offsetV3);
						builder.AddIndex(globalOffset + j * vertexNumberBySection + i + offsetV4);
					}
				}

				// the extremity triangle
				for (int i = 0; i < tessellation; ++i)
				{
					builder.AddIndex(globalOffset + (tessellation - 1) * vertexNumberBySection + i);
					builder.AddIndex(globalOffset + (tessellation - 1) * vertexNumberBySection + i + offsetV1);
					builder.AddIndex(globalOffset + (tessellation - 1) * vertexNumberBySection + i + offsetV2);
				}
			}

			return builder;
		}

		/// <summary>
		/// Creates a cone a circular base and a rolled face.
		/// </summary>
		/// <param name="radius">The radius or the base</param>
		/// <param name="height">The height of the cone</param>
		/// <param name="tessellation">The number of segments composing the base</param>
		/// <param name="uScale">Scale U coordinates between 0 and the values of this parameter.</param>
		/// <param name="vScale">Scale V coordinates 0 and the values of this parameter.</param>
		/// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
		/// <returns>A cone.</returns>
		public static Submesh CreateConeSubmesh(float radius = 0.5f, float height = 1.0f, int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
		{
			var builder = CreateConeMeshBuilder(radius, height, tessellation, uScale, vScale);

			return builder.CreateSubmesh(toLeftHanded);
		}


		/// <summary>
		/// Creates a cone a circular base and a rolled face.
		/// </summary>
		/// <param name="radius">The radius or the base</param>
		/// <param name="height">The height of the cone</param>
		/// <param name="tessellation">The number of segments composing the base</param>
		/// <param name="uScale">Scale U coordinates between 0 and the values of this parameter.</param>
		/// <param name="vScale">Scale V coordinates 0 and the values of this parameter.</param>
		/// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
		/// <returns>A cone.</returns>
		public static Mesh CreateConeMesh(float radius = 0.5f, float height = 1.0f, int tessellation = 16, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
		{
			var builder = CreateConeMeshBuilder(radius, height, tessellation, uScale, vScale);

			return builder.CreateMesh(toLeftHanded);
		}
	}
}
