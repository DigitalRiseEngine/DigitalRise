using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRise.CommandLine.Tests
{
    [TestFixture]
    public class UnknownArgumentExceptionTest
    {
        [Test]
        public void ConstructorTest0()
        {
            var exception = new UnknownArgumentException();
        }


        [Test]
        public void ConstructorTest1()
        {
            const string argument = "Argument";
            var exception = new UnknownArgumentException(argument);
            Assert.AreEqual(argument, exception.Argument);
        }


        [Test]
        public void ConstructorTest2()
        {
            const string argument = "Argument";
            const string message = "message";
            var exception = new UnknownArgumentException(argument, message);
            Assert.AreEqual(argument, exception.Argument);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest3()
        {
            const string message = "message";
            var innerException = new Exception();
            var exception = new UnknownArgumentException(message, innerException);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorTest4()
        {
            const string argument = "Argument";
            const string message = "message";
            var innerException = new Exception();
            var exception = new UnknownArgumentException(argument, message, innerException);
            Assert.AreEqual(argument, exception.Argument);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new UnknownArgumentException(null));
        }
    }
}
