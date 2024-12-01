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
  public class PerspectiveViewVolumeTest
  {
    [Test]
    public void ConvertToHorizontalFieldOfViewTest()
    {
      float horizontalFieldOfView = PerspectiveViewVolume.GetFieldOfViewX(MathHelper.ToRadians(60), 1);
      float expectedFieldOfView = MathHelper.ToRadians(60);
      AssertExt.AreNumericallyEqual(expectedFieldOfView, horizontalFieldOfView);

      horizontalFieldOfView = PerspectiveViewVolume.GetFieldOfViewX(MathHelper.ToRadians(60), (float)(4.0 / 3.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(75.178179f);
      AssertExt.AreNumericallyEqual(expectedFieldOfView, horizontalFieldOfView);

      horizontalFieldOfView = PerspectiveViewVolume.GetFieldOfViewX(MathHelper.ToRadians(45), (float)(16.0 / 9.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(72.734351f);
      AssertExt.AreNumericallyEqual(expectedFieldOfView, horizontalFieldOfView);
    }

    [Test]
    public void ConvertToVerticalFieldOfViewTest()
    {
      float verticalFieldOfView = PerspectiveViewVolume.GetFieldOfViewY(MathHelper.ToRadians(90), 1);
      float expectedFieldOfView = MathHelper.ToRadians(90);
      AssertExt.AreNumericallyEqual(expectedFieldOfView, verticalFieldOfView);

      verticalFieldOfView = PerspectiveViewVolume.GetFieldOfViewY(MathHelper.ToRadians(75), (float)(4.0 / 3.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(59.840444f);
      AssertExt.AreNumericallyEqual(expectedFieldOfView, verticalFieldOfView);

      verticalFieldOfView = PerspectiveViewVolume.GetFieldOfViewY(MathHelper.ToRadians(90), (float)(16.0 / 9.0));
      expectedFieldOfView = (float)MathHelper.ToRadians(58.715507f);
      AssertExt.AreNumericallyEqual(expectedFieldOfView, verticalFieldOfView);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetHorizontalViewException2()
    {
      PerspectiveViewVolume.GetFieldOfViewX(ConstantsF.PiOver4, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetVerticalViewException2()
    {
      PerspectiveViewVolume.GetFieldOfViewY(ConstantsF.PiOver4, 0);
    }

    [Test]
    public void GetExtentTest()
    {
      float extent;
      extent = PerspectiveViewVolume.GetExtent(MathHelper.ToRadians(90), 1);
      AssertExt.AreNumericallyEqual(2, extent);

      extent = PerspectiveViewVolume.GetExtent(MathHelper.ToRadians(60), 10);
      AssertExt.AreNumericallyEqual(11.547005f, extent);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetExtentException2()
    {
      PerspectiveViewVolume.GetExtent(ConstantsF.PiOver4, -0.1f);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetWidthAndHeightException2()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 0, 1, out width, out height);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetWidthAndHeightException3()
    {
      float width, height;
      PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 1, -0.1f, out width, out height);
    }

    [Test]
    public void GetFieldOfViewTest()
    {
      float fieldOfView;
      fieldOfView = PerspectiveViewVolume.GetFieldOfView(2, 1);
      AssertExt.AreNumericallyEqual(MathHelper.ToRadians(90), fieldOfView);

      fieldOfView = PerspectiveViewVolume.GetFieldOfView(1.1547005f, 1);
      AssertExt.AreNumericallyEqual(MathHelper.ToRadians(60), fieldOfView);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetFieldOfViewException()
    {
      PerspectiveViewVolume.GetFieldOfView(-0.1f, 1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetFieldOfViewException2()
    {
      PerspectiveViewVolume.GetFieldOfView(1, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void NearException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume
      {
        Near = 0,
        Far = 10
      };
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void FarException()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume
      {
        Near = 1,
        Far = 0
      };
    }


    [Test]
    public void SetFieldOfViewTest()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

      var rect = frustum.Rectangle;
      AssertExt.AreNumericallyEqual(-2.0528009f / 2.0f, rect.Left);
      AssertExt.AreNumericallyEqual(2.0528009f / 2.0f, rect.Right);
      AssertExt.AreNumericallyEqual(-1.1547005f / 2.0f, rect.Bottom);
      AssertExt.AreNumericallyEqual(1.1547005f / 2.0f, rect.Top);
      Assert.AreEqual(1, frustum.Near);
      Assert.AreEqual(10, frustum.Far);
      AssertExt.AreNumericallyEqual(2.0528009f, rect.Width);
      AssertExt.AreNumericallyEqual(1.1547005f, rect.Height);
      Assert.AreEqual(9, frustum.Depth);
      Assert.AreEqual(16.0f / 9.0f, frustum.AspectRatio);
      AssertExt.AreNumericallyEqual(MathHelper.ToRadians(60), frustum.FieldOfViewY);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException3()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 0, 1, 10);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SetFieldOfViewException4()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 0, 10);
    }


    [Test]
    public void SetFieldOfView2Test()
    {
      PerspectiveViewVolume frustum = new PerspectiveViewVolume();
      frustum.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

      PerspectiveViewVolume frustum2 = new PerspectiveViewVolume { Near = 1, Far = 10 };
      frustum2.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f);

      Assert.AreEqual(frustum.Near, frustum2.Near);
      Assert.AreEqual(frustum.Far, frustum2.Far);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.IsTrue(new PerspectiveViewVolume().ToString().Contains("PerspectiveViewVolume"));
    }

     [Test]
    public void Clone()
    {
      PerspectiveViewVolume perspectiveViewVolume = new PerspectiveViewVolume(1.23f, 2.13f, 1.01f, 10.345f);
      PerspectiveViewVolume clone = perspectiveViewVolume.Clone() as PerspectiveViewVolume;
      Assert.IsNotNull(clone);
      Assert.AreEqual(perspectiveViewVolume.Rectangle.Left, clone.Rectangle.Left);
      Assert.AreEqual(perspectiveViewVolume.Rectangle.Right, clone.Rectangle.Right);
      Assert.AreEqual(perspectiveViewVolume.Rectangle.Bottom, clone.Rectangle.Bottom);
      Assert.AreEqual(perspectiveViewVolume.Rectangle.Top, clone.Rectangle.Top);
      Assert.AreEqual(perspectiveViewVolume.Near, clone.Near);
      Assert.AreEqual(perspectiveViewVolume.Far, clone.Far);
      Assert.AreEqual(perspectiveViewVolume.FieldOfViewY, clone.FieldOfViewY);
      Assert.AreEqual(perspectiveViewVolume.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(perspectiveViewVolume.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new PerspectiveViewVolume(1.23f, 2.13f, 1.01f, 10.345f);

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
      var b = (PerspectiveViewVolume)deserializer.Deserialize(stream);

      Assert.AreEqual(a.Rectangle.Left, b.Rectangle.Left);
      Assert.AreEqual(a.Rectangle.Right, b.Rectangle.Right);
      Assert.AreEqual(a.Rectangle.Top, b.Rectangle.Top);
      Assert.AreEqual(a.Rectangle.Bottom, b.Rectangle.Bottom);
      Assert.AreEqual(a.Near, b.Near);
      Assert.AreEqual(a.Far, b.Far);
      Assert.AreEqual(a.InnerPoint, b.InnerPoint);
    }

		[Test]
		public void GetWidthAndHeightTest()
		{
			float width, height;
			PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(90), 1, 1, out width, out height);
			AssertExt.AreNumericallyEqual(2, width);
			AssertExt.AreNumericallyEqual(2, height);

			PerspectiveViewVolume.GetWidthAndHeight(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, out width, out height);
			AssertExt.AreNumericallyEqual(2.0528009f, width);
			AssertExt.AreNumericallyEqual(1.1547005f, height);

			// We are pretty confident that the ViewVolume.CreateViewVolumeXxx() works. 
			// Use ViewVolume.CreateViewVolumeXxx() to test GetWidthAndHeight().
			Matrix44F projection = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);
			Matrix44F projection2 = Matrix44F.CreatePerspective(width, height, 1, 10);
			AssertExt.AreNumericallyEqual(projection, projection2);
		}


		[Test]
		public void SetViewVolumeFieldOfViewTest()
		{
			PerspectiveViewVolume projection = new PerspectiveViewVolume();
			projection.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);

			PerspectiveViewVolume projection2 = new PerspectiveViewVolume();
			projection2.SetFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f);
			projection2.Near = 1;
			projection2.Far = 10;

			Matrix44F expected = Matrix44F.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), 16.0f / 9.0f, 1, 10);
			AssertExt.AreNumericallyEqual(expected, projection.Projection);
			AssertExt.AreNumericallyEqual(expected, projection2.Projection);
		}
	}
}