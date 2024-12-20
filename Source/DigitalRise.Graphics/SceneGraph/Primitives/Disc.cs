﻿using DigitalRise.Attributes;
using DigitalRise.Mathematics;
using DigitalRise.Data.Meshes.Primitives;
using DigitalRise.Data.Meshes;

namespace DigitalRise.SceneGraph.Primitives
{
	/// <summary>
	/// A disc - a circular base, or a circular sector.
	/// </summary>
	[EditorInfo("Primitive")]
	public class Disc : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private float _sectorAngle = 360;
		private int _tessellation = 16;

		public float Radius
		{
			get => _radius;

			set
			{
				if (Numeric.AreEqual(value, _radius))
				{
					return;
				}

				_radius = value;
				InvalidateMesh();
			}
		}

		public float SectorAngle
		{
			get => _sectorAngle;

			set
			{
				if (Numeric.AreEqual(value, _sectorAngle))
				{
					return;
				}

				_sectorAngle = value;
				InvalidateMesh();
			}
		}

		public int Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreateDiscMesh(Radius, 
			MathHelper.ToRadians(SectorAngle),
			Tessellation, UScale, VScale, IsLeftHanded);

		public new Disc Clone() => (Disc)base.Clone();

		protected override SceneNode CreateInstanceCore() => new Disc();

		protected override void CloneCore(SceneNode source)
		{
			base.CloneCore(source);

			var src = (Disc)source;

			Radius = src.Radius;
			SectorAngle = src.SectorAngle;
			Tessellation = src.Tessellation;
		}
	}
}
