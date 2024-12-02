using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Geometry.Shapes.Tests
{
	[TestFixture]
	public class TransformShapeTest
	{
		[Test]
		public void Constructor()
		{
			Assert.AreNotEqual(null, new TransformedShape().Shape);
			Assert.AreEqual(Vector3.Zero, new TransformedShape().InnerPoint);
		}


		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GeometryException()
		{
			TransformedShape t = new TransformedShape();
			t.Shape = null;
		}


		[Test]
		public void InnerPoint()
		{
			Assert.AreEqual(Pose.Identity, new TransformedShape().Pose);

			Assert.AreEqual(new Vector3(0, 0, 0), new CompositeShape().InnerPoint);
		}


		[Test]
		public void InnerPoint2()
		{
			TransformedShape t = new TransformedShape(new PointShape(1, 0, 0), new Pose(new Vector3(0, 1, 0)));

			Assert.AreEqual(new Vector3(1, 1, 0), t.InnerPoint);
		}


		[Test]
		public void GetBoundingBox()
		{
			Assert.AreEqual(new Vector3(0, 0, 0), new TransformedShape().GetBoundingBox(Pose.Identity).Min);
			Assert.AreEqual(new Vector3(0, 0, 0), new TransformedShape().GetBoundingBox(Pose.Identity).Max);

			TransformedShape t = new TransformedShape(new SphereShape(10), new Pose(new Vector3(0, 1, 0)));

			Assert.AreEqual(new Vector3(-10, -9, -10), t.GetBoundingBox(Pose.Identity).Min);
			Assert.AreEqual(new Vector3(10, 11, 10), t.GetBoundingBox(Pose.Identity).Max);

			Assert.AreEqual(new Vector3(-8, -9, -10), t.GetBoundingBox(new Pose(new Vector3(2, 0, 0))).Min);
			Assert.AreEqual(new Vector3(12, 11, 10), t.GetBoundingBox(new Pose(new Vector3(2, 0, 0))).Max);
		}


		private bool _propertyChanged;
		[Test]
		public void PropertyChangedTest()
		{
			TransformedShape t = new TransformedShape();
			t.Changed += delegate { _propertyChanged = true; };

			Assert.IsFalse(_propertyChanged);

			t.Shape = new SphereShape(1);
			Assert.IsTrue(_propertyChanged);
			_propertyChanged = false;

			((SphereShape)t.Shape).Radius = 3;
			Assert.IsTrue(_propertyChanged);
			_propertyChanged = false;

			t.Pose = new Pose(new Vector3(1, 2, 3));
			Assert.IsTrue(_propertyChanged);
			_propertyChanged = false;

			// Setting Pose to the same value does not create a changed event.
			t.Pose = new Pose(new Vector3(1, 2, 3));
			Assert.IsFalse(_propertyChanged);
			_propertyChanged = false;

			t.Pose = Pose.Identity;
			Assert.IsTrue(_propertyChanged);
			_propertyChanged = false;

			t.Shape = Shape.Empty;
			Assert.IsTrue(_propertyChanged);
			_propertyChanged = false;

			// Setting Pose to the same value does not create a changed event.
			t.Pose = Pose.Identity;
			Assert.IsFalse(_propertyChanged);
			_propertyChanged = false;
		}


		[Test]
		public void Clone()
		{
			Pose pose = new Pose(new Vector3(1, 2, 3));
			PointShape pointShape = new PointShape(3, 4, 5);

			TransformedShape transformedShape = new TransformedShape(pointShape, pose);
			TransformedShape clone = transformedShape.Clone() as TransformedShape;
			Assert.IsNotNull(clone);
			Assert.IsNotNull(clone.Shape);
			Assert.AreNotSame(pointShape, clone.Shape);
			Assert.AreEqual(pose, clone.Pose);
			Assert.IsNotNull(clone.Shape);
			Assert.AreNotSame(pointShape, clone.Shape);
			Assert.IsTrue(clone.Shape is PointShape);
			Assert.AreEqual(pointShape.Position, ((PointShape)clone.Shape).Position);
			Assert.AreEqual(transformedShape.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
			Assert.AreEqual(transformedShape.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
		}
	}
}
