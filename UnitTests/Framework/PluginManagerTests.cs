using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using HealthCheck;
using HealthCheck.Framework;
using Moq;
using Quartz;
using Xunit;

namespace UnitTests.Framework
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class PluginManagerTests
    {
        [Fact]
        public void InitializeCheckJob_Should_CreatePingCheckJobWithListener()
        {
            // Arrange
            var config = new JobConfiguration
            {
                Type = "Ping"
            };

            var xml = new XElement("Listener");
            xml.SetAttributeValue("Type", "LogFile");
            xml.SetAttributeValue("Threshold", "Warning");
            config.Listeners.Add(xml);

            var job = new HealthCheckJob
            {
                JobConfiguration = config
            };

            var mock = new Mock<IComponentFactory>();
            _ = mock.Setup(f => f.GetPlugin("Ping")).Returns(new Mock<IHealthCheckPlugin>().Object);
            _ = mock.Setup(f => f.GetListener(It.IsAny<string>())).Returns(new Mock<IStatusListener>().Object);

            var manager = new PluginManager(mock.Object);
            var group = new HealthCheckGroup
            {
                Name = "PROD"
            };

            // Act
            _ = manager.InitializeCheckJob(job, group);

            // Assert
            _ = Assert.Single(job.Listeners);
        }

        [Fact]
        public void InitializeCheckJob_Should_CreatePingCheckJobWithNoListeners()
        {
            // Arrange
            var config = new JobConfiguration
            {
                Type = "Ping"
            };

            var job = new HealthCheckJob
            {
                JobConfiguration = config
            };

            var mock = new Mock<IComponentFactory>();
            _ = mock.Setup(f => f.GetPlugin("Ping")).Returns(new Mock<IHealthCheckPlugin>().Object);
            _ = mock.Setup(f => f.GetListener(It.IsAny<string>())).Returns(new Mock<IStatusListener>().Object);

            var manager = new PluginManager(mock.Object);
            var group = new HealthCheckGroup
            {
                Name = "PROD"
            };

            // Act
            _ = manager.InitializeCheckJob(job, group);

            // Assert
            Assert.Empty(job.Listeners);
        }

        [Fact]
        public void InitializeCheckJob_Should_CreatePingCheckJobWithNoTriggers()
        {
            // Arrange
            var config = new JobConfiguration
            {
                Type = "Ping"
            };

            var job = new HealthCheckJob
            {
                JobConfiguration = config
            };

            var mock = new Mock<IComponentFactory>();
            _ = mock.Setup(f => f.GetPlugin("Ping")).Returns(new Mock<IHealthCheckPlugin>().Object);
            _ = mock.Setup(f => f.GetTrigger(It.IsAny<XElement>())).Returns(new Mock<ITrigger>().Object);

            var manager = new PluginManager(mock.Object);
            var group = new HealthCheckGroup
            {
                Name = "PROD"
            };

            // Act
            _ = manager.InitializeCheckJob(job, group);

            // Assert
            Assert.Empty(job.Triggers);
        }

        [Fact]
        public void InitializeCheckJob_Should_CreatePingCheckJobWithTrigger()
        {
            // Arrange
            var config = new JobConfiguration
            {
                Type = "Ping"
            };

            var xml = new XElement("Trigger");
            xml.SetAttributeValue("Type", "simple");
            xml.SetAttributeValue("Repeat", "10");

            config.Triggers.Add(xml);

            var job = new HealthCheckJob
            {
                JobConfiguration = config
            };

            var mock = new Mock<IComponentFactory>();
            _ = mock.Setup(f => f.GetPlugin("Ping")).Returns(new Mock<IHealthCheckPlugin>().Object);
            _ = mock.Setup(f => f.GetTrigger(It.IsAny<XElement>())).Returns(new Mock<ITrigger>().Object);

            var manager = new PluginManager(mock.Object);
            var group = new HealthCheckGroup
            {
                Name = "PROD"
            };

            // Act
            _ = manager.InitializeCheckJob(job, group);

            // Assert
            _ = Assert.Single(job.Triggers);
        }

        [Fact]
        public void InitializeCheckJob_Should_CreatePingCheckJobWithTriggerAndListener()
        {
            // Arrange
            var config = new JobConfiguration
            {
                Type = "Ping"
            };

            var xml = new XElement("Listener");
            xml.SetAttributeValue("Type", "LogFile");
            xml.SetAttributeValue("Threshold", "Warning");
            config.Listeners.Add(xml);

            xml = new XElement("Trigger");
            xml.SetAttributeValue("Type", "simple");
            xml.SetAttributeValue("Repeat", "10");
            config.Triggers.Add(xml);

            var job = new HealthCheckJob
            {
                JobConfiguration = config
            };

            var mock = new Mock<IComponentFactory>();
            _ = mock.Setup(f => f.GetPlugin("Ping")).Returns(new Mock<IHealthCheckPlugin>().Object);
            _ = mock.Setup(f => f.GetListener(It.IsAny<string>())).Returns(new Mock<IStatusListener>().Object);
            _ = mock.Setup(f => f.GetTrigger(It.IsAny<XElement>())).Returns(new Mock<ITrigger>().Object);

            var manager = new PluginManager(mock.Object);
            var group = new HealthCheckGroup
            {
                Name = "PROD"
            };

            // Act
            _ = manager.InitializeCheckJob(job, group);

            // Assert
            Assert.True(job.Triggers.Any() && job.Listeners.Any());
        }
    }
}
