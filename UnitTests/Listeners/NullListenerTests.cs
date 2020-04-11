using System.Diagnostics.CodeAnalysis;
using HealthCheck;
using HealthCheck.Listeners;
using Moq;
using Xunit;

namespace UnitTests.Listeners
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class NullListenerTests
    {
        [Fact]
        public void Process_Should_ReturnTrue_When_StatusIsError()
        {
            // Arrange
            var listener = new NullListener();

            var mock = new Mock<IHealthCheckPlugin>();
            _ = mock.Setup(f => f.Name).Returns("UnitTest Plugin");

            var status = new HealthStatus()
            {
                Plugin = mock.Object,
                Status = CheckResult.Error,
                StatusCode = 404
            };

            // Act
            var actual = listener.Process(status);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void Process_Should_ReturnTrue_When_StatusIsSuccessful()
        {
            // Arrange
            var listener = new NullListener();

            var mock = new Mock<IHealthCheckPlugin>();
            _ = mock.Setup(f => f.Name).Returns("UnitTest Plugin");

            var status = new HealthStatus()
            {
                Plugin = mock.Object,
                Status = CheckResult.Success,
                StatusCode = 200
            };

            // Act
            var actual = listener.Process(status);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void Process_Should_ReturnTrue_When_StatusIsWarning()
        {
            // Arrange
            var listener = new NullListener();

            var mock = new Mock<IHealthCheckPlugin>();
            _ = mock.Setup(f => f.Name).Returns("UnitTest Plugin");

            var status = new HealthStatus()
            {
                Plugin = mock.Object,
                Status = CheckResult.Warning,
                StatusCode = 304
            };

            // Act
            var actual = listener.Process(status);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public void Threshold_Should_BeSuccess_When_InitializeIsCalled()
        {
            // Arrange
            var listener = new NullListener();

            // Act
            listener.Initialize();
            var actual = listener.Threshold;

            // Assert
            Assert.Equal(CheckResult.Success, actual);
        }

        [Fact]
        public void Threshold_Should_DefaultToSuccess()
        {
            // Arrange
            var listener = new NullListener();

            // Act
            var actual = listener.Threshold;

            // Assert
            Assert.Equal(CheckResult.Success, actual);
        }
    }
}
