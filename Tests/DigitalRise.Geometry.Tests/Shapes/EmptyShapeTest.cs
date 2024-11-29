using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace DigitalRise.Geometry.Shapes.Tests
{
  [TestFixture]
  public class EmptyShapeTest
  {
    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new BoundingBox(), Shape.Empty.GetBoundingBox(Pose.Identity));
      Assert.AreEqual(new BoundingBox(new Vector3(11, 12, -13), new Vector3(11, 12, -13)),
                      Shape.Empty.GetBoundingBox(new Pose(new Vector3(11, 12, -13), MathHelper.CreateRotation(new Vector3(1, 1, 1), 0.7f))));
    }


    [Test]
    public void Clone()
    {
      var emptyShape = Shape.Empty;
      var clone = emptyShape.Clone() as EmptyShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(emptyShape.GetBoundingBox(Pose.Identity).Min, clone.GetBoundingBox(Pose.Identity).Min);
      Assert.AreEqual(emptyShape.GetBoundingBox(Pose.Identity).Max, clone.GetBoundingBox(Pose.Identity).Max);
    }


    [Test]
    public void SerializationXml()
    {
      var a = Shape.Empty;

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
      var b = (EmptyShape)deserializer.Deserialize(stream);

      Assert.IsNotNull(b);
    }


    [Test]
    public void GetMesh()
    {
      var s = Shape.Empty;
      var mesh = s.GetMesh(0.05f, 3);
      Assert.AreEqual(0, mesh.NumberOfTriangles);
    }
  }
}
