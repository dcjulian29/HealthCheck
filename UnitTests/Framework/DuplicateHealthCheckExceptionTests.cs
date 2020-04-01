using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using HealthCheck.Framework;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitTests.Framework
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class DuplicateHealthCheckExceptionTests
    {
        [Fact]
        [SuppressMessage(
            "Major Code Smell",
            "S3928:Parameter names used into ArgumentException constructors should match an existing one ",
            Justification = "Testing for the Type in this test")]
        public void Exception_Should_ReturnExpectedException()
        {
            // Arrange & Act
            var actual = new DuplicateHealthCheckException();

            // Assert
            Assert.IsType<DuplicateHealthCheckException>(actual);
        }

        [Fact]
        public void ExceptionFromSerializedObject_Should_ReturnExpectedException()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString();
            var message = "UnitTest";
            var exception = new DuplicateHealthCheckException(message);

            if (File.Exists(guid))
            {
                File.Delete(guid);
            }

            var stream = File.Create(guid);
            var serializer = new BinaryFormatter();
            serializer.Serialize(stream, exception);
            stream.Close();

            // Act
            stream = File.OpenRead(guid);
            var actual = (DuplicateHealthCheckException)serializer.Deserialize(stream);
            stream.Close();

            // Assert
            Assert.Equal(message, actual.Message);
        }

        [Fact]
        public void ExceptionWithInner_Should_ReturnExpectedException()
        {
            // Arrange
            var message = "UnitTest";
            var inner = new ApplicationException(message);

            // Act
            var actual = new DuplicateHealthCheckException(message, inner);

            // Assert
            Assert.IsType<DuplicateHealthCheckException>(actual);
        }

        [Fact]
        public void ExceptionWithMessage_Should_ReturnExpectedException()
        {
            // Arrange
            var message = "UnitTest";

            // Act
            var actual = new DuplicateHealthCheckException(message);

            // Assert
            Assert.Equal(message, actual.Message);
        }
    }
}
