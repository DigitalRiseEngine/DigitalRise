﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Geometry.Tests
{
  [TestFixture]
  public class DirectionalLookupTableTest
  {
    [Test]
    public void DirectionalLookup()
    {
      DirectionalLookupTableF<int> lookupTable = new DirectionalLookupTableF<int>(4);
      Assert.AreEqual(4, ((dynamic)lookupTable.Internals).Width);

      // Store 6 * 4 * 4 values.
      int value = 0;
      foreach (Vector3 direction in lookupTable.GetSampleDirections())
      {
        lookupTable[direction] = value;
        value++;
      }

      Assert.AreEqual(6 * 4 * 4, value);

      // Check values.
      value = 0;
      foreach (Vector3 direction in lookupTable.GetSampleDirections())
      {
        Assert.AreEqual(value, lookupTable[direction]);
        value++;
      }

      Assert.AreEqual(6 * 4 * 4, value);
    }
  }
}
