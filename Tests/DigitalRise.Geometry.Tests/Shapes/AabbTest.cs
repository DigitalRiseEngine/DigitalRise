using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class BoundingBoxTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(new Vector3(), new BoundingBox().Min);
      Assert.AreEqual(new Vector3(), new BoundingBox().Max);

      Assert.AreEqual(new Vector3(), new BoundingBox(Vector3.Zero, Vector3.Zero).Min);
      Assert.AreEqual(new Vector3(), new BoundingBox(Vector3.Zero, Vector3.Zero).Max);

      Assert.AreEqual(new Vector3(10, 20, 30), new BoundingBox(new Vector3(10, 20, 30), new Vector3(11, 22, 33)).Min);
      Assert.AreEqual(new Vector3(11, 22, 33), new BoundingBox(new Vector3(10, 20, 30), new Vector3(11, 22, 33)).Max);
    }


    [Test]
    public void TestProperties()
    {
      BoundingBox b = new BoundingBox();
      Assert.AreEqual(new Vector3(), b.Min);
      Assert.AreEqual(new Vector3(), b.Max);

      b.Min = new Vector3(-10, -20, -30);
      Assert.AreEqual(new Vector3(-10, -20, -30), b.Min);
      Assert.AreEqual(new Vector3(), b.Max);

      b.Max = new Vector3(100, 200, 300);
      Assert.AreEqual(new Vector3(-10, -20, -30), b.Min);
      Assert.AreEqual(new Vector3(100, 200, 300), b.Max);

      Assert.AreEqual(new Vector3(90f / 2, 180f / 2, 270f / 2), b.Center());
      Assert.AreEqual(new Vector3(110, 220, 330), b.Extent());
    }


    [Test]
    public void EqualsTest()
    {
      Assert.IsTrue(new BoundingBox().Equals(new BoundingBox()));
      Assert.IsTrue(new BoundingBox().Equals(new BoundingBox(Vector3.Zero, Vector3.Zero)));
      Assert.IsTrue(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsFalse(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals(new BoundingBox(new Vector3(0, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsFalse(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).Equals(new LineSegmentShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6))));
      Assert.IsFalse(new BoundingBox().Equals(null));

      Assert.IsTrue(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)) == new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)));
      Assert.IsTrue(new BoundingBox(new Vector3(1, 2, 4), new Vector3(4, 5, 6)) != new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)));
    }


    [Test]
    public void AreNumericallyEqual()
    {
      AssertExt.AreNumericallyEqual(new BoundingBox(), new BoundingBox());
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)), 
                                             new BoundingBox(new Vector3(1, 2, 3 + Numeric.EpsilonF / 2), new Vector3(4, 5, 6))));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                                             new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6 + Numeric.EpsilonF / 2))));
      Assert.IsFalse(MathHelper.AreNumericallyEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                                              new BoundingBox(new Vector3(1, 2, 3 + 10 * Numeric.EpsilonF), new Vector3(4, 5, 6))));

      AssertExt.AreNumericallyEqual(new BoundingBox(), new BoundingBox());
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                                             new BoundingBox(new Vector3(1, 2, 3.1f), new Vector3(4, 5, 6)),
                                             0.2f));
      Assert.IsTrue(MathHelper.AreNumericallyEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                                             new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5.1f, 6)),
                                             0.2f));
      Assert.IsFalse(MathHelper.AreNumericallyEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)),
                                             new BoundingBox(new Vector3(1, 2, 3.3f), new Vector3(4, 5, 6)),
                                             0.2f));
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new BoundingBox(), new BoundingBox().GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(10, 100, 1000), new Vector3(10, 100, 1000)),
                      new BoundingBox().GetBoundingBox(new Pose(new Vector3(10, 100, 1000), Quaternion.Identity)));
      Assert.AreEqual(new BoundingBox(new Vector3(10, 100, 1000), new Vector3(10, 100, 1000)),
                      new BoundingBox().GetBoundingBox(new Pose(new Vector3(10, 100, 1000), MathHelper.CreateRotation(new Vector3(1, 2, 3), 0.7f))));
      
      
      BoundingBox aabb = new BoundingBox(new Vector3(1, 10, 100), new Vector3(2, 20, 200));
      Assert.AreEqual(aabb, aabb.GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(11, 110, 1100), new Vector3(12, 120, 1200)),
                      aabb.GetBoundingBox(new Pose(new Vector3(10, 100, 1000), Quaternion.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GrowFromPoint()
    {
      var a = new BoundingBox(new Vector3(1, 2, 3), new Vector3(3, 4, 5));
      a.Grow(new Vector3(10, -20, -30));
      Assert.AreEqual(new BoundingBox(new Vector3(1, -20, -30), new Vector3(10, 4, 5)), a);
    }


    [Test]
    public void GetHashCodeTest()
    {
      Assert.AreEqual(new BoundingBox().GetHashCode(), new BoundingBox().GetHashCode());
      Assert.AreEqual(new BoundingBox().GetHashCode(), new BoundingBox(Vector3.Zero, Vector3.Zero).GetHashCode());
      Assert.AreEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode(), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode(), new BoundingBox(new Vector3(0, 2, 3), new Vector3(4, 5, 6)).GetHashCode());
      Assert.AreNotEqual(new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode(), new LineSegmentShape(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetHashCode());
    }


    //[Test]
    //public void GetSupportPoint()
    //{
    //  Assert.AreEqual(new Vector3(0, 0, 0), new BoundingBox().GetSupportPoint(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(new Vector3(0, 0, 0), new BoundingBox().GetSupportPoint(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(new Vector3(0, 0, 0), new BoundingBox().GetSupportPoint(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(new Vector3(0, 0, 0), new BoundingBox().GetSupportPoint(new Vector3(1, 1, 1)));

    //  Assert.AreEqual(new Vector3(4, 5, 6), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(new Vector3(4, 5, 6), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(new Vector3(4, 5, 6), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(new Vector3(1, 5, 6), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(-1, 0, 0)));
    //  Assert.AreEqual(new Vector3(4, 2, 6), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(0, -1, 0)));
    //  Assert.AreEqual(new Vector3(4, 5, 3), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(0, 0, -1)));
    //  Assert.AreEqual(new Vector3(4, 5, 6), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(1, 1, 1)));
    //  Assert.AreEqual(new Vector3(1, 2, 3), new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6)).GetSupportPoint(new Vector3(-1, -1, -1)));
    //}


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new BoundingBox().GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(0, new BoundingBox().GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(0, new BoundingBox().GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(0, new BoundingBox().GetSupportPointDistance(new Vector3(1, 1, 1)));

    //  BoundingBox aabb = new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
    //  AssertExt.AreNumericallyEqual(4, aabb.GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  AssertExt.AreNumericallyEqual(5, aabb.GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  AssertExt.AreNumericallyEqual(6, aabb.GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  AssertExt.AreNumericallyEqual(-1, aabb.GetSupportPointDistance(new Vector3(-1, 0, 0)));
    //  AssertExt.AreNumericallyEqual(-2, aabb.GetSupportPointDistance(new Vector3(0, -1, 0)));
    //  AssertExt.AreNumericallyEqual(-3, aabb.GetSupportPointDistance(new Vector3(0, 0, -1)));
    //  AssertExt.AreNumericallyEqual(Vector3.Dot(new Vector3(1, 2, 6), new Vector3(-1, -1, 0).Normalized()), aabb.GetSupportPointDistance(new Vector3(-1, -1, 0)));
    //  AssertExt.AreNumericallyEqual(MathHelper.ProjectTo(new Vector3(4, 5, 6), new Vector3(1, 1, 1)).Length, aabb.GetSupportPointDistance(new Vector3(1, 1, 1)));
    //}


    [Test]
    public void Scale()
    {
      BoundingBox aabb = new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
      aabb.Scale(new Vector3(-2, 3, 4));
      Assert.AreEqual(new BoundingBox(new Vector3(-8, 6, 12), new Vector3(-2, 15, 24)), aabb);

      aabb = new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
      aabb.Scale(new Vector3(2, -3, 4));
      Assert.AreEqual(new BoundingBox(new Vector3(2, -15, 12), new Vector3(8, -6, 24)), aabb);

      aabb = new BoundingBox(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
      aabb.Scale(new Vector3(2, 3, -4));
      Assert.AreEqual(new BoundingBox(new Vector3(2, 6, -24), new Vector3(8, 15, -12)), aabb);
    }
  }
}
