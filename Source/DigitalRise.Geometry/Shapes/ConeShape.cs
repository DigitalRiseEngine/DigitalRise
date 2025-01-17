// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRise.Geometry.Meshes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes
{
  /// <summary>
  /// Represents a cone standing on the xz plane, pointing up in the +y direction.
  /// </summary>
  [Serializable]
  public class ConeShape : ConvexShape
  {
    // See Bergen: "Collision Detection in Interactive 3D Environments", pp. 135

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private float? _sinθ; // Cached sine of half cone angle.
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point: (0, <see cref="Height"/> / 2, 0).</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3 InnerPoint
    {
      get { return new Vector3(0, _height / 2, 0); }
    }


    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Height
    {
      get { return _height; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The height must be greater than or equal to 0.");

        if (_height != value)
        {
          _height = value;
          if (_height == 0 && _radius == 0)
            _sinθ = null;
          else
            _sinθ = _radius / (float)Math.Sqrt(_radius * _radius + _height * _height);

          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _height;


    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    /// <value>The radius.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Radius
    {
      get { return _radius; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The radius must be greater than or equal to 0.");

        if (_radius != value)
        {
          _radius = value;
          if (_height == 0 && _radius == 0)
            _sinθ = null;
          else
            _sinθ = _radius / (float)Math.Sqrt(_radius * _radius + _height * _height);

          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _radius;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ConeShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ConeShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates an empty cone.
    /// </remarks>
    public ConeShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConeShape"/> class with the given radius and 
    /// height.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="height">The height.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="radius"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="height"/> is negative.
    /// </exception>
    public ConeShape(float radius, float height)
    {
      if (radius < 0)
        throw new ArgumentOutOfRangeException("radius", "The radius must be greater than or equal to 0.");
      if (height < 0)
        throw new ArgumentOutOfRangeException("height", "The height must be greater than or equal to 0.");

      _height = height;
      _radius = radius;

      if (_height == 0 && _radius == 0)
        _sinθ = null;
      else
        _sinθ = _radius / (float) Math.Sqrt(_radius * _radius + _height * _height);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new ConeShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (ConeShape)sourceShape;
      Radius = source.Radius;
      Height = source.Height;
    }
    #endregion


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
      // The general formula for cones with arbitrary orientation is in 
      // Bergen: "Collision Detection in Interactive 3D Environments", pp. 135.
      // The formula for cones with up-axis = y-axis is simpler:

      if(_sinθ.HasValue == false)
        return Vector3.Zero; // Height and Radius are 0

      if (directionNormalized.Y > _sinθ)
      {
        // Return the tip of the cone.
        return new Vector3(0, _height, 0);
      }

      Vector3 directionInXYPlane = new Vector3(directionNormalized.X, 0, directionNormalized.Z);
      float length = directionInXYPlane.Length();
      if (Numeric.IsZero(length))
      {
        // localDirection == +/-(0, 1, 0)
        Debug.Assert(
          MathHelper.AreNumericallyEqual(directionNormalized, Vector3.UnitY) || MathHelper.AreNumericallyEqual(directionNormalized, -Vector3.UnitY),
          "The vector direction should be (0, 1, 0) or (0, -1, 0).");
        
        // Return any point on the base circle.
        return new Vector3(_radius, 0, 0);
      }
      else
      {
        // Return the correct point on the base circle.
        return _radius * directionInXYPlane / length;
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
      float radius = Radius;
      return 1.0f / 3.0f * ConstantsF.Pi * radius * radius * Height;
    }


    /// <summary>
    /// Gets the volume of this cone.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The volume of this cone.</returns>
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
      // Estimate required segment angle for given accuracy. 
      // (Easy to derive from simple drawing of a circle segment with a triangle used to represent
      // the segment.)
      float alpha = (float)Math.Acos((_radius - absoluteDistanceThreshold) / _radius) * 2;
      int numberOfSegments = (int)Math.Ceiling(ConstantsF.TwoPi / alpha);

      // Apply the iteration limit - in case absoluteDistanceThreshold is 0.
      // Lets say each iteration doubles the number of segments. This is an arbitrary interpretation
      // of the "iteration limit".
      numberOfSegments = Math.Min(numberOfSegments, 2 << iterationLimit);

      alpha = ConstantsF.TwoPi / numberOfSegments;

      Vector3 r0 = new Vector3(_radius, 0, 0);
      Vector3 tip = new Vector3(0, _height, 0);
      Quaternion rotation = MathHelper.CreateRotationY(alpha);

      TriangleMesh mesh = new TriangleMesh();

      for (int i = 1; i <= numberOfSegments; i++)
      {
        Vector3 r1 = rotation.Rotate(r0);

        // Bottom triangle
        mesh.Add(new Triangle
        {
          Vertex0 = Vector3.Zero,
          Vertex1 = r1,
          Vertex2 = r0,
        }, false);

        // Side triangle
        mesh.Add(new Triangle
        {
          Vertex0 = r0,
          Vertex1 = r1,
          Vertex2 = tip,
        }, false);
        r0 = r1;
      }

      mesh.WeldVertices();

      return mesh;
    }

   
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "ConeShape {{ Radius = {0}, Height = {1} }}", _radius, _height);
    }
    #endregion
  }
}
