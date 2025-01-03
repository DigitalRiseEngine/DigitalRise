﻿using DigitalRise.Mathematics;
using System.ComponentModel;

namespace DigitalRise.Data.Meshes.Primitives.Objects
{
	public class Cylinder : BasePrimitive
	{
		private float _height = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 32;
		private bool _isCapped = true;

		public float Height
		{
			get => _height;

			set
			{
				if (Numeric.AreEqual(value, _height))
				{
					return;
				}

				_height = value;
				Invalidate();
			}
		}

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
				Invalidate();
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
				Invalidate();
			}
		}

		[DefaultValue(true)]
		public bool IsCapped
		{
			get => _isCapped;

			set
			{
				if (value == _isCapped)
				{
					return;
				}

				_isCapped = value;
				Invalidate();
			}	
		}

		protected override Mesh CreateMesh() => MeshPrimitives.CreateCylinderMesh(Height, Radius, Tessellation, UScale, VScale, IsCapped, IsLeftHanded);

		public new Cylinder Clone() => (Cylinder)base.Clone();

		protected override BasePrimitive CreateInstanceCore() => new Cylinder();

		protected override void CloneCore(BasePrimitive source)
		{
			base.CloneCore(source);

			var src = (Cylinder)source;

			Height = src.Height;
			Radius = src.Radius;
			Tessellation = src.Tessellation;
		}
	}
}
