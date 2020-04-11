using System;
using System.Diagnostics.CodeAnalysis;
using HealthCheck;
using HealthCheck.Plugins;
using Moq;
using Xunit;

namespace UnitTests.Plugins
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class ProcessMemoryStatusTests
    {
        [Fact]
        public void Details_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal("Status Details", status.Details);
        }

        [Fact]
        public void InstanceId_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal(1, status.InstanceId);
        }

        [Fact]
        public void MemoryUsed_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal(24117248, status.MemoryUsed);
        }

        [Fact]
        public void MemoryUsedMB_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal(23, status.MemoryUsedMB);
        }

        [Fact]
        public void Plugin_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.IsAssignableFrom<IHealthCheckPlugin>(status.Plugin);
        }

        [Fact]
        public void ProcessName_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal("ProcessA", status.ProcessName);
        }

        [Fact]
        public void Status_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal(CheckResult.Success, status.Status);
        }

        [Fact]
        public void StatusCode_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal(101, status.StatusCode);
        }

        [Fact]
        public void Summary_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal("Status Summary", status.Summary);
        }

        [Fact]
        public void Timestamp_Should_ReturnCorrectValue()
        {
            // Arrange & Act
            var status = CreateHealthStatus();

            // Assert
            Assert.Equal(DateTime.MinValue, status.Timestamp);
        }

        private ProcessMemoryStatus CreateHealthStatus()
        {
            var mock = new Mock<IHealthCheckPlugin>();
            _ = mock.Setup(f => f.Name).Returns("UnitTest Plugin");

            return new ProcessMemoryStatus()
            {
                Details = "Status Details",
                InstanceId = 1,
                MemoryUsed = 24117248,
                Plugin = mock.Object,
                ProcessName = "ProcessA",
                Status = CheckResult.Success,
                StatusCode = 101,
                Summary = "Status Summary",
                Timestamp = DateTime.MinValue
            };
        }
    }
}
