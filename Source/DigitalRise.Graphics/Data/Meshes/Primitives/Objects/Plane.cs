﻿using Microsoft.Xna.Framework;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public class Plane : BasePrimitive
	{
		private Vector2 _size = Vector2.One;
		private Point _tessellation = new Point(1, 1);
		private bool _generateBackFace;
		private NormalDirection _normalDirection;

		public Vector2 Size
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

		public Point Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				Invalidate();
			}
		}

		public bool GenerateBackface
		{
			get => _generateBackFace;

			set
			{
				if (value == _generateBackFace)
				{
					return;
				}

				_generateBackFace = value;
				Invalidate();
			}
		}

		public NormalDirection NormalDirection
		{
			get => _normalDirection;

			set
			{
				if (value == _normalDirection)
				{
					return;
				}

				_normalDirection = value;
				Invalidate();
			}
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreatePlaneMesh(Size.X, Size.Y, Tessellation.X, Tessellation.Y, UScale, VScale, GenerateBackface, IsLeftHanded, NormalDirection);

		public new Plane Clone() => (Plane)base.Clone();

		protected override BasePrimitive CreateInstanceCore() => new Plane();

		protected override void CloneCore(BasePrimitive source)
		{
			base.CloneCore(source);

			var src = (Plane)source;

			Size = src.Size;
			Tessellation = src.Tessellation;
			GenerateBackface = src.GenerateBackface;
			NormalDirection = src.NormalDirection;
		}
	}
}