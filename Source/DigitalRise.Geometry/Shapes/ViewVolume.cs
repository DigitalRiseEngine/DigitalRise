// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Newtonsoft.Json;
using System;
using System.ComponentModel;


namespace DigitalRise.Geometry.Shapes
{
	public struct ProjectionRectangle
	{
		public float Left, Right, Top, Bottom;

		/// <summary>
		/// Gets the width of the view volume at the near clip plane.
		/// </summary>
		/// <value>The width of the view volume at the near clip plane.</value>
		[Browsable(false)]
		[JsonIgnore]
		public float Width
		{
			get { return Math.Abs(Right - Left); }
		}


		/// <summary>
		/// Gets the height of the view volume at the near clip plane.
		/// </summary>
		/// <value>The height of the view volume at the near clip plane.</value>
		[Browsable(false)]
		[JsonIgnore]
		public float Height
		{
			get { return Math.Abs(Top - Bottom); }
		}

		/// <summary>
		/// Gets the aspect ratio (width / height).
		/// </summary>
		/// <value>The aspect ratio (<see cref="Width"/> / <see cref="Height"/>).</value>
		[Browsable(false)]
		[JsonIgnore]
		public float AspectRatio
		{
			get { return Width / Height; }
		}
	}

	/// <summary>
	/// Represents a view volume (base implementation).
	/// </summary>
	/// <para>
	/// The <see cref="ViewVolume"/> class is designed to model the view volume of a camera: The 
	/// observer is looking from the origin along the negative z-axis. The x-axis points to the right 
	/// and the y-axis points upwards. <see cref="ViewVolume.Near"/> and <see cref="ViewVolume.Far"/> 
	/// specify the distance from the origin (observer) to the near and far clip planes 
	/// (<see cref="ViewVolume.Near"/> &lt; <see cref="ViewVolume.Far"/>).
	/// </para>
	[Serializable]
	public abstract class ViewVolume : ConvexShape
	{
		#region Fields

		private float _far, _near;
		private ProjectionRectangle _rectangle;
		private Matrix44F _projection;
		private bool _dirty = true;

		#endregion

		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the distance to the near clip plane. 
		/// </summary>
		/// <value>The distance to the near clip plane.</value>
		public virtual float Near
		{
			get { return _near; }
			set
			{
				if (Numeric.AreEqual(value, _near))
				{
					return;
				}

				_near = value;
				Invalidate();
			}
		}


		/// <summary>
		/// Gets or sets the distance to the far clip plane. 
		/// </summary>
		/// <value>The distance to the far clip plane.</value>
		public virtual float Far
		{
			get { return _far; }
			set
			{
				if (Numeric.AreEqual(value, _far))
				{
					return;
				}

				_far = value;
				Invalidate();
			}
		}


		/// <summary>
		/// Gets the depth of the view volume (= <see cref="Far"/> - <see cref="Near"/>).
		/// </summary>
		/// <value>The depth of the view volume (= <see cref="Far"/> - <see cref="Near"/>).</value>
		[Browsable(false)]
		[JsonIgnore]
		public float Depth
		{
			get { return Math.Abs(Far - Near); }
		}


		[Browsable(false)]
		[JsonIgnore]
		public ProjectionRectangle Rectangle
		{
			get
			{
				Update();

				return _rectangle;
			}
		}


		[Browsable(false)]
		[JsonIgnore]
		public Matrix44F Projection
		{
			get
			{
				Update();

				return _projection;
			}
		}

		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		protected void Update()
		{
			if (!_dirty)
			{
				return;
			}

			InternalUpdate(out _rectangle, out _projection);

			_dirty = false;
		}

		protected void Invalidate()
		{
			OnChanged(ShapeChangedEventArgs.Empty);
			_dirty = true;
		}


		/// <summary>
		/// Updates the shape.
		/// </summary>
		protected abstract void InternalUpdate(out ProjectionRectangle rectangle, out Matrix44F projection);
		#endregion
	}
}
