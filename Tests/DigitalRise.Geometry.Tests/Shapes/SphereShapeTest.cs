using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class SphereTest
  {
    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new SphereShape().Radius);
      Assert.AreEqual(0, new SphereShape(0).Radius);
      Assert.AreEqual(10, new SphereShape(10).Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ConstructorException()
    {
      new SphereShape(-1);
    }


    [Test]
    public void Radius()
    {
      SphereShape s = new SphereShape();
      Assert.AreEqual(0, s.Radius);
      s.Radius = 3;
      Assert.AreEqual(3, s.Radius);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void RadiusException()
    {
      SphereShape s = new SphereShape();
      s.Radius = -1;
    }


    [Test]
    public void Volume()
    {
      var s = new SphereShape(17);
      Assert.AreEqual(4f/3f * ConstantsF.Pi * 17 * 17 * 17, s.GetVolume(0.1f, 1));
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new BoundingBox(), new SphereShape().GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(10, 100, -13), new Vector3(10, 100, -13)),
                     new SphereShape().GetBoundingBox(new Pose(new Vector3(10, 100, -13),
                                                                         MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f))));
      Assert.AreEqual(new BoundingBox(new Vector3(0, 90, 990), new Vector3(20, 110, 1010)),
                     new SphereShape(10).GetBoundingBox(new Pose(new Vector3(10, 100, 1000),
                                                                         MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f))));
    }


    [Test]
    public void GetSupportPoint()
    {
      Assert.AreEqual(new Vector3(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(new Vector3(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3(0, 0, 1)));
      Assert.AreEqual(new Vector3(0, 0, 0), new SphereShape().GetSupportPoint(new Vector3(1, 1, 1)));

      Assert.AreEqual(new Vector3(10, 0, 0), new SphereShape(10).GetSupportPoint(new Vector3(1, 0, 0)));
      Assert.AreEqual(new Vector3(0, 10, 0), new SphereShape(10).GetSupportPoint(new Vector3(0, 1, 0)));
      Assert.AreEqual(new Vector3(0, 0, 10), new SphereShape(10).GetSupportPoint(new Vector3(0, 0, 1)));
      AssertExt.AreNumericallyEqual(new Vector3(5.773502f), new SphereShape(10).GetSupportPoint(new Vector3(1, 1, 1)));
    }


    //[Test]
    //public void GetSupportPointDistance()
    //{
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(0, new Sphere().GetSupportPointDistance(new Vector3(1, 1, 1)));

    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3(1, 0, 0)));
    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3(0, 1, 0)));
    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3(0, 0, 1)));
    //  Assert.AreEqual(10, new Sphere(10).GetSupportPointDistance(new Vector3(1, 1, 1)));
    //}


    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3(0, 0, 0), new SphereShape(2).InnerPoint);
    }


    [Test]
    public void HaveContactWithPoint()
    {
      Assert.IsTrue(GeometryHelper.HaveContact(0, new Vector3()));
      Assert.IsTrue(GeometryHelper.HaveContact(0, new Vector3(Numeric.EpsilonF, 0, 0)));
      Assert.IsFalse(GeometryHelper.HaveContact(0, new Vector3(Numeric.EpsilonF)));

      Assert.IsTrue(GeometryHelper.HaveContact(10, new Vector3(0, 0, 0)));
      Assert.IsTrue(GeometryHelper.HaveContact(10, new Vector3(-10, 0, 0)));
      Assert.IsTrue(GeometryHelper.HaveContact(10, new Vector3(-10.00001f, 0, 0)));
      Assert.IsFalse(GeometryHelper.HaveContact(10, new Vector3(0, 10.01f, 0)));
    }

    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("SphereShape { Radius = 10 }", new SphereShape(10).ToString());
    }


    [Test]
    public void Clone()
    {
      SphereShape sphere = new SphereShape(0.1234f);
      SphereShape clone = sphere.Clone() as SphereShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(sphere.Radius, clone.Radius);
      Assert.AreEqual(sphere.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(sphere.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new SphereShape(11);

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
      var b = (SphereShape)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Radius, b.Radius);
    }


    [Test]
    public void GetMesh()
    {
      var s = new SphereShape(3);
      var mesh = s.GetMesh(0.05f, 3);
      Assert.Greater(mesh.NumberOfTriangles, 1);
      
      for (int i = 0; i < mesh.Vertices.Count; i++)
        AssertExt.AreNumericallyEqual(3, mesh.Vertices[i].Length());

    }
  }
}
