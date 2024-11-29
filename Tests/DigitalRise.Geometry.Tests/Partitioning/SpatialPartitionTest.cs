using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRise.Collections;
using DigitalRise.Geometry.Shapes;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using NUnit.Utils;
using Ray = DigitalRise.Geometry.Shapes.Ray;

namespace DigitalRise.Geometry.Partitioning.Tests
{
  [TestFixture]
  public class SpatialPartitionTest
  {
    class TestObject
    {
      public static int NextId;

      public int Id;
      public BoundingBox BoundingBox;
      public int Group;

      public TestObject(BoundingBox aabb)
      {
        Id = NextId++;
        BoundingBox = aabb;
        Group = RandomHelper.Random.NextInteger(0, 9);
      }

      public override string ToString()
      {
        return Id.ToString();
      }
    }


    List<TestObject> _testObjects = new List<TestObject>();
    List<TestObject> _testObjectsOfPartition2 = new List<TestObject>();
    private ISpatialPartition<int> _partition2; // Second test partition for partition vs. partition tests.
    private bool _conservativeBoundingBox;
    private bool _conservativeOverlaps;


    private static BoundingBox GetBoundingBoxOfTestObject(List<TestObject> testObjects, int id)
    {
      TestObject testObject = testObjects.FirstOrDefault(to => to.Id == id);
      return (testObject != null) ? testObject.BoundingBox : new BoundingBox();      
    }


    private BoundingBox GetBoundingBoxOfTestObject(int id)
    {
      return GetBoundingBoxOfTestObject(_testObjects, id);
    }


    private BoundingBox GetBoundingBoxOfTestObjectOfPartition2(int id)
    {
      return GetBoundingBoxOfTestObject(_testObjectsOfPartition2, id);
    }


    private bool AreInSameGroup(Pair<int> pair)
    {
      TestObject firstObject = _testObjects.FirstOrDefault(to => to.Id == pair.First);
      TestObject secondObject = _testObjects.FirstOrDefault(to => to.Id == pair.Second);
      int firstGroup = (firstObject != null) ? firstObject.Group : 0;
      int secondGroup = (secondObject != null) ? secondObject.Group : 0;
      return firstGroup == secondGroup;
    }


    [SetUp]
    public void SetUp()
    {
      RandomHelper.Random = new Random(1234567);

      _testObjects.Clear();
      _testObjectsOfPartition2.Clear();

      TestObject.NextId = 0;
      _testObjectsOfPartition2.Add(new TestObject(GetRandomBoundingBox()));
      _testObjectsOfPartition2.Add(new TestObject(GetRandomBoundingBox()));
      _testObjectsOfPartition2.Add(new TestObject(GetRandomBoundingBox()));
      _testObjectsOfPartition2.Add(new TestObject(GetRandomBoundingBox()));

      _partition2 = new BoundingBoxTree<int>();
      _partition2.GetBoundingBoxForItem = GetBoundingBoxOfTestObjectOfPartition2;
      _partition2.Add(0);
      _partition2.Add(1);
      _partition2.Add(2);
      _partition2.Add(3);

      _conservativeBoundingBox = false;
      _conservativeOverlaps = false;
    }


    [Test]
    public void TestDebugSpatialPartition()
    {
      TestPartition(new DebugSpatialPartition<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    [Test]
    public void TestBoundingBoxTree()
    {
      TestPartition(new BoundingBoxTree<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    [Test]
    public void TestCompressedBoundingBoxTree()
    {
      _conservativeBoundingBox = true;
      _conservativeOverlaps = true;
      TestPartition(new CompressedBoundingBoxTree { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    [Test]
    public void TestSAP()
    {
      TestPartition(new SweepAndPruneSpace<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    [Test]
    public void TestDynamicBoundingBoxTreeWithoutMotionPrediction()
    {
      _conservativeBoundingBox = true;
      _conservativeOverlaps = true;
      TestPartition(new DynamicBoundingBoxTree<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    [Test]
    public void TestDynamicBoundingBoxTreeWithMotionPrediction()
    {
      _conservativeBoundingBox = true;
      _conservativeOverlaps = true;
      TestPartition(new DynamicBoundingBoxTree<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject, EnableMotionPrediction = true });
    }


    [Test]
    public void TestDualPartition()
    {
      _conservativeBoundingBox = true;
      _conservativeOverlaps = true;
      TestPartition(new DualPartition<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    [Test]
    public void TestAdaptiveBoundingBoxTree()
    {
      TestPartition(new AdaptiveBoundingBoxTree<int> { GetBoundingBoxForItem = GetBoundingBoxOfTestObject });
    }


    private void TestPartition(ISpatialPartition<int> partition)
    {
      partition.Clear();
      Assert.AreEqual(0, partition.Count);

      partition.EnableSelfOverlaps = true;
      Assert.AreEqual(0, partition.GetOverlaps().Count());
      Assert.AreEqual(0, partition.GetOverlaps(0).Count());
      Assert.AreEqual(0, partition.GetOverlaps(new BoundingBox()).Count());
      Assert.AreEqual(0, partition.GetOverlaps(_partition2).Count());
      Assert.AreEqual(0, partition.GetOverlaps(Vector3.One, Pose.Identity, _partition2, Vector3.One, Pose.Identity).Count());


      var testObject = new TestObject(new BoundingBox(new Vector3(10), new Vector3(10)));
      _testObjects.Add(testObject);
      partition.Add(testObject.Id);

      for (int i = 0; i < 1000; i++)
      {
        // ----- Tests        
        Assert.AreEqual(_testObjects.Count, partition.Count, "Wrong number of items.");

        if (i > 10 && i % 6 == 0)
          TestGetOverlaps0(partition);
        if (i > 10 && i % 6 == 1)
          TestGetOverlaps1(partition);
        if (i > 10 && i % 6 == 2)
          TestGetOverlaps2(partition);
        if (i > 10 && i % 6 == 3)
          TestGetOverlaps3(partition);
        if (i > 10 && i % 6 == 4)
          TestGetOverlaps4(partition);
        if (i > 10 && i % 6 == 5)
          TestGetOverlaps5(partition);

        // Update partition. From time to time rebuild all.
        // For the above tests update should have been called automatically!
        partition.Update(i % 10 == 9);
        TestBoundingBox(partition);

        var dice100 = RandomHelper.Random.Next(0, 100);
        if (dice100 < 2)
        {
          // Test remove/re-add without Update inbetween.
          if (partition.Count > 0)
          {
            partition.Remove(_testObjects[0].Id);
            partition.Add(_testObjects[0].Id);
          }
        }


        dice100 = RandomHelper.Random.Next(0, 100);
        if (dice100 < 10)
        {
          // Remove objects.
          int removeCount = RandomHelper.Random.NextInteger(1, 4);
          for (int k = 0; k < removeCount && partition.Count > 0; k++)
          {
            var index = RandomHelper.Random.NextInteger(0, partition.Count - 1);
            var obj = _testObjects[index];
            _testObjects.Remove(obj);
            partition.Remove(obj.Id);
          }
        }

        dice100 = RandomHelper.Random.Next(0, 100);
        if (dice100 < 10)
        {
          // Add new objects.
          int addCount = RandomHelper.Random.NextInteger(1, 4);
          for (int k = 0; k < addCount; k++)
          {
            var newObj = new TestObject(GetRandomBoundingBox());
            _testObjects.Add(newObj);
            partition.Add(newObj.Id);
          }
        }
        else
        {
          // Move an object.
          int moveCount = RandomHelper.Random.NextInteger(1, 10);
          for (int k = 0; k < moveCount && partition.Count > 0; k++)
          {
            var index = RandomHelper.Random.NextInteger(0, partition.Count - 1);
            var obj = _testObjects[index];
            obj.BoundingBox = GetRandomBoundingBox();
            partition.Invalidate(obj.Id);
          }
        }

        // From time to time invalidate all.
        if (dice100 < 3)
          partition.Invalidate();

        // From time to time change EnableSelfOverlaps.
        if (dice100 > 3 && dice100 < 6)
          partition.EnableSelfOverlaps = false;
        else if (dice100 < 10)
          partition.EnableSelfOverlaps = true;

        // From time to time change filter.
        if (dice100 > 10 && dice100 < 13)
        {
          partition.Filter = null;
        }
        else if (dice100 < 10)
        {
          if (partition.Filter == null)
            partition.Filter = new DelegatePairFilter<int>(AreInSameGroup);
        }
      }

      partition.Clear();
      Assert.AreEqual(0, partition.Count);
    }


    private BoundingBox GetRandomBoundingBox()
    {
      var point = RandomHelper.Random.NextVector3(0, 100);
      var point2 = RandomHelper.Random.NextVector3(0, 100);
      var newBoundingBox = new BoundingBox(point, point);
      newBoundingBox.Grow(point2);
      return newBoundingBox;
    }


    private void TestBoundingBox(ISpatialPartition<int> partition)
    {
      if (_testObjects.Count == 0)
        return;

      // Compute desired result.
      var desiredBoundingBox = _testObjects[0].BoundingBox;
      _testObjects.ForEach(obj => desiredBoundingBox.Grow(obj.BoundingBox));

      // The AABB of the spatial partition can be slightly bigger.
      // E.g. the CompressedBoundingBoxTree adds a margin to avoid divisions by zero.
      Assert.IsTrue(Numeric.IsFinite(partition.BoundingBox.Min.X));
      Assert.IsTrue(Numeric.IsFinite(partition.BoundingBox.Min.Y));
      Assert.IsTrue(Numeric.IsFinite(partition.BoundingBox.Min.Z));
      Assert.IsTrue(Numeric.IsFinite(partition.BoundingBox.Max.X));
      Assert.IsTrue(Numeric.IsFinite(partition.BoundingBox.Max.Y));
      Assert.IsTrue(Numeric.IsFinite(partition.BoundingBox.Max.Z));

      if (_conservativeBoundingBox)
      {
        // AABB can be bigger than actual objects.
        Assert.IsTrue(partition.BoundingBox.Contains(desiredBoundingBox) == ContainmentType.Contains, "Wrong AABB: AABB is too small.");
      }
      else
      {
        // The AABB should be identical.
        AssertExt.AreNumericallyEqual(desiredBoundingBox.Min, partition.BoundingBox.Min);
        AssertExt.AreNumericallyEqual(desiredBoundingBox.Max, partition.BoundingBox.Max);        
      }
    }


    private void TestGetOverlaps0(ISpatialPartition<int> partition)
    {
      var aabb = GetRandomBoundingBox();

      // Compute desired result.
      var desiredResults = new List<int>();
      foreach (var testObject in _testObjects)
      {
        if (GeometryHelper.HaveContact(aabb, testObject.BoundingBox))
          desiredResults.Add(testObject.Id);
      }

      var results = partition.GetOverlaps(aabb).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(BoundingBox) returns different number of results.");
    }


    private void TestGetOverlaps1(ISpatialPartition<int> partition)
    {
      // Temporarily add random test object.
      var randomTestObject = new TestObject(GetRandomBoundingBox());
      _testObjects.Add(randomTestObject);

      // Compute desired result.
      var desiredResults = new List<int>();
      foreach (var testObject in _testObjects)
      {
        if (testObject == randomTestObject)
          continue;

        if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(randomTestObject.Id, testObject.Id)))
          if (GeometryHelper.HaveContact(randomTestObject.BoundingBox, testObject.BoundingBox))
            desiredResults.Add(testObject.Id);
      }

      var results = partition.GetOverlaps(randomTestObject.Id).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(T) returns different number of results.");

      _testObjects.Remove(randomTestObject);
    }


    private void TestGetOverlaps2(ISpatialPartition<int> partition)
    {
      var aabb = GetRandomBoundingBox();
      var ray = new Ray(aabb.Min, aabb.Extent().Normalized(), aabb.Extent().Length());

      ray.Direction = RandomHelper.Random.NextVector3(-1, 1).Normalized();

      // Compute desired result.
      var desiredResults = new List<int>();
      foreach (var testObject in _testObjects)
      {
        if (GeometryHelper.HaveContact(testObject.BoundingBox, ray))
          desiredResults.Add(testObject.Id);
      }

      var results = partition.GetOverlaps(ray).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(Ray) returns different number of results.");
    }


    private void TestGetOverlaps3(ISpatialPartition<int> partition)
    {
      if (!partition.EnableSelfOverlaps)
        return;

      // Compute desired result.
      var desiredResults = new List<Pair<int>>();
      for (int i = 0; i < _testObjects.Count; i++)
      {
        var a = _testObjects[i];
        for (int j = i + 1; j < _testObjects.Count; j++)
        {
          var b = _testObjects[j];
          if (a != b)
            if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(a.Id, b.Id)))
              if (GeometryHelper.HaveContact(a.BoundingBox, b.BoundingBox))
                desiredResults.Add(new Pair<int>(a.Id, b.Id));
        }
      }

      var results = partition.GetOverlaps().ToList();

      if (desiredResults.Count != results.Count)
      {
        var distinct = results.Except(desiredResults).ToList();
      }

      CompareResults(desiredResults, results, "GetOverlaps() returns different number of results.");
    }


    private void TestGetOverlaps4(ISpatialPartition<int> partition)
    {
      // Compute desired result.
      var desiredResults = new List<Pair<int>>();
      foreach (var a in _testObjects)
      {
        foreach (var b in _testObjectsOfPartition2)
        {
          if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(a.Id, b.Id)))
            if (GeometryHelper.HaveContact(a.BoundingBox, b.BoundingBox))
              desiredResults.Add(new Pair<int>(a.Id, b.Id));
        }
      }

      var results = partition.GetOverlaps(_partition2).ToList();
      CompareResults(desiredResults, results, "GetOverlaps(Partition) returns different number of results.");
    }


    private void TestGetOverlaps5(ISpatialPartition<int> partition)
    {
      // Get random pose for _partition2
      var pose = new Pose(GetRandomBoundingBox().Center(), RandomHelper.Random.NextQuaternion());
      var scale = RandomHelper.Random.NextVector3(0.1f, 3f);

      // Compute desired result.
      var desiredResults = new List<Pair<int>>();
      foreach (var a in _testObjects)
      {
        foreach (var b in _testObjectsOfPartition2)
        {
          if (partition.Filter == null || partition.Filter.Filter(new Pair<int>(a.Id, b.Id)))
          {
            var aabbB = b.BoundingBox;
            aabbB.Scale(scale);
            var boxB = aabbB.Extent();
            var poseB = pose * new Pose(aabbB.Center());

            if (GeometryHelper.HaveContact(a.BoundingBox, boxB, poseB, true))
              desiredResults.Add(new Pair<int>(a.Id, b.Id));
          }
        }
      }

      var results = partition.GetOverlaps(Vector3.One, Pose.Identity, _partition2, scale, pose).ToList();

      if (desiredResults.Count > results.Count)
        Debugger.Break();

      CompareResults(desiredResults, results, "GetOverlaps(Partition, Pose, Scale) returns a wrong number of results or has missed an overlap.");
    }


    private void CompareResults(List<int> expected, List<int> actual, string message)
    {
      // The spatial partition must have computed all desired overlaps. It is ok if it has computed 
      // a bit more. (Some partitions are more conservative.)
      if (_conservativeOverlaps)
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
      }
      else
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
        Assert.LessOrEqual(actual.Count - expected.Count, 3);
      }

      expected.ForEach(id => Assert.IsTrue(actual.Contains(id), message));
    }


    private void CompareResults(List<Pair<int>> expected, List<Pair<int>> actual, string message)
    {
      // The spatial partition must have computed all desired overlaps. It is ok if it has computed 
      // a  bit more. (Some partitions are more conservative.)
      if (_conservativeOverlaps)
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
      }
      else
      {
        Assert.LessOrEqual(expected.Count, actual.Count, message);
        Assert.LessOrEqual(actual.Count - expected.Count, 3);
      }

      expected.ForEach(pair => Assert.IsTrue(actual.Contains(pair), message));
    }
  }
}
