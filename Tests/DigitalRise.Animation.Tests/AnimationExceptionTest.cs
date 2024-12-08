using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using NUnit.Framework;


namespace DigitalRise.Animation.Tests
{
	[TestFixture]
	public class AnimationExceptionTest
	{
		[Test]
		public void ConstructorTest()
		{
			var exception = new AnimationException();
		}


		[Test]
		public void ConstructorTest1()
		{
			const string message = "message";
			var exception = new AnimationException(message);
			Assert.AreEqual(message, exception.Message);
		}


		[Test]
		public void ConstructorTest2()
		{
			const string message = "message";
			var innerException = new Exception();
			var exception = new AnimationException(message, innerException);
			Assert.AreEqual(message, exception.Message);
			Assert.AreEqual(innerException, exception.InnerException);
		}
	}
}
