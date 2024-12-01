using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;


namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class OrthographicViewVolumeTest
  {
    [Test]
    public void BoundingBoxTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetOffCenter(-1, 1, -1, 1, 2, 5);
      BoundingBox aabb = viewVolume.GetBoundingBox(Pose.Identity);
      Assert.AreEqual(new Vector3(-1, -1, -5), aabb.Min);
      Assert.AreEqual(new Vector3(1, 1, -2), aabb.Max);
    }


    [Test]
    public void PropertiesTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume
      {
        Left = -2,
        Right = 2,
        Bottom = -1,
        Top = 1,
        Near = 2,
        Far = 10
      };

      Assert.AreEqual(-2, viewVolume.Left);
      Assert.AreEqual(2, viewVolume.Right);
      Assert.AreEqual(-1, viewVolume.Bottom);
      Assert.AreEqual(1, viewVolume.Top);
      Assert.AreEqual(2, viewVolume.Near);
      Assert.AreEqual(10, viewVolume.Far);
      Assert.AreEqual(4, viewVolume.Width);
      Assert.AreEqual(2, viewVolume.Height);
      Assert.AreEqual(8, viewVolume.Depth);
      Assert.AreEqual(2, viewVolume.AspectRatio);


      viewVolume = new OrthographicViewVolume
      {
        Left = 2,
        Right = -2,
        Bottom = 1,
        Top = -1,
        Near = 10,
        Far = 2
      };

      Assert.AreEqual(2, viewVolume.Left);
      Assert.AreEqual(-2, viewVolume.Right);
      Assert.AreEqual(1, viewVolume.Bottom);
      Assert.AreEqual(-1, viewVolume.Top);
      Assert.AreEqual(10, viewVolume.Near);
      Assert.AreEqual(2, viewVolume.Far);
      Assert.AreEqual(4, viewVolume.Width);
      Assert.AreEqual(2, viewVolume.Height);
      Assert.AreEqual(8, viewVolume.Depth);
      Assert.AreEqual(2, viewVolume.AspectRatio);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetOffCenter(2, 2, 3, 4, 5, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException2()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetOffCenter(1, 2, 4, 4, 5, 6);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetException3()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetOffCenter(1, 2, 3, 4, 6, 6);
    }

    [Test]
    public void SetWidthAndHeightTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(4, 3, 2, 9);

      Assert.AreEqual(-2, viewVolume.Left);
      Assert.AreEqual(2, viewVolume.Right);
      Assert.AreEqual(-1.5f, viewVolume.Bottom);
      Assert.AreEqual(1.5f, viewVolume.Top);
      Assert.AreEqual(2, viewVolume.Near);
      Assert.AreEqual(9, viewVolume.Far);
      Assert.AreEqual(4, viewVolume.Width);
      Assert.AreEqual(3, viewVolume.Height);
      Assert.AreEqual(7, viewVolume.Depth);
      Assert.AreEqual(4.0f / 3.0f, viewVolume.AspectRatio);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(0, 1, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetWidthAndHeightException2()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(2, 0, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void SetWidthAndHeightException3()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(2, 1, 1, 0);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new OrthographicViewVolume().ToString().Contains("OrthographicViewVolume"));
    }

    [Test]
    public void InnerPointTest()
    {
      OrthographicViewVolume viewVolume = new OrthographicViewVolume();
      viewVolume.SetWidthAndHeight(1, 1, 1, 10);
      Vector3 innerPoint = viewVolume.InnerPoint;
      Assert.AreEqual(0, innerPoint.X);
      Assert.AreEqual(0, innerPoint.Y);
      Assert.AreEqual(-5.5f, innerPoint.Z);
    }


    [Test]
    public void Clone()
    {
      OrthographicViewVolume orthographicViewVolume = new OrthographicViewVolume(-1.23f, 2.13f, -0.3f, 2.34f, 1.01f, 10.345f);
      OrthographicViewVolume clone = orthographicViewVolume.Clone() as OrthographicViewVolume;
      Assert.IsNotNull(clone);
      Assert.AreEqual(orthographicViewVolume.Left, clone.Left);
      Assert.AreEqual(orthographicViewVolume.Right, clone.Right);
      Assert.AreEqual(orthographicViewVolume.Bottom, clone.Bottom);
      Assert.AreEqual(orthographicViewVolume.Top, clone.Top);
      Assert.AreEqual(orthographicViewVolume.Near, clone.Near);
      Assert.AreEqual(orthographicViewVolume.Far, clone.Far);
      Assert.AreEqual(orthographicViewVolume.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(orthographicViewVolume.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new OrthographicViewVolume(-1.23f, 2.13f, -0.3f, 2.34f, 1.01f, 10.345f);

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
      var b = (OrthographicViewVolume)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Left, b.Left);
      Assert.AreEqual(a.Right, b.Right);
      Assert.AreEqual(a.Top, b.Top);
      Assert.AreEqual(a.Bottom, b.Bottom);
      Assert.AreEqual(a.Near, b.Near);
      Assert.AreEqual(a.Far, b.Far);
      Assert.AreEqual(a.InnerPoint, b.InnerPoint);
    }

		[Test]
		public void SetViewVolumeTest()
		{
			OrthographicViewVolume projection = new OrthographicViewVolume();
			projection.SetWidthAndHeight(4, 3, 2, 10);

			OrthographicViewVolume camera2 = new OrthographicViewVolume();
			camera2.SetWidthAndHeight(4, 3);
			camera2.Near = 2;
			camera2.Far = 10;

			OrthographicViewVolume camera3 = new OrthographicViewVolume
			{
				Left = -2,
				Right = 2,
				Bottom = -1.5f,
				Top = 1.5f,
				Near = 2,
				Far = 10,
			};

			Matrix44F expected = Matrix44F.CreateOrthographic(4, 3, 2, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, camera2.Projection);
			AssertExt.AreNumericallyEqual(expected, camera3.Projection);
		}

		[Test]
		public void SetViewVolumeOffCenterTest()
		{
			OrthographicViewVolume projection = new OrthographicViewVolume();
			projection.SetOffCenter(0, 4, 1, 4, 2, 10);

			OrthographicViewVolume camera2 = new OrthographicViewVolume();
			camera2.SetOffCenter(0, 4, 1, 4);
			camera2.Near = 2;
			camera2.Far = 10;

			ViewVolume camera3 = new OrthographicViewVolume
			{
				Left = 0,
				Right = 4,
				Bottom = 1,
				Top = 4,
				Near = 2,
				Far = 10,
			};

			Matrix44F expected = Matrix44F.CreateOrthographicOffCenter(0, 4, 1, 4, 2, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, camera2.Projection);
			AssertExt.AreNumericallyEqual(expected, camera3.Projection);
		}
	}
}