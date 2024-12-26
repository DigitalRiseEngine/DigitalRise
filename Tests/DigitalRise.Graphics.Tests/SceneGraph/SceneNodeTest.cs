using DigitalRise.SceneGraph;
using NUnit.Framework;


namespace DigitalRise.SceneGraph.Tests
{
	[TestFixture]
	public class SceneNodeTest
	{
		[Test]
		public void UserFlagsTest()
		{
			var sceneNode = new SceneNode();
			sceneNode.UserFlags = 0x5555;
			Assert.AreEqual(0x5555, sceneNode.UserFlags);
		}
	}
}
