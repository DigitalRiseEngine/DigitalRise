using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class CircleTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new CircleShape().Radius);

      Assert.AreEqual(0, new CircleShape(0).Radius);

      Assert.AreEqual(3, new CircleShape(3).Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new CircleShape(-10);
    }


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3(0, 0, 0), new CircleShape(2).InnerPoint);
    }


    [Test]
    public void TestProperties()
    {
      CircleShape b = new CircleShape();
      Assert.AreEqual(0, b.Radius);

      b.Radius = 4;
      Assert.AreEqual(4, b.Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RadiusException()
    {
      CircleShape b = new CircleShape();
      b.Radius = -1;   
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new BoundingBox(), new CircleShape().GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(10, 100, -13), new Vector3(10, 100, -13)),
                     new CircleShape().GetBoundingBox(new Pose(new Vector3(10, 100, -13),
                                                                         MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f))));
      Assert.AreEqual(new BoundingBox(new Vector3(0, 90, 1000), new Vector3(20, 110, 1000)),
                     new CircleShape(10).GetBoundingBox(new Pose(new Vector3(10, 100, 1000),
                                                                   Quaternion.Identity)));
      // TODO: Test rotations.
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3(0, 0, 0), new CircleShape().GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new CircleShape().GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new CircleShape().GetSupportPoint(new Vector3(0, 0, 1)));
      Assert.AreEqual(new Vector3(0, 0, 0), new CircleShape().GetSupportPoint(new Vector3(1, 1, 1)));

      Assert.AreEqual(new Vector3(10, 0, 0), new CircleShape(10).GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(new Vector3(0, 10, 0), new CircleShape(10).GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(new Vector3(10, 0, 0), new CircleShape(10).GetSupportPoint(new Vector3(0, 0, 1)));
      Assert.AreEqual(new Vector3(-10, 0, 0), new CircleShape(10).GetSupportPoint(new Vector3(-1, 0, 0)));
      Assert.AreEqual(new Vector3(0, -10, 0), new CircleShape(10).GetSupportPoint(new Vector3(0, -1, 0)));
      Assert.AreEqual(new Vector3(10, 0, 0), new CircleShape(10).GetSupportPoint(new Vector3(0, 0, -1)));
      Assert.AreEqual(10 * new Vector3(1, 1, 0).Normalized(), new CircleShape(10).GetSupportPoint(new Vector3(1, 1, 1)));
      Assert.AreEqual(10 * new Vector3(-1, -1, 0).Normalized(), new CircleShape(10).GetSupportPoint(new Vector3(-1, -1, -1)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new CircleShape().GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(0, new CircleShape().GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(0, new CircleShape().GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(0, new CircleShape().GetSupportPointDistance(new Vector3(1, 1, 1)));

    //  AssertExt.AreNumericallyEqual(10, new CircleShape(10).GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  AssertExt.AreNumericallyEqual(10, new CircleShape(10).GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  AssertExt.AreNumericallyEqual(0, new CircleShape(10).GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  AssertExt.AreNumericallyEqual(MathHelper.ProjectTo(new Vector3(-10, 0, 0), new Vector3(-1, 0, -1)).Length, new CircleShape(10).GetSupportPointDistance(new Vector3(-1, 0, -1)));
    //  AssertExt.AreNumericallyEqual(MathHelper.ProjectTo(10*new Vector3(1, 1, 0).Normalized, new Vector3(1, 1, 1)).Length, new CircleShape(10).GetSupportPointDistance(new Vector3(1, 1, 1)));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("CircleShape { Radius = 1 }", new CircleShape(1).ToString());
    }


    [Test]
    public void Clone()
    {
      CircleShape circle = new CircleShape(1.23f);
      CircleShape clone = circle.Clone() as CircleShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(circle.Radius, clone.Radius);
      Assert.AreEqual(circle.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(circle.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new CircleShape(11);

      // Serialize object.
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(Shape));
      serializer.Serialize(stream, a);

      // Output generated xml. Can be manually checked in output window.
      stream.Position = 0;
      var xml = new StreamReader(stream).ReadToEnd();
      Trace.WriteLine("Serialized Object:\n" + xml);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new XmlSerializer(typeof(Shape));
      var b = (CircleShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
    }


    [Test]
    public void GetMesh()
    {
      var s = new CircleShape(3);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.Greater(mesh.NumberOfTriangles, 1);

      foreach(var vertex in mesh.Vertices)
      {
        Assert.AreEqual(0, vertex.Z);
      }
    }
  }
}
