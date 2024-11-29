using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
	[TestFixture]
	public class MinkowskiSumShapeTest
	{
		GeometricObject child0, child1;
		MinkowskiSumShape cs;

		[SetUp]
		public void SetUp()
		{
			child0 = new GeometricObject(new CircleShape(3), new Pose(new Vector3(), MathHelper.CreateRotationX(ConstantsF.PiOver2)));
			child1 = new GeometricObject(new LineSegmentShape(new Vector3(0, 5, 0), new Vector3(0, -5, 0)), Pose.Identity);

			cs = new MinkowskiSumShape
			{
				ObjectA = child0,
				ObjectB = child1
			};
		}


		[Test]
		public void InnerPoint()
		{
			Assert.AreEqual(new Vector3(0, 0, 0), new ConvexHullOfShapes().InnerPoint);
			Assert.AreEqual(new Vector3(0, 0, 0), cs.InnerPoint);
			cs.ObjectB = new GeometricObject(new PointShape(new Vector3(5, 0, 0)), new Pose(new Vector3(1, 0, 0), Quaternion.Identity));
			Assert.AreEqual(new Vector3(6, 0, 0), cs.InnerPoint);
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
			Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3(1, 0, 0)));
			Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3(0, 1, 0)));
			Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3(0, 0, 1)));
			Assert.AreEqual(new Vector3(0, 0, 0), new MinkowskiSumShape().GetSupportPoint(new Vector3(1, 1, 1)));

			AssertExt.AreNumericallyEqual(new Vector3(3, -5, 0), cs.GetSupportPoint(new Vector3(1, -1, 0)));
			AssertExt.AreNumericallyEqual(new Vector3(3, 5, 0), cs.GetSupportPoint(new Vector3(1, 1, 0)));
			AssertExt.AreNumericallyEqual(new Vector3(0, -5, 3), cs.GetSupportPoint(new Vector3(0, 0, 1)));
			AssertExt.AreNumericallyEqual(new Vector3(-3, -5, 0), cs.GetSupportPoint(new Vector3(-1, 0, 0)));
			AssertExt.AreNumericallyEqual(new Vector3(3, -5, 0), cs.GetSupportPoint(new Vector3(0, -1, 0)));
			AssertExt.AreNumericallyEqual(new Vector3(0, -5, -3), cs.GetSupportPoint(new Vector3(0, 0, -1)));
			AssertExt.AreNumericallyEqual(new Vector3(0, 5, 0) + 3 * new Vector3(1, 0, 1).Normalized(), cs.GetSupportPoint(new Vector3(1, 1, 1)));
			AssertExt.AreNumericallyEqual(new Vector3(0, -5, 0) + 3 * new Vector3(-1, 0, -1).Normalized(), cs.GetSupportPoint(new Vector3(-1, -1, -1)));
		}


		//[Test]
		//public void ToStringTest()
		//{
		//  Assert.AreEqual("MinkowskiSumShape()", cs.ToString());
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

			MinkowskiSumShape minkowskiSumShape = new MinkowskiSumShape(geometryA, geometryB);
			MinkowskiSumShape clone = minkowskiSumShape.Clone() as MinkowskiSumShape;
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
			Assert.AreEqual(minkowskiSumShape.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
			Assert.AreEqual(minkowskiSumShape.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
		}
	}
}
