using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using NUnit.Framework;


namespace DigitalRise.Animation.Tests
{
	[TestFixture]
	public class InvalidAnimationExceptionTest
	{
		[Test]
		public void ConstructorTest()
		{
			var exception = new InvalidAnimationException();
		}


		[Test]
		public void ConstructorTest1()
		{
			const string message = "message";
			var exception = new InvalidAnimationException(message);
			Assert.AreEqual(message, exception.Message);
		}


		[Test]
		public void ConstructorTest2()
		{
			const string message = "message";
			var innerException = new Exception();
			var exception = new InvalidAnimationException(message, innerException);
			Assert.AreEqual(message, exception.Message);
			Assert.AreEqual(innerException, exception.InnerException);
		}
	}
}
