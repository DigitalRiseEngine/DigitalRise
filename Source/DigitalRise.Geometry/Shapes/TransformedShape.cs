// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRise.Geometry.Meshes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;

using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes
{
  /// <summary>
  /// Represents a transformed shape.
  /// </summary>
  /// <remarks> 
  /// <para>
  /// This shape can be used to add a local transformation (scaling, rotation and translation) to a 
  /// <see cref="Shape"/>. The actual shape and the transformation is stored in 
  /// <see cref="Child"/>.
  /// </para>
  /// </remarks>
  [Serializable]
  public class TransformedShape : Shape
  {
		//--------------------------------------------------------------
		#region Properties
		//--------------------------------------------------------------

		/// <summary>
		/// Gets or sets the shape.
		/// </summary>
		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is <see langword="null"/>.
		/// </exception>
		public Shape Shape
		{
			get { return _shape; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				if (_shape != null)
				{
					_shape.Changed -= OnShapeChanged;
				}

				_shape = value;

				_shape.Changed += OnShapeChanged;
				OnChanged(ShapeChangedEventArgs.Empty);
			}
		}
		private Shape _shape;

		/// <summary>
		/// Gets or sets the pose (position and orientation).
		/// </summary>
		/// <inheritdoc/>
		public Pose Pose
		{
			get { return _pose; }
			set
			{
				if (_pose != value)
				{
					_pose = value;
					OnPoseChanged();
				}
			}
		}
		private Pose _pose;


		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value>
		/// The scale factors for the dimensions x, y and z. The default value is (1, 1, 1), which means
		/// "no scaling".
		/// </value>
		/// <remarks>
		/// <para>
		/// This value is a scale factor that scales the <see cref="Shape"/> of this geometric object.
		/// The scale can even be negative to mirror an object.
		/// </para>
		/// <para>
		/// Changing this value does not actually change any values in the <see cref="Shape"/> instance.
		/// Collision algorithms and anyone who uses this geometric object must use the shape and apply
		/// the scale factor as appropriate. The scale is automatically applied in the property
		/// <see cref="BoundingBox"/>.
		/// </para>
		/// <para>
		/// Changing this property raises the <see cref="ShapeChanged"/> event.
		/// </para>
		/// </remarks>
		public Vector3 Scale
		{
			get { return _scale; }
			set
			{
				if (_scale != value)
				{
					_scale = value;
					OnShapeChanged(this, ShapeChangedEventArgs.Empty);
				}
			}
		}
		private Vector3 _scale;


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
        Debug.Assert(Shape != null, "GeometricObject must not be null.");

        // Return the inner point of the child.
        return Pose.ToWorldPosition(Shape.InnerPoint * Scale);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShape"/> class.
    /// </summary>
    /// <remarks>
    /// <see cref="Child"/> is initialized with a <see cref="Child"/>
    /// with an <see cref="EmptyShape"/>.
    /// </remarks>
    public TransformedShape(): this(Shape.Empty, Pose.Identity, Vector3.One)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShape"/> class from the given 
    /// geometric object.
    /// </summary>
    /// <param name="shape">The geometric object (pose + shape).</param>
    /// <param name="pose"></param>
    /// <param name="scale"></param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public TransformedShape(Shape shape, Pose pose, Vector3 scale)
    {
      Shape = shape;
            Pose = pose;
            Scale = scale;
    }

        public TransformedShape(Shape shape, Pose pose): this(shape, pose, Vector3.One)
        {
        }

		public TransformedShape(Shape shape, Vector3 scale) : this(shape, Pose.Identity, scale)
		{
		}


		public TransformedShape(Shape shape) : this(shape, Pose.Identity, Vector3.One)
		{
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		#region ----- Cloning -----

		/// <inheritdoc/>
		protected override Shape CreateInstanceCore()
    {
      var clone = Shape.Clone();
      return new TransformedShape(clone, Pose, Scale);
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
    }
    #endregion


    /// <inheritdoc/>
    public override BoundingBox GetBoundingBox(Vector3 scale, Pose pose)
    {
      // Note: 
      // Uniform scaling is no problem. The scale can be applied anytime in the process.
      // With uniform scaling we compute the child AABB directly for world space.
      // Non-uniform scaling cannot be used with rotated objects. With non-uniform
      // scaling we compute the AABB of the child in the parent space. Then we
      // apply the scale in parent space. Then we compute a world space AABB that contains
      // the parent space AABB.

      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Uniform scaling:
        // Transform the shape to world space and return its AABB.
        var childPose = new Pose(Pose.Position * scale.X, Pose.Orientation);
        return Shape.GetBoundingBox(scale.X * Scale, pose * childPose);
      }
      else
      {
        // Non-uniform scaling:
        // Get AABB of child, transform the box to world space and return its AABB.
        return Shape.GetBoundingBox(Scale, Pose).GetBoundingBox(scale, pose);

        // Possible improvement: We can check if child.Pose.Orientation contains no orientation.
        // Then we compute a tighter AABB like in the uniform case.
      }
    }


    /// <inheritdoc/>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      Vector3 scale = MathHelper.Absolute(Scale);
      return Shape.GetVolume(relativeError, iterationLimit) * scale.X * scale.Y * scale.Z;
    }


    /// <summary>
    /// Called when the shape of a child geometric object was changed.
    /// </summary>
    private void OnPoseChanged()
    {
      OnChanged(ShapeChangedEventArgs.Empty);
    }

    
    /// <summary>
    /// Called when the shape of a child geometric object was changed.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      OnChanged(eventArgs);
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Convert absolute error to relative error.
      Vector3 extents = GetBoundingBox(Vector3.One, Pose.Identity).Extent();
      float maxExtent = extents.LargestComponent();
      float relativeThreshold = !Numeric.IsZero(maxExtent) 
                                ? absoluteDistanceThreshold / maxExtent
                                : Numeric.EpsilonF;

      // Get child mesh.
      TriangleMesh mesh = Shape.GetMesh(relativeThreshold, iterationLimit);

      // Transform child mesh into local space of this parent shape.
      mesh.Transform(Pose.ToMatrix44F() * Matrix44F.CreateScale(Scale));
      return mesh;
    }
    #endregion
  }
}
