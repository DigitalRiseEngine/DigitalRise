using System;
using NUnit.Framework;


namespace DigitalRise.Tests
{
	[TestFixture]
	public class GraphicsExceptionTest
	{
		[Test]
		public void TestConstructors()
		{
			var exception = new GraphicsException();

			exception = new GraphicsException("message");
			Assert.AreEqual("message", exception.Message);

			exception = new GraphicsException("message", new Exception("inner"));
			Assert.AreEqual("message", exception.Message);
			Assert.AreEqual("inner", exception.InnerException.Message);
		}
	}
}