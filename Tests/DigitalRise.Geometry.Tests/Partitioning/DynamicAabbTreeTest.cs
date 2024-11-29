﻿using System.Linq;
using DigitalRise.Collections;
using DigitalRise.Geometry.Shapes;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Geometry.Partitioning.Tests
{
  [TestFixture]
  public class DynamicBoundingBoxTreeTest
  {
    private BoundingBox GetBoundingBoxForItem(int i)
    {
      switch (i)
      {
        case 0:
          return new BoundingBox(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity));
        case 1:
          return new BoundingBox(new Vector3(-1), new Vector3(2));
        case 2:
          return new BoundingBox(new Vector3(1), new Vector3(3));
        case 3:
          return new BoundingBox(new Vector3(4), new Vector3(5));
        case 4:
          return new BoundingBox(new Vector3(0), new Vector3(1, float.NaN, 1));
        case 5: case 6: case 7:
          return new BoundingBox(new Vector3(0), new Vector3(float.PositiveInfinity));
        default:
          return new BoundingBox(new Vector3(), new Vector3());
      }
    }


    [Test]
    public void Infinite()
    {
      GlobalSettings.ValidationLevel = 0xff;

      var partition = new DynamicBoundingBoxTree<int>
      {
        EnableSelfOverlaps = true,
        GetBoundingBoxForItem = GetBoundingBoxForItem
      };

      partition.Add(1);
      partition.Add(0);
      partition.Add(2);
      partition.Add(3);

      Assert.AreEqual(new BoundingBox(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity)), partition.BoundingBox);

      var overlaps = partition.GetOverlaps().ToArray();
      Assert.AreEqual(4, overlaps.Length);
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 1)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 2)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 3)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(1, 2)));

      partition.Add(5);
      partition.Add(6);
      partition.Add(7);
      partition.Update(true);
      Assert.AreEqual(new BoundingBox(new Vector3(float.NegativeInfinity), new Vector3(float.PositiveInfinity)), partition.BoundingBox);

      overlaps = partition.GetOverlaps().ToArray();
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 1)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 2)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(0, 3)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(1, 2)));
      Assert.IsTrue(overlaps.Contains(new Pair<int>(1, 5)));
    }


    [Test]
    public void NaN()
    {
      GlobalSettings.ValidationLevel = 0x00;

      var partition = new DynamicBoundingBoxTree<int>
      {
        EnableSelfOverlaps = true,
        GetBoundingBoxForItem = GetBoundingBoxForItem
      };

      partition.Add(1);
      partition.Add(4);
      partition.Add(2);
      partition.Add(3);

      // BoundingBox builder throws exception.
      Assert.Throws<GeometryException>(() => partition.Update(false));
    }


    [Test]
    public void NaNWithValidation()
    {
      GlobalSettings.ValidationLevel = 0xff;

      var partition = new DynamicBoundingBoxTree<int>();
      partition.EnableSelfOverlaps = true;
      partition.GetBoundingBoxForItem = GetBoundingBoxForItem;

      partition.Add(1);
      partition.Add(4);
      partition.Add(2);
      partition.Add(3);

      // Full rebuild.
      Assert.Throws<GeometryException>(() => partition.Update(true));

      partition = new DynamicBoundingBoxTree<int>();
      partition.EnableSelfOverlaps = true;
      partition.GetBoundingBoxForItem = GetBoundingBoxForItem;

      partition.Add(1);
      partition.Add(2);
      partition.Add(3);
      partition.Update(true);
      partition.Add(4);

      // Partial rebuild.
      Assert.Throws<GeometryException>(() => partition.Update(false));
    }

    [Test]
    public void Clone()
    {
      DynamicBoundingBoxTree<int> partition = new DynamicBoundingBoxTree<int>();
      partition.GetBoundingBoxForItem = i => new BoundingBox();
      partition.EnableSelfOverlaps = true;
      partition.Filter = new DelegatePairFilter<int>(pair => true);
      partition.EnableMotionPrediction = true;
      partition.MotionPrediction = 0.25f;
      partition.OptimizationPerFrame = 0.1f;
      partition.RelativeMargin = 0.33f;
      partition.Add(0);
      partition.Add(1);
      partition.Add(2);
      partition.Add(3);

      var clone = (DynamicBoundingBoxTree<int>)partition.Clone();
      Assert.NotNull(clone);
      Assert.AreNotSame(clone, partition);
      Assert.AreEqual(clone.EnableSelfOverlaps, partition.EnableSelfOverlaps);
      Assert.AreEqual(clone.Filter, partition.Filter);
      Assert.AreEqual(clone.EnableMotionPrediction, partition.EnableMotionPrediction);
      Assert.AreEqual(clone.MotionPrediction, partition.MotionPrediction);
      Assert.AreEqual(clone.OptimizationPerFrame, partition.OptimizationPerFrame);
      Assert.AreEqual(clone.RelativeMargin, partition.RelativeMargin);
      Assert.AreEqual(0, clone.Count);

      clone.Add(0);
      Assert.AreEqual(4, partition.Count);
      Assert.AreEqual(1, clone.Count);
    }
  }
}
