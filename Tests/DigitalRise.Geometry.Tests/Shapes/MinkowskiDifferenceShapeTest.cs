using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class MinkowskiDifferenceShapeTest
  {
    MinkowskiDifferenceShape cs;
    
    [SetUp]
    public void SetUp()
    {
      cs = new MinkowskiDifferenceShape();
      cs.ObjectA = new GeometricObject(new CircleShape(3), new Pose(new Vector3(1, 0, 0), MathHelper.CreateRotationX(ConstantsF.PiOver2)));
      cs.ObjectB = new GeometricObject(new LineSegmentShape(new Vector3(0, 5, 0), new Vector3(0, -5, 0)), Pose.Identity);
    }

    [Test]
    public void Constructor()
    {
      Assert.AreEqual(Vector3.Zero, ((PointShape)new MinkowskiDifferenceShape().ObjectA.Shape).Position);
      Assert.AreEqual(Vector3.Zero, ((PointShape)new MinkowskiDifferenceShape().ObjectB.Shape).Position);
      Assert.AreEqual(Pose.Identity, new MinkowskiDifferenceShape().ObjectA.Pose);
      Assert.AreEqual(Pose.Identity, new MinkowskiDifferenceShape().ObjectB.Pose);

      var m = new MinkowskiDifferenceShape(
        new GeometricObject(new CircleShape(3), new Pose(new Vector3(1, 0, 0), MathHelper.CreateRotationX(ConstantsF.PiOver2))),
        new GeometricObject(new LineSegmentShape(new Vector3(0, 5, 0), new Vector3(0, -5, 0)), Pose.Identity));
      Assert.AreEqual(new Vector3(1, 0, 0), m.ObjectA.Pose.Position);
      Assert.AreEqual(new Vector3(0, 0, 0), m.ObjectB.Pose.Position);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException()
    {
      var m = new MinkowskiDifferenceShape(null, new GeometricObject());
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ConstructorException2()
    {
      var m = new MinkowskiDifferenceShape(new GeometricObject(), null);
    }


    [Test]
    public void PropertiesTest()
    {
      Assert.AreEqual(3, ((CircleShape)cs.ObjectA.Shape).Radius);
      Assert.AreEqual(new Vector3(0, 5, 0), ((LineSegmentShape)cs.ObjectB.Shape).Start);
      Assert.AreEqual(new Vector3(0, -5, 0), ((LineSegmentShape) cs.ObjectB.Shape).End);
      Assert.AreEqual(new Pose(new Vector3(1, 0, 0), MathHelper.CreateRotationX(ConstantsF.PiOver2)), cs.ObjectA.Pose);
      Assert.AreEqual(Pose.Identity, cs.ObjectB.Pose);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PropertiesException()
    {
      cs.ObjectA = null;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PropertiesException2()
    {
      cs.ObjectB = null;
    }    


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3(1, 0, 0), cs.InnerPoint);
    }


    //[Test]
    //public void GetBoundingBox()
    //{
    //  Assert.AreEqual(new BoundingBox(), new ConvexHullOfPoints().GetBoundingBox(Pose.Identity));
    //  Assert.AreEqual(new BoundingBox(new Vector3(10, 100, -13), new Vector3(10, 100, -13)),
    //                 new ConvexHullOfPoints().GetBoundingBox(new Pose(new Vector3(10, 100, -13),
    //                                                                     MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f))));
    //  Assert.AreEqual(new BoundingBox(new Vector3(11, 102, 1003), new Vector3(11, 102, 1003)),
    //                 new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetBoundingBox(new Pose(new Vector3(10, 100, 1000),
    //                                                                     Quaternion.Identity)));
    //  Quaternion rotation = MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f);
    //  Vector3 worldPos = rotation.Rotate(new Vector3(1, 2, 3)) + new Vector3(10, 100, 1000);
    //  AssertExt.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetBoundingBox(new Pose(new Vector3(10, 100, 1000), rotation)).Minimum);
    //  AssertExt.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetBoundingBox(new Pose(new Vector3(10, 100, 1000), rotation)).Maximum);
    //}


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3(0, 0, 1)));
      Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiDifferenceShape().GetSupportPoint(new Vector3(1, 1, 1)));

      AssertExt.AreNumericallyEqual(new Vector3(4, 5, 0), cs.GetSupportPoint(new Vector3(1, 0, 0)));
      AssertExt.AreNumericallyEqual(new Vector3(4, 5, 0), cs.GetSupportPoint(new Vector3(0, 1, 0)));
      AssertExt.AreNumericallyEqual(new Vector3(1, 5, 3), cs.GetSupportPoint(new Vector3(0, 0, 1)));
      AssertExt.AreNumericallyEqual(new Vector3(-2, 5, 0), cs.GetSupportPoint(new Vector3(-1, 0, 0)));
      AssertExt.AreNumericallyEqual(new Vector3(4, -5, 0), cs.GetSupportPoint(new Vector3(0, -1, 0)));

      MinkowskiDifferenceShape m = new MinkowskiDifferenceShape();
      ((GeometricObject)m.ObjectB).Shape = new LineSegmentShape(new Vector3(1, 0, 0), new Vector3(3, 0, 0));
      Assert.AreEqual(new Vector3(-1, 0, 0), m.GetSupportPoint(new Vector3(1, 1, 0)));
      ((GeometricObject)m.ObjectB).Pose = new Pose(new Vector3(1, 1, 0), Quaternion.Identity);
      Assert.AreEqual(new Vector3(-2, -1, 0), m.GetSupportPoint(new Vector3(1, 1, 0)));
      ((GeometricObject)m.ObjectA).Shape = new CircleShape(20);
      Assert.AreEqual(new Vector3(18, -1, 0), m.GetSupportPoint(new Vector3(1, 0, 0)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(0, new ConvexHullOfPoints().GetSupportPointDistance(new Vector3(1, 1, 1)));

    //  Assert.AreEqual(1, new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(2, new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(3, new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  AssertExt.AreNumericallyEqual(MathHelper.ProjectTo(new Vector3(1, 2, 3), new Vector3(1, 1, 1)).Length, new ConvexHullOfPoints(new Vector3(1, 2, 3)).GetSupportPointDistance(new Vector3(1, 1, 1)));
    //}


    //[Test]
    //public void ToStringTest()
    //{
    //  Assert.AreEqual("MinkowskiDifferenceShape()", cs.ToString());
    //}


    [Test]
    public void Clone()
    {
      Pose poseA = new Pose(new Vector3(1, 2, 3));
      PointShape pointA = new PointShape(3, 4, 5);
      GeometricObject geometryA = new GeometricObject(pointA, poseA);

      Pose poseB = new Pose(new Vector3(1, 2, 3));
      PointShape pointB = new PointShape(3, 4, 5);
      GeometricObject geometryB = new GeometricObject(pointB, poseB);

      MinkowskiDifferenceShape minkowskiDifferenceShape = new MinkowskiDifferenceShape(geometryA, geometryB);
      MinkowskiDifferenceShape clone = minkowskiDifferenceShape.Clone() as MinkowskiDifferenceShape;
      Assert.IsNotNull(clone);
      Assert.IsNotNull(clone.ObjectA);
      Assert.IsNotNull(clone.ObjectB);
      Assert.AreNotSame(geometryA, clone.ObjectA);
      Assert.AreNotSame(geometryB, clone.ObjectB);
      Assert.IsTrue(clone.ObjectA is GeometricObject);
      Assert.IsTrue(clone.ObjectB is GeometricObject);
      Assert.AreEqual(poseA, clone.ObjectA.Pose);
      Assert.AreEqual(poseB, clone.ObjectB.Pose);
      Assert.IsNotNull(clone.ObjectA.Shape);
      Assert.IsNotNull(clone.ObjectB.Shape);
      Assert.AreNotSame(pointA, clone.ObjectA.Shape);
      Assert.AreNotSame(pointB, clone.ObjectB.Shape);
      Assert.IsTrue(clone.ObjectA.Shape is PointShape);
      Assert.IsTrue(clone.ObjectB.Shape is PointShape);
      Assert.AreEqual(pointA.Position, ((PointShape)clone.ObjectA.Shape).Position);
      Assert.AreEqual(pointB.Position, ((PointShape)clone.ObjectB.Shape).Position);
      Assert.AreEqual(minkowskiDifferenceShape.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(minkowskiDifferenceShape.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }
    }
}
