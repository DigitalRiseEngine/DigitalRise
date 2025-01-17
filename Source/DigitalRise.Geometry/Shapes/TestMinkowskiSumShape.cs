// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;

namespace DigitalRise.Geometry.Shapes
{
  /// <summary>
  /// Represents a lightweight <see cref="MinkowskiSumShape"/> implementation without events.
  /// (Internal use only.)
  /// </summary>
  /// <remarks>
  /// This <see cref="MinkowskiSumShape"/> implementation is used by collision algorithms to get a
  /// temporary <see cref="MinkowskiSumShape"/> instance for tests. This shape cannot be used for
  /// normal <see cref="IGeometricObject"/> shapes!
  /// </remarks>
  internal sealed class TestMinkowskiSumShape : ConvexShape, IRecyclable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    public static readonly ResourcePool<TestMinkowskiSumShape> Pool =
      new ResourcePool<TestMinkowskiSumShape>(
        () => new TestMinkowskiSumShape(),
        null,
        null);
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    public override Vector3 InnerPoint
    {
      get
      {
        // Return the sum of child inner points.
        // Return the difference of child inner points.
        Vector3 innerPointA = _objectA.Pose.ToWorldPosition(_objectA.Shape.InnerPoint);
        Vector3 innerPointB = _objectB.Pose.ToWorldPosition(_objectB.Shape.InnerPoint);
        return innerPointA + innerPointB;
      }
    }


    public TestGeometricObject ObjectA
    {
      get { return _objectA; }
      set { _objectA = value; }
    }
    private TestGeometricObject _objectA;


    public TestGeometricObject ObjectB
    {
      get { return _objectB; }
      set { _objectB = value; }
    }
    private TestGeometricObject _objectB;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    private TestMinkowskiSumShape()
    {
    }


    public static TestMinkowskiSumShape Create()
    {
      return Pool.Obtain();
    }


    public void Recycle()
    {
      ObjectA = null;
      ObjectB = null;
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override Shape CreateInstanceCore()
    {
      throw new NotImplementedException();
    }


    protected override void CloneCore(Shape sourceShape)
    {
      throw new NotImplementedException();
    }


    public override Vector3 GetSupportPoint(Vector3 direction)
    {
      Vector3 directionLocalA = _objectA.Pose.ToLocalDirection(direction);
      Vector3 directionLocalB = _objectB.Pose.ToLocalDirection(direction);
      Vector3 pointALocalA = ((ConvexShape)_objectA.Shape).GetSupportPoint(directionLocalA);
      Vector3 pointBLocalB = ((ConvexShape)_objectB.Shape).GetSupportPoint(directionLocalB);
      Vector3 pointA = _objectA.Pose.ToWorldPosition(pointALocalA);
      Vector3 pointB = _objectB.Pose.ToWorldPosition(pointBLocalB);
      return pointA + pointB;
    }


    public override float GetVolume(float relativeError, int iterationLimit)
    {
      throw new NotImplementedException();
    }


    public override Vector3 GetSupportPointNormalized(Vector3 directionNormalized)
    {
      Vector3 directionLocalA = _objectA.Pose.ToLocalDirection(directionNormalized);
      Vector3 directionLocalB = _objectB.Pose.ToLocalDirection(directionNormalized);
      Vector3 pointALocalA = ((ConvexShape)_objectA.Shape).GetSupportPointNormalized(directionLocalA);
      Vector3 pointBLocalB = ((ConvexShape)_objectB.Shape).GetSupportPointNormalized(directionLocalB);
      Vector3 pointA = _objectA.Pose.ToWorldPosition(pointALocalA);
      Vector3 pointB = _objectB.Pose.ToWorldPosition(pointBLocalB);
      return pointA + pointB;
    }
    #endregion
  }
}
