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
	/// Represents a perspective view volume (frustum).
	/// </summary>
	/// <remarks>
	/// <para>
	/// A perspective view volume is a frustum. A frustum is a portion of a pyramid that lies between 
	/// two cutting planes.
	/// </para>
	/// <para>
	/// The <see cref="PerspectiveViewVolume"/> class is designed to model the view volume of a 
	/// perspective camera: The observer is looking from the origin along the negative z-axis. The 
	/// x-axis points to the right and the y-axis points upwards. <see cref="ViewVolume.Near"/> and 
	/// <see cref="ViewVolume.Far"/> are positive values that specify the distance from the origin 
	/// (observer) to the near and far clip planes 
	/// (<see cref="ViewVolume.Near"/> ≤ <see cref="ViewVolume.Far"/>).
	/// </para>
	/// </remarks>
	[Serializable]
	public class PerspectiveViewVolume : ViewVolume
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
		/// The default value for <see cref="Projection.AspectRatio"/>.
		/// </summary>
		private const float DefaultAspectRatio = 16.0f / 9.0f;


		/// <summary>
		/// The default value for <see cref="Projection.FieldOfViewY"/>.
		/// </summary>
		private const float DefaultFieldOfViewY = 60.0f; // 60°
		#endregion

		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		private float _fieldOfViewY, _aspectRatio;
		private Vector3 _innerPoint;
		private Plane? _nearClipPlane;

		// Cached vertices.
		private Vector3 _nearBottomLeftVertex;
		private Vector3 _nearBottomRightVertex;
		private Vector3 _nearTopLeftVertex;
		private Vector3 _nearTopRightVertex;
		private Vector3 _farBottomLeftVertex;
		private Vector3 _farBottomRightVertex;
		private Vector3 _farTopLeftVertex;
		private Vector3 _farTopRightVertex;
		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

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
				return _innerPoint;
			}
		}


		/// <summary>
		/// Gets or sets the vertical field of view in degrees.
		/// </summary>
		/// <value>The vertical field of view in degrees.</value>
		public float FieldOfViewY
		{
			get => _fieldOfViewY;

			set
			{
				if (Numeric.AreEqual(value, _fieldOfViewY))
				{
					return;
				}

				_fieldOfViewY = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the aspect ratio (width / height).
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public float AspectRatio
		{
			get => _aspectRatio;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "The aspect ratio must not be negative or 0.");
				}

				if (Numeric.AreEqual(value, _aspectRatio))
				{
					return;
				}

				_aspectRatio = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the near clip plane in view space.
		/// </summary>
		/// <value>
		/// The near clip plane in view space. The plane normal must point to the viewer.
		/// </value>
		/// <remarks>
		/// <para>
		/// When rendering mirrors or portals, the objects before the mirror or portal should not be
		/// rendered. This could be solved using clip planes, but these clip planes need to be supported
		/// by all shaders. Alternatively, we can also solve this problem by creating a view frustum
		/// where the near plane is parallel to the clip plane - such frustums are called oblique view
		/// frustums because the near plane (and also the far plane) are tilted compared to standard
		/// view frustums.
		/// </para>
		/// <para>
		/// Use the property <see cref="NearClipPlane"/> to set a clip plane for the near view-plane.
		/// Setting a near clip plane changes the projection matrix. However, it does not affect the
		/// shape (see <see cref="Projection.ViewVolume"/>) of the <see cref="Projection"/>!
		/// </para>
		/// <para>
		/// For general information about oblique view frustums, see
		/// <see href="http://www.terathon.com/code/oblique.html" />.
		/// </para>
		/// </remarks>
		[Browsable(false)]
		[JsonIgnore]
		public Plane? NearClipPlane
		{
			get => _nearClipPlane;

			set
			{
				if (value == _nearClipPlane)
				{
					return;
				}

				_nearClipPlane = value;
				Invalidate();
			}
		}

		public override float Near
		{
			get => base.Near;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("near", "The near plane distance of a perspective view volume needs to be greater than 0.");
				}

				base.Near = value;
			}
		}

		public override float Far
		{
			get => base.Far;

			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException("far", "The far plane distance of a perspective view volume needs to be greater than 0.");
				}

				base.Far = value;
			}
		}

		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="PerspectiveViewVolume"/> class.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Initializes a new instance of the <see cref="PerspectiveViewVolume"/> class using default 
		/// settings.
		/// </summary>
		public PerspectiveViewVolume() : this(DefaultFieldOfViewY, DefaultAspectRatio, DefaultNear, DefaultFar)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="PerspectiveViewVolume"/> class with the given
		/// field of view and depth.
		/// </summary>
		/// <param name="fieldOfViewY">The vertical field of view.</param>
		/// <param name="aspectRatio">The aspect ratio (width / height).</param>
		/// <param name="near">The distance to the near clip plane.</param>
		/// <param name="far">The distance to the far clip plane.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="aspectRatio"/> is negative or 0, <paramref name="near"/> is negative or 0,
		/// or <paramref name="far"/> is negative or 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
		/// </exception>
		public PerspectiveViewVolume(float fieldOfViewY, float aspectRatio, float near, float far)
		{
			SetFieldOfView(fieldOfViewY, aspectRatio, near, far);
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		#region ----- Cloning -----

		/// <inheritdoc/>
		protected override Shape CreateInstanceCore()
		{
			return new PerspectiveViewVolume();
		}


		/// <inheritdoc/>
		protected override void CloneCore(Shape sourceShape)
		{
			var source = (PerspectiveViewVolume)sourceShape;
			SetFieldOfView(source.FieldOfViewY, source.AspectRatio, source.Near, source.Far);
		}

		#endregion


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

			if (direction.X > 0)
			{
				// Get a right vertex.

				if (direction.Y > 0)
				{
					// Get a top right vertex.
					if (Vector3.Dot(_nearTopRightVertex, direction) > Vector3.Dot(_farTopRightVertex, direction))
						return _nearTopRightVertex;
					else
						return _farTopRightVertex;
				}
				else
				{
					// Get a bottom right vertex;
					if (Vector3.Dot(_nearBottomRightVertex, direction) > Vector3.Dot(_farBottomRightVertex, direction))
						return _nearBottomRightVertex;
					else
						return _farBottomRightVertex;
				}
			}
			else
			{
				// Get a left vertex.

				if (direction.Y > 0)
				{
					// Get a top left vertex.
					if (Vector3.Dot(_nearTopLeftVertex, direction) > Vector3.Dot(_farTopLeftVertex, direction))
						return _nearTopLeftVertex;
					else
						return _farTopLeftVertex;
				}
				else
				{
					// Get a bottom left vertex;
					if (Vector3.Dot(_nearBottomLeftVertex, direction) > Vector3.Dot(_farBottomLeftVertex, direction))
						return _nearBottomLeftVertex;
					else
						return _farBottomLeftVertex;
				}
			}
		}



		/// <summary>
		/// Gets a support point for a given normalized direction vector.
		/// </summary>
		/// <param name="directionNormalized">
		/// The normalized direction vector for which to get the support point.
		/// </param>
		/// <returns>A support point regarding the given direction.</returns>
		/// <remarks>
		/// A support point regarding a direction is an extreme point of the shape that is furthest away
		/// from the center regarding the given direction. This point is not necessarily unique.
		/// </remarks>
		public override Vector3 GetSupportPointNormalized(Vector3 directionNormalized)
		{
			Update();

			if (directionNormalized.X > 0)
			{
				// Get a right vertex.

				if (directionNormalized.Y > 0)
				{
					// Get a top right vertex.
					if (Vector3.Dot(_nearTopRightVertex, directionNormalized) > Vector3.Dot(_farTopRightVertex, directionNormalized))
						return _nearTopRightVertex;
					else
						return _farTopRightVertex;
				}
				else
				{
					// Get a bottom right vertex;
					if (Vector3.Dot(_nearBottomRightVertex, directionNormalized) > Vector3.Dot(_farBottomRightVertex, directionNormalized))
						return _nearBottomRightVertex;
					else
						return _farBottomRightVertex;
				}
			}
			else
			{
				// Get a left vertex.

				if (directionNormalized.Y > 0)
				{
					// Get a top left vertex.
					if (Vector3.Dot(_nearTopLeftVertex, directionNormalized) > Vector3.Dot(_farTopLeftVertex, directionNormalized))
						return _nearTopLeftVertex;
					else
						return _farTopLeftVertex;
				}
				else
				{
					// Get a bottom left vertex;
					if (Vector3.Dot(_nearBottomLeftVertex, directionNormalized) > Vector3.Dot(_farBottomLeftVertex, directionNormalized))
						return _nearBottomLeftVertex;
					else
						return _farBottomLeftVertex;
				}
			}
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
			float nearArea = r.Width * r.Height;
			float scale = Far / Near;
			float farArea = nearArea * scale * scale;

			// Volume is total pyramid minus the pyramid before the near plane.
			return 1.0f / 3.0f * (farArea * Far - nearArea * Near);
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
			var r = Rectangle;

			// Get coordinates of corners:
			float near = -Math.Min(Near, Far);
			float far = -Math.Max(Near, Far);
			float leftNear = Math.Min(r.Left, r.Right);
			float rightNear = Math.Max(r.Left, r.Right);
			float topNear = Math.Max(r.Top, r.Bottom);
			float bottomNear = Math.Min(r.Top, r.Bottom);
			float farFactor = 1 / near * far;    // Multiply near-values by this factor to get far-values.
			float leftFar = leftNear * farFactor;
			float rightFar = rightNear * farFactor;
			float topFar = topNear * farFactor;
			float bottomFar = bottomNear * farFactor;

			TriangleMesh mesh = new TriangleMesh();

			// -y face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(leftNear, bottomNear, near),
				Vertex1 = new Vector3(leftFar, bottomFar, far),
				Vertex2 = new Vector3(rightFar, bottomFar, far),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(rightFar, bottomFar, far),
				Vertex1 = new Vector3(rightNear, bottomNear, near),
				Vertex2 = new Vector3(leftNear, bottomNear, near),
			}, true);

			// +x face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(rightNear, topNear, near),
				Vertex1 = new Vector3(rightNear, bottomNear, near),
				Vertex2 = new Vector3(rightFar, bottomFar, far),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(rightFar, bottomFar, far),
				Vertex1 = new Vector3(rightFar, topFar, far),
				Vertex2 = new Vector3(rightNear, topNear, near),
			}, true);

			// -z face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(rightFar, topFar, far),
				Vertex1 = new Vector3(rightFar, bottomFar, far),
				Vertex2 = new Vector3(leftFar, bottomFar, far),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(leftFar, bottomFar, far),
				Vertex1 = new Vector3(leftFar, topFar, far),
				Vertex2 = new Vector3(rightFar, topFar, far),
			}, true);

			// -x face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(leftFar, topFar, far),
				Vertex1 = new Vector3(leftFar, bottomFar, far),
				Vertex2 = new Vector3(leftNear, bottomNear, near),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(leftNear, bottomNear, near),
				Vertex1 = new Vector3(leftNear, topNear, near),
				Vertex2 = new Vector3(leftFar, topFar, far),
			}, true);

			// +z face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(leftNear, topNear, near),
				Vertex1 = new Vector3(leftNear, bottomNear, near),
				Vertex2 = new Vector3(rightNear, bottomNear, near),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(rightNear, bottomNear, near),
				Vertex1 = new Vector3(rightNear, topNear, near),
				Vertex2 = new Vector3(leftNear, topNear, near),
			}, true);

			// +y face
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(leftFar, topFar, far),
				Vertex1 = new Vector3(leftNear, topNear, near),
				Vertex2 = new Vector3(rightNear, topNear, near),
			}, true);
			mesh.Add(new Triangle
			{
				Vertex0 = new Vector3(rightNear, topNear, near),
				Vertex1 = new Vector3(rightFar, topFar, far),
				Vertex2 = new Vector3(leftFar, topFar, far),
			}, true);

			return mesh;
		}

		protected override void InternalUpdate(out ProjectionRectangle rectangle, out Matrix44F projection)
		{
			// Sort near and far.
			float near = Near;
			float far = Far;
			if (near <= 0)
				throw new ArgumentOutOfRangeException("near", "The near plane distance of a perspective view volume needs to be greater than 0.");
			if (far <= 0)
				throw new ArgumentOutOfRangeException("far", "The far plane distance of a perspective view volume needs to be greater than 0.");
			if (near > far)
				Mathematics.MathHelper.Swap(ref near, ref far);

			float width, height;
			GetWidthAndHeight(_fieldOfViewY, _aspectRatio, Near, out width, out height);

			float halfWidth = width / 2.0f;
			float halfHeight = height / 2.0f;
			var left = -halfWidth;
			var right = halfWidth;
			var bottom = -halfHeight;
			var top = halfHeight;

			rectangle.Left = left;
			rectangle.Top = top;
			rectangle.Right = right;
			rectangle.Bottom = bottom;

			projection = Matrix44F.CreatePerspectiveOffCenter(left, right, bottom, top, Near, Far);
			if (NearClipPlane.HasValue)
			{
				Vector4 clipPlane = new Vector4(NearClipPlane.Value.Normal, -NearClipPlane.Value.DistanceFromOrigin);

				// Calculate the clip-space corner point opposite the clipping plane as
				// (-sign(clipPlane.x), -sign(clipPlane.y), 1, 1) and transform it into
				// camera space by multiplying it by the inverse of the projection matrix.
				Vector4 q;
				q.X = (-Math.Sign(clipPlane.X) + projection.M02) / projection.M00;
				q.Y = (-Math.Sign(clipPlane.Y) + projection.M12) / projection.M11;
				q.Z = -1.0f;
				q.W = (1.0f + projection.M22) / projection.M23;

				// Calculate the scaled plane vector
				Vector4 c = clipPlane * (1.0f / Vector4.Dot(clipPlane, q));

				// Replace the third row of the projection matrix
				projection.M20 = c.X;
				projection.M21 = c.Y;
				projection.M22 = c.Z;
				projection.M23 = c.W;
			}


			// Update near view rectangle.
			_nearBottomLeftVertex = new Vector3(left, bottom, -near);
			_nearBottomRightVertex = new Vector3(right, bottom, -near);
			_nearTopLeftVertex = new Vector3(left, top, -near);
			_nearTopRightVertex = new Vector3(right, top, -near);

			// Update far view rectangle.
			float factor = far / near;
			left = left * factor;
			right = right * factor;
			bottom = bottom * factor;
			top = top * factor;

			_farBottomLeftVertex = new Vector3(left, bottom, -far);
			_farBottomRightVertex = new Vector3(right, bottom, -far);
			_farTopLeftVertex = new Vector3(left, top, -far);
			_farTopRightVertex = new Vector3(right, top, -far);

			_innerPoint = new Vector3(left + right, bottom + top, -near - far) * 0.5f;
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
				"PerspectiveViewVolume {{ FieldOfViewY = {0}, AspectRatio = {1}, Near = {2}, Far = {3} }}",
				FieldOfViewY, AspectRatio, Near, Far);
		}


		/// <overloads>
		/// <summary>
		/// Sets the dimensions of the frustum to the specified field of view.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Sets the dimensions of the frustum to the specified field of view and near/far values.
		/// </summary>
		/// <param name="fieldOfViewY">The vertical field of view.</param>
		/// <param name="aspectRatio">The aspect ratio (width / height).</param>
		/// <param name="near">The distance to the near clip plane.</param>
		/// <param name="far">The distance to the far clip plane.</param>
		/// <remarks>
		/// This method creates a symmetric frustum.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="aspectRatio"/> is negative or 0, <paramref name="near"/> is negative or 0,
		/// or <paramref name="far"/> is negative or 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="near"/> is greater than or equal to <paramref name="far"/>.
		/// </exception>
		public void SetFieldOfView(float fieldOfViewY, float aspectRatio, float near, float far)
		{
			if (near >= far)
				throw new ArgumentException("The near plane distance of a frustum needs to be less than the far plane distance (near < far).");

			FieldOfViewY = fieldOfViewY;
			AspectRatio = aspectRatio;
			Near = near;
			Far = far;
		}


		/// <summary>
		/// Sets the dimensions of the frustum to the specified field of view.
		/// </summary>
		/// <param name="fieldOfViewY">The vertical field of view.</param>
		/// <param name="aspectRatio">The aspect ratio (width / height).</param>
		/// <remarks>
		/// This method creates a symmetric frustum.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="aspectRatio"/> is negative or 0.
		/// </exception>
		public void SetFieldOfView(float fieldOfViewY, float aspectRatio)
		{
			FieldOfViewY = fieldOfViewY;
			AspectRatio = aspectRatio;
		}


		/// <summary>
		/// Converts the vertical field of view of a symmetric frustum to a horizontal field of view.
		/// </summary>
		/// <param name="fieldOfViewY">The vertical field of view in radians.</param>
		/// <param name="aspectRatio">The aspect ratio (width / height).</param>
		/// <returns>The horizontal field of view in radians.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="aspectRatio"/> is negative or 0.
		/// </exception>
		public static float GetFieldOfViewX(float fieldOfViewY, float aspectRatio)
		{
			if (aspectRatio <= 0)
				throw new ArgumentOutOfRangeException("aspectRatio", "The aspect ratio must not be negative or 0.");

			float height = 2.0f * (float)Math.Tan(fieldOfViewY / 2.0f);
			float width = height * aspectRatio;
			float horizontalFieldOfView = 2.0f * (float)Math.Atan(width / 2.0f);
			return horizontalFieldOfView;
		}


		/// <summary>
		/// Converts a horizontal field of view of a symmetric frustum to a vertical field of view.
		/// </summary>
		/// <param name="fieldOfViewX">The horizontal field of view in radians.</param>
		/// <param name="aspectRatio">The aspect ratio.</param>
		/// <returns>The vertical field of view in radians.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="aspectRatio"/> is negative or 0.
		/// </exception>
		public static float GetFieldOfViewY(float fieldOfViewX, float aspectRatio)
		{
			if (aspectRatio <= 0)
				throw new ArgumentOutOfRangeException("aspectRatio", "The aspect ratio must not be negative or 0.");

			float width = 2.0f * (float)Math.Tan(fieldOfViewX / 2.0f);
			float height = width / aspectRatio;
			float verticalFieldOfView = 2.0f * (float)Math.Atan(height / 2.0f);
			return verticalFieldOfView;
		}


		/// <summary>
		/// Gets the extent of the frustum at the given distance.
		/// </summary>
		/// <param name="fieldOfView">The field of view in radians.</param>
		/// <param name="distance">The distance at which the extent is calculated.</param>
		/// <returns>The extent of the view volume at the given distance.</returns>
		/// <remarks>
		/// <para>
		/// To calculate the width of the frustum the horizontal field of view must be specified.
		/// To calculate the height of the frustum the vertical field of view needs to be specified.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="distance"/> is negative.
		/// </exception>
		public static float GetExtent(float fieldOfView, float distance)
		{
			if (distance < 0)
				throw new ArgumentOutOfRangeException("distance", "The distance must not be negative.");

			return 2.0f * distance * (float)Math.Tan(fieldOfView / 2.0f);
		}


		/// <summary>
		/// Converts a field of view of a symmetric frustum to width and height.
		/// </summary>
		/// <param name="fieldOfViewY">The vertical field of view in degrees.</param>
		/// <param name="aspectRatio">The aspect ratio (width / height).</param>
		/// <param name="distance">
		/// The distance at which <paramref name="width"/> and <paramref name="height"/> are calculated.
		/// </param>
		/// <param name="width">
		/// The width of the view volume at the specified <paramref name="distance"/>.
		/// </param>
		/// <param name="height">
		/// The height of the view volume at the specified <paramref name="distance"/>.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="aspectRatio"/> is negative or 0, or <paramref name="distance"/> is negative.
		/// </exception>
		public static void GetWidthAndHeight(float fieldOfViewY, float aspectRatio, float distance, out float width, out float height)
		{
			if (aspectRatio <= 0)
				throw new ArgumentOutOfRangeException("aspectRatio", "The aspect ratio must not be negative or 0.");
			if (distance < 0)
				throw new ArgumentOutOfRangeException("distance", "The distance must not be negative.");

			height = 2.0f * distance * (float)Math.Tan(Microsoft.Xna.Framework.MathHelper.ToRadians(fieldOfViewY) / 2.0f);
			width = height * aspectRatio;
		}


		/// <summary>
		/// Gets the field of view from a frustum with the given extent.
		/// </summary>
		/// <param name="extent">
		/// The extent of the frustum at the specified <paramref name="distance"/>.
		/// </param>
		/// <param name="distance">The distance.</param>
		/// <returns>The field of view for the given extent.</returns>
		/// <remarks>
		/// To get the horizontal field of view the horizontal extent (x direction) needs to be 
		/// specified. To get the vertical field of view the vertical extent (y direction) needs to be
		/// specified.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="extent"/> is negative, or <paramref name="distance"/> is negative or 0.
		/// </exception>
		public static float GetFieldOfView(float extent, float distance)
		{
			if (extent < 0)
				throw new ArgumentOutOfRangeException("extent", "The extent of the frustum must be greater than or equal to 0.");
			if (distance <= 0)
				throw new ArgumentOutOfRangeException("distance", "The distance must be greater than 0.");

			return 2.0f * (float)Math.Atan(extent / (2.0f * distance));
		}

		#endregion
	}
}