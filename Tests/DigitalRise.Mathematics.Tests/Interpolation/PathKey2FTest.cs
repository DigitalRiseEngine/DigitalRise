using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using NUnit.Framework;


namespace DigitalRise.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class PathKey2FTest
  {
    [Test]
    public void SerializationXml()
    {
      PathKey2F pathKey1 = new PathKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector2(1.2f, 3.4f),
        TangentIn = new Vector2(0.7f, 2.6f),
        TangentOut = new Vector2(1.9f, 3.3f)
      };
      PathKey2F pathKey2;

      const string fileName = "SerializationPath2FKey.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(PathKey2F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, pathKey1);
      writer.Close();

      serializer = new XmlSerializer(typeof(PathKey2F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      pathKey2 = (PathKey2F)serializer.Deserialize(fileStream);
      MathAssert.AreEqual(pathKey1, pathKey2);
    }
  }
}
