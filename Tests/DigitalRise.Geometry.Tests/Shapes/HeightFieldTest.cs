using System;
using DigitalRise.Mathematics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class HeightFieldTest
  {
    HeightField _field;
    float[] _samples;

    [SetUp]
    public void Setup()
    {
      _samples = new float [] { 0, 1, 2, 
                                3, 4, 2, 
                                1, 0, 1, 
                                2, 2, 2, 
                                5, 3, 2, 
                                1, 2, 1, 
                                2, 0, 3, 
                               -1, 0, 2 };
      _field = new HeightField
      {
        OriginX = 1000,
        OriginZ = 2000,
        WidthX = 100, 
        WidthZ = 200,
      };
      _field.SetSamples(_samples, 3, 8);
    }


    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new HeightField().OriginX);
      Assert.AreEqual(0, new HeightField().OriginZ);
      Assert.AreEqual(1000, new HeightField().WidthX);
      Assert.AreEqual(1000, new HeightField().WidthZ);
      Assert.AreEqual(4, new HeightField().Samples.Length);
      Assert.AreEqual(2, new HeightField().NumberOfSamplesX);
      Assert.AreEqual(2, new HeightField().NumberOfSamplesZ);
    }


    [Test]
    public void PropertiesTest()
    {
      Assert.AreEqual(100, new HeightField().Depth);
      Assert.AreEqual(100, new HeightField(0, 0, 10, 20, _samples, 3, 8).Depth);
      Assert.AreEqual(_samples, new HeightField(0, 0, 10, 20, _samples, 3, 8).Samples);
      Assert.AreEqual(10, new HeightField(0, 0, 10, 20, _samples, 3, 8).WidthX);
      Assert.AreEqual(20, new HeightField(0, 0, 10, 20, _samples, 3, 8).WidthZ);
      Assert.AreEqual(1, new HeightField(1, 2, 10, 20, _samples, 3, 8).OriginX);
      Assert.AreEqual(2, new HeightField(1, 2, 10, 20, _samples, 3, 8).OriginZ);
      Assert.AreEqual(3, new HeightField(1, 2, 10, 20, _samples, 3, 8).NumberOfSamplesX);
      Assert.AreEqual(8, new HeightField(1, 2, 10, 20, _samples, 3, 8).NumberOfSamplesZ);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PropertiesException()
    {
      _field.WidthX = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PropertiesException2()
    {
      _field.WidthZ = -1;
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PropertiesException3()
    {
      _field.SetSamples(null, 10, 10);
    }


    //[Test]
    //[ExpectedException(typeof(ArgumentException))]
    //public void PropertiesException4()
    //{
    //  _field.Array = new float[,] { {0}, {0} };
    //}


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void PropertiesException5()
    {
      _field.SetSamples(new float[0], 10, 10);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PropertiesException6()
    {
      _field.SetSamples(new float[10], 1, 10);
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void PropertiesException7()
    {
      _field.SetSamples(new float[10], 10, 1);
    }


    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new BoundingBox(new Vector3(0, -100, 0), new Vector3(1000, 0, 1000)), 
                      new HeightField().GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(1000, -101, 2000), new Vector3(1100, 5, 2200)), 
                      _field.GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(0, -1, 0), new Vector3(10, 5, 20)), 
                      new HeightField(0, 0, 10, 20, _samples, 3, 8) { Depth = 0 }.GetBoundingBox(Pose.Identity));

      // Now with pose.
      Quaternion rotation = MathHelper.CreateRotationX(0.2f);
      Pose pose = new Pose(new Vector3(1, 1, 1), rotation);
      _field.Depth = 0;
      var box = new TransformedShape(new BoxShape(100, 6, 200), new Pose(new Vector3(1050, 2, 2100)));
      AssertExt.AreNumericallyEqual(box.GetBoundingBox(pose).Min, _field.GetBoundingBox(pose).Min);
      _field.Depth = 4;
      box = new TransformedShape(new BoxShape(100, 10, 200), new Pose(new Vector3(1000, 0, 2000)));
      AssertExt.AreNumericallyEqual(box.GetBoundingBox(pose).Min + rotation.Rotate(new Vector3(50, 0, 100)), _field.GetBoundingBox(pose).Min);
    }


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("HeightField { OriginX = 1000, OriginZ = 2000, WidthX = 100, WidthZ = 200 }", _field.ToString());
    }


    [Test]
    public void HeightFieldWithHoles()
    {
      float[,] array = new float[2, 3];
      array[0, 0] = float.NaN;
      array[0, 1] = 1;
      array[0, 2] = float.NaN;
      array[1, 0] = float.NaN;
      array[1, 1] = 4;
      array[1, 2] = float.NaN;

      HeightField heightField = new HeightField(0, 0, 10, 20, _samples, 3, 8);
      
      // Check if returned values do not contain NaN.
      Assert.IsTrue(Numeric.IsFinite(heightField.InnerPoint.Y));
      Assert.IsTrue(Numeric.IsFinite(heightField.GetBoundingBox(Pose.Identity).Extent().Length()));
    }


    [Test]
    public void InnerPoint()
    {
      var aabb = _field.GetBoundingBox(Pose.Identity);
      Assert.IsTrue(GeometryHelper.HaveContact(aabb, _field.InnerPoint));
      Assert.IsTrue(_field.InnerPoint.Y < _field.GetHeight(_field.InnerPoint.X, _field.InnerPoint.Z));
    }


    [Test]
    public void Clone()
    {
      float[] array = new float[6] { 0, 1, 2, 3, 4, 5, };

      HeightField heightField = new HeightField(100, 200, 1.23f, 45.6f, array, 2, 3);
      HeightField clone = heightField.Clone() as HeightField;
      Assert.IsNotNull(clone);
      Assert.AreNotSame(heightField.Samples, clone.Samples);
      Assert.AreEqual(heightField.OriginX, clone.OriginX);
      Assert.AreEqual(heightField.OriginZ, clone.OriginZ);
      Assert.AreEqual(heightField.WidthX, clone.WidthX);
      Assert.AreEqual(heightField.WidthZ, clone.WidthZ);
      Assert.AreEqual(heightField.Depth, clone.Depth);
      Assert.AreEqual(heightField.NumberOfSamplesX, clone.NumberOfSamplesX);
      Assert.AreEqual(heightField.NumberOfSamplesZ, clone.NumberOfSamplesZ);
      Assert.AreEqual(heightField.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(heightField.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }


    [Test]
    public void GetHeight()
    {
      AssertExt.AreNumericallyEqual(4, _field.GetHeight(1050, 2000 + 200 / 7.0f));
    }
  }
}

