// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using DigitalRise.Geometry.Meshes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace DigitalRise.Geometry.Shapes
{
	/// <summary>
	/// Represents an orthographic view volume.
	/// </summary>
	/// <remarks>
	/// The <see cref="OrthographicViewVolume"/> class is designed to model the view volume of a 
	/// orthographic camera: The observer is looking from the origin along the negative z-axis. The 
	/// x-axis points to the right and the y-axis points upwards. <see cref="ViewVolume.Near"/> and 
	/// <see cref="ViewVolume.Far"/> specify the distance from the origin (observer) to the near and 
	/// far clip planes (<see cref="ViewVolume.Near"/> &lt; <see cref="ViewVolume.Far"/>).
	/// </remarks>
	[Serializable]
	public class OrthographicViewVolume : ViewVolume
	{
		//--------------------------------------------------------------
		#region Constants
		//--------------------------------------------------------------

		/// <summary>
		/// The default value for <see cref="Projection.Near"/>.
		/// </summary>
		private const float DefaultNear = 1.0f;


		/// <summary>
		/// The default value for <see cref="Projection.Far"/>.
		/// </summary>
		private const float DefaultFar = 1000.0f;


		/// <summary>
		/// The default value for <see cref="Projection.Width"/>.
		/// </summary>
		private const float DefaultWidth = 16.0f;


		/// <summary>
		/// The default value for <see cref="Projection.Height"/>.
		/// </summary>
		private const float DefaultHeight = 9.0f;
		#endregion

		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private float _left, _right, _top, _bottom;
		private Vector3 _boxCenter;
		private readonly BoxShape _box;
		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the minimum x-value of the view volume at the near clip plane.
		/// </summary>
		/// <value>The minimum x-value of the view volume at the near clip plane.</value>
		public float Left
		{
			get => _left;

			set
			{
				if (Numeric.AreEqual(value, _left))
				{
					return;
				}

				_left = value;
				Invalidate();
			}
		}


		/// <summary>
		/// Gets or sets the maximum x-value of the view volume at the near clip plane.
		/// </summary>
		/// <value>The maximum x-value of the view volume at the near clip plane.</value>
		public float Right
		{
			get => _right;

			set
			{
				if (Numeric.AreEqual(value, _right))
				{
					return;
				}

				_right = value;
				Invalidate();
			}
		}


		/// <summary>
		/// Gets or sets the minimum y-value of the view volume at the near clip plane.
		/// </summary>
		/// <value>The minimum y-value of the view volume at the near clip plane.</value>
		public float Bottom
		{
			get => _bottom;

			set
			{
				if (Numeric.AreEqual(value, _bottom))
				{
					return;
				}

				_bottom = value;
				Invalidate();
			}
		}


		/// <summary>
		/// Gets or sets the maximum y-value of the view volume at the near clip plane.
		/// </summary>
		/// <value>The maximum y-value of the view volume at the near clip plane.</value>
		public float Top
		{
			get => _top;

			set
			{
				if (Numeric.AreEqual(value, _top))
				{
					return;
				}

				_top = value;
				Invalidate();
			}
		}

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


		/// <summary>
		/// Gets an inner point.
		/// </summary>
		/// <value>An inner point.</value>
		/// <remarks>
		/// This point is a "deep" inner point of the shape (in local space).
		/// </remarks>
		public override Vector3 InnerPoint
		{
			get
			{
				Update();
				return _boxCenter + _box.InnerPoint;
			}
		}

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class using default 
		/// settings.
		/// </summary>
		public OrthographicViewVolume() : this(DefaultWidth, DefaultHeight, DefaultNear, DefaultFar)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class as a symmetric
		/// view volume.
		/// </summary>
		/// <param name="width">The width of the view volume at the near clip plane.</param>
		/// <param name="height">The height of the view volume at the near clip plane.</param>
		/// <param name="near">The distance to the near clip plane.</param>
		/// <param name="far">The distance to the far clip plane.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="width"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="height"/> is negative.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="near"/> is negative or 0.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
		/// </exception>
		public OrthographicViewVolume(float width, float height, float near, float far)
		{
			_boxCenter = Vector3.Zero;
			_box = new BoxShape();

			SetWidthAndHeight(width, height, near, far);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="OrthographicViewVolume"/> class as an 
		/// asymmetric, off-center view volume.
		/// </summary>
		/// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
		/// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
		/// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
		/// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
		/// <param name="near">The distance to the near clip plane.</param>
		/// <param name="far">The distance to the far clip plane.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="left"/> is greater than or equal to <paramref name="right"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="near"/> is negative or 0.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
		/// </exception>
		public OrthographicViewVolume(float left, float right, float bottom, float top, float near, float far)
		{
			_boxCenter = Vector3.Zero;
			_box = new BoxShape();

			SetOffCenter(left, right, bottom, top, near, far);
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		#region ----- Cloning -----

		/// <inheritdoc/>
		protected override Shape CreateInstanceCore()
		{
			return new OrthographicViewVolume();
		}


		/// <inheritdoc/>
		protected override void CloneCore(Shape sourceShape)
		{
			var source = (OrthographicViewVolume)sourceShape;
			SetOffCenter(source.Left, source.Right, source.Bottom, source.Top, source.Near, source.Far);
		}
		#endregion


		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox(Vector3 scale, Pose pose)
		{
			Update();
			
			return _box.GetBoundingBox(scale, pose * new Pose(_boxCenter * scale));
		}


		/// <summary>
		/// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="String"/> that represents the current <see cref="Object"/>.
		/// </returns>
		public override string ToString()
		{
			return String.Format(
			  CultureInfo.InvariantCulture,
			  "OrthographicViewVolume {{ Left = {0}, Right = {1}, Bottom = {2}, Top = {3}, Near = {4}, Far = {5} }}",
			  Left, Right, Bottom, Top, Near, Far);
		}


		/// <summary>
		/// Gets a support point for a given direction.
		/// </summary>
		/// <param name="direction">
		/// The direction for which to get the support point. The vector does not need to be normalized.
		/// The result is undefined if the vector is a zero vector.
		/// </param>
		/// <returns>
		/// A support point regarding the given direction.
		/// </returns>
		/// <remarks>
		/// <para>
		/// A support point regarding a direction is an extreme point of the shape that is furthest away
		/// from the center regarding the given direction. This point is not necessarily unique.
		/// </para>
		/// </remarks>
		public override Vector3 GetSupportPoint(Vector3 direction)
		{
			Update();
			Vector3 localDirection = direction;
			Vector3 localVertex = _box.GetSupportPoint(localDirection);
			return _boxCenter + localVertex;
		}


		/// <summary>
		/// Gets a support point for a given normalized direction vector.
		/// </summary>
		/// <param name="directionNormalized">
		/// The normalized direction vector for which to get the support point.
		/// </param>
		/// <returns>
		/// A support point regarding the given direction.
		/// </returns>
		/// <remarks>
		/// A support point regarding a direction is an extreme point of the shape that is furthest away
		/// from the center regarding the given direction. This point is not necessarily unique.
		/// </remarks>
		public override Vector3 GetSupportPointNormalized(Vector3 directionNormalized)
		{
			Update();

			Vector3 localDirection = directionNormalized;
			Vector3 localVertex = _box.GetSupportPointNormalized(localDirection);
			return _boxCenter + localVertex;
		}


		/// <overloads>
		/// <summary>
		/// Gets the volume of this shape.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Gets the volume of this shape.
		/// </summary>
		/// <returns>The volume of this shape.</returns>
		public float GetVolume()
		{
			var r = Rectangle;
			return r.Width * r.Height * Depth;
		}


		/// <summary>
		/// Gets the volume of this shape.
		/// </summary>
		/// <param name="relativeError">Not used.</param>
		/// <param name="iterationLimit">Not used.</param>
		/// <returns>The volume of this shape.</returns>
		public override float GetVolume(float relativeError, int iterationLimit)
		{
			return GetVolume();
		}


		/// <summary>
		/// Called when a mesh should be generated for the shape.
		/// </summary>
		/// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
		/// <param name="iterationLimit">The iteration limit.</param>
		/// <returns>The triangle mesh for this shape.</returns>
		protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
		{
			Update();

			// Get coordinates of corners:
			float near = -Math.Min(Near, Far);
			float far = -Math.Max(Near, Far);
			float left = Math.Min(Left, Right);
			float right = Math.Max(Left, Right);
			float top = Math.Max(Top, Bottom);
			float bottom = Math.Min(Top, Bottom);

			TriangleMesh mesh = new TriangleMesh();
			// -y face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(left, bottom, near),
				Vertex1 = new Vector3(left, bottom, far),
				Vertex2 = new Vector3(right, bottom, far),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(right, bottom, far),
				Vertex1 = new Vector3(right, bottom, near),
				Vertex2 = new Vector3(left, bottom, near),
			}, true);

			// +x face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(right, top, near),
				Vertex1 = new Vector3(right, bottom, near),
				Vertex2 = new Vector3(right, bottom, far),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(right, bottom, far),
				Vertex1 = new Vector3(right, top, far),
				Vertex2 = new Vector3(right, top, near),
			}, true);

			// -z face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(right, top, far),
				Vertex1 = new Vector3(right, bottom, far),
				Vertex2 = new Vector3(left, bottom, far),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(left, bottom, far),
				Vertex1 = new Vector3(left, top, far),
				Vertex2 = new Vector3(right, top, far),
			}, true);

			// -x face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(left, top, far),
				Vertex1 = new Vector3(left, bottom, far),
				Vertex2 = new Vector3(left, bottom, near),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(left, bottom, near),
				Vertex1 = new Vector3(left, top, near),
				Vertex2 = new Vector3(left, top, far),
			}, true);

			// +z face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(left, top, near),
				Vertex1 = new Vector3(left, bottom, near),
				Vertex2 = new Vector3(right, bottom, near),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(right, bottom, near),
				Vertex1 = new Vector3(right, top, near),
				Vertex2 = new Vector3(left, top, near),
			}, true);

			// +y face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(left, top, far),
				Vertex1 = new Vector3(left, top, near),
				Vertex2 = new Vector3(right, top, near),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(right, top, near),
				Vertex1 = new Vector3(right, top, far),
				Vertex2 = new Vector3(left, top, far),
			}, true);

			return mesh;
		}

		/// <overloads>
		/// <summary>
		/// Sets the width and height of the view volume to the specified values.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Sets the width and height of the view volume to the specified size and depth.
		/// </summary>
		/// <param name="width">The width of the view volume at the near clip plane.</param>
		/// <param name="height">The height of the view volume at the near clip plane.</param>
		/// <param name="near">The distance to the near clip plane.</param>
		/// <param name="far">The distance to the far clip plane.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
		/// </exception>
		public void SetWidthAndHeight(float width, float height, float near, float far)
		{
			if (near >= far)
				throw new ArgumentException("The near plane distance of a view volume needs to be less than the far plane distance.");

			Near = near;
			Far = far;
			SetWidthAndHeight(width, height);
		}


		/// <summary>
		/// Sets the width and height of the view volume to the specified size.
		/// </summary>
		/// <param name="width">The width of the view volume at the near clip plane.</param>
		/// <param name="height">The height of the view volume at the near clip plane.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
		/// </exception>
		public void SetWidthAndHeight(float width, float height)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException("width", "The width of the view volume must be greater than 0.");
			if (height <= 0)
				throw new ArgumentOutOfRangeException("height", "The height of the view volume must be greater than 0.");

			float halfWidth = width / 2.0f;
			float halfHeight = height / 2.0f;
			Left = -halfWidth;
			Right = halfWidth;
			Bottom = -halfHeight;
			Top = halfHeight;
		}


		/// <overloads>
		/// <summary>
		/// Sets the dimensions of the view volume.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Sets the dimensions of the view volume (including depths).
		/// </summary>
		/// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
		/// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
		/// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
		/// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
		/// <param name="near">The distance to the near clip plane.</param>
		/// <param name="far">The distance to the far clip plane.</param>
		/// <remarks>
		/// This method can be used to define an asymmetric, off-center view volume.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// <paramref name="left"/> is greater than or equal to <paramref name="right"/>, 
		/// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>, or
		/// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
		/// </exception>
		public void SetOffCenter(float left, float right, float bottom, float top, float near, float far)
		{
			if (near >= far)
				throw new ArgumentException("The near plane distance of a view volume needs to be less than the far plane distance (near < far).");

			Near = near;
			Far = far;
			SetOffCenter(left, right, bottom, top);
		}


		/// <summary>
		/// Sets the dimensions of the view volume.
		/// </summary>
		/// <param name="left">The minimum x-value of the view volume at the near clip plane.</param>
		/// <param name="right">The maximum x-value of the view volume at the near clip plane.</param>
		/// <param name="bottom">The minimum y-value of the view volume at the near clip plane.</param>
		/// <param name="top">The maximum y-value of the view volume at the near clip plane.</param>
		/// <remarks>
		/// This method can be used to define an asymmetric, off-center view volume.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// <paramref name="left"/> is greater than or equal to <paramref name="right"/>, or
		/// <paramref name="bottom"/> is greater than or equal to <paramref name="top"/>.
		/// </exception>
		public void SetOffCenter(float left, float right, float bottom, float top)
		{
			if (left >= right)
				throw new ArgumentException("Left needs to be less than right (left < right).");
			if (bottom >= top)
				throw new ArgumentException("Bottom needs to be less than top (bottom < top).");

			Left = left;
			Right = right;
			Bottom = bottom;
			Top = top;
		}


		protected override void InternalUpdate(out ProjectionRectangle rectangle, out Matrix44F projection)
		{
			// Sort left and right.
			float left, right;
			if (Left <= Right)
			{
				left = Left;
				right = Right;
			}
			else
			{
				left = Right;
				right = Left;
			}

			// Sort bottom and top.
			float bottom, top;
			if (Bottom <= Top)
			{
				bottom = Bottom;
				top = Top;
			}
			else
			{
				bottom = Top;
				top = Bottom;
			}

			// Sort near and far.
			float near, far;
			if (Near <= Far)
			{
				near = Near;
				far = Far;
			}
			else
			{
				near = Far;
				far = Near;
			}

			// Update shape.
			float width = right - left;
			float height = top - bottom;
			float depth = far - near;

			_box.WidthX = width;
			_box.WidthY = height;
			_box.WidthZ = depth;
			float centerX = left + width / 2.0f;
			float centerY = bottom + height / 2.0f;
			float centerZ = -(near + depth / 2.0f);
			_boxCenter = new Vector3(centerX, centerY, centerZ);

			rectangle.Left = left;
			rectangle.Top = top;
			rectangle.Right = right;
			rectangle.Bottom = bottom;

			projection = Matrix44F.CreateOrthographicOffCenter(Left, Right, Bottom, Top, Near, Far);
		}

		#endregion
	}
}
