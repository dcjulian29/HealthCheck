using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using HealthCheck;
using HealthCheck.Plugins;
using Xunit;

namespace UnitTests.Plugins
{
    [SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1600:ElementsMustBeDocumented",
        Justification = "Test Suites do not need XML Documentation.")]
    public class PingCheckTests
    {
        [Fact]
        public void Execute_Should_ReturnError_When_PingingHostThatDoesNotExist()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            var xml = new XElement("Settings");
            xml.Add(new XElement("HostName", "169.254.0.1"));
            xml.Add(new XElement("Retries", "1"));

            plugin.SetTaskConfiguration(xml);

            // Act
            var status = plugin.Execute();

            // Assert
            Assert.Equal(CheckResult.Error, status.Status);
        }

        [Fact]
        public void Execute_Should_ReturnError_When_PingingInvalidHost()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());
            plugin.HostName = "Invalid";

            // Act
            var status = plugin.Execute();

            // Assert
            Assert.Equal(CheckResult.Error, status.Status);
        }

        [Fact]
        public void Execute_Should_ReturnPluginStatusToIdle()
        {
            // Arrange
            var plugin = CreatePingCheck();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            _ = plugin.Execute();

            // Assert
            Assert.Equal(PluginStatus.Idle, plugin.PluginStatus);
        }

        [Fact]
        public void Execute_Should_ReturnSuccess_When_PingingLocalHost()
        {
            // Arrange
            var plugin = CreatePingCheck();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.Execute();

            // Assert
            Assert.Equal(CheckResult.Success, status.Status);
        }

        [Fact]
        public void Execute_Should_SetPluginStatusFailed_When_PingingInvalidHost()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());
            plugin.HostName = "Invalid";

            // Act
            _ = plugin.Execute();

            // Assert
            Assert.Equal(PluginStatus.TaskExecutionFailure, plugin.PluginStatus);
        }

        [Fact]
        public void HostName_Should_ReturnWhatItWasSetToDuringCreation()
        {
            // Arrange
            var plugin = CreatePingCheck();

            // Act
            var actual = plugin.HostName;

            // Assert
            Assert.Equal("127.0.0.1", actual);
        }

        [Fact]
        public void ProcessPingResponse_Should_ReturnError_When_ErrorResponse()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Act
            var status = plugin.ProcessPingResponse(IPStatus.Success, 3500);

            // Assert
            Assert.Equal(CheckResult.Error, status.Status);
        }

        [Fact]
        public void ProcessPingResponse_Should_ReturnError_When_PingFailed()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Act
            var status = plugin.ProcessPingResponse(IPStatus.Unknown, 0);

            // Assert
            Assert.Equal(CheckResult.Error, status.Status);
        }

        [Fact]
        public void ProcessPingResponse_Should_ReturnSuccess_When_GoodResponse()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            var status = plugin.ProcessPingResponse(IPStatus.Success, 1);

            // Assert
            Assert.Equal(CheckResult.Success, status.Status);
        }

        [Fact]
        public void ProcessPingResponse_Should_ReturnWarning_When_WarnResponse()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Act
            var status = plugin.ProcessPingResponse(IPStatus.Success, 501);

            // Assert
            Assert.Equal(CheckResult.Warning, status.Status);
        }

        [Fact]
        public void ResponseTimeError_Should_ReturnCorrectValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(1500, plugin.ResponseTimeError);
        }

        [Fact]
        public void ResponseTimeError_Should_ReturnDefault_When_SettingDoesNotExist()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(3000, plugin.ResponseTimeError);
        }

        [Fact]
        public void ResponseTimeWarn_Should_ReturnCorrectValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(250, plugin.ResponseTimeWarn);
        }

        [Fact]
        public void ResponseTimeWarn_Should_ReturnDefault_When_SettingDoesNotExist()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(500, plugin.ResponseTimeWarn);
        }

        [Fact]
        public void Retries_Should_ReturnCorrectValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(10, plugin.Retries);
        }

        [Fact]
        public void Retries_Should_ReturnDefault_When_SettingDoesNotExist()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(3, plugin.Retries);
        }

        [Fact]
        public void RetryDelay_Should_ReturnCorrectValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(5000, plugin.RetryDelay);
        }

        [Fact]
        public void RetryDelay_Should_ReturnDefault_When_SettingDoesNotExist()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(2500, plugin.RetryDelay);
        }

        [Fact]
        public void SetTaskConfiguration_Should_ThrowException_When_HostNameIsMissing()
        {
            // Arrange
            var plugin = new PingCheckPlugin();
            var xml = new XElement("Settings");

            // Act & Assert
            Assert.Throws<MissingRequiredSettingException>(() => plugin.SetTaskConfiguration(xml));
        }

        [Fact]
        public void Shutdown_Should_ReturnNothing()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act & Assert
            plugin.Shutdown();
            Assert.IsAssignableFrom<IHealthCheckPlugin>(plugin);
        }

        [Fact]
        public void Startup_Should_SetPluginStatusToIdle()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.Startup();

            // Assert
            Assert.Equal(PluginStatus.Idle, plugin.PluginStatus);
        }

        [Fact]
        public void TimeOut_Should_ReturnCorrectValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(200, plugin.TimeOut);
        }

        [Fact]
        public void TimeOut_Should_ReturnDefault_When_SettingDoesNotExist()
        {
            // Arrange
            var plugin = new PingCheckPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(5000, plugin.TimeOut);
        }

        private PingCheckPlugin CreatePingCheck()
        {
            return new PingCheckPlugin()
            {
                GroupName = "UnitTest",
                HostName = "127.0.0.1"
            };
        }

        private XElement SettingsWithEverything()
        {
            var xml = new XElement("Settings");
            xml.Add(new XElement("HostName", "127.0.0.1"));
            xml.Add(new XElement("Retries", "10"));
            xml.Add(new XElement("RetryDelay", "5000"));
            xml.Add(new XElement("TimeOut", "200"));
            xml.Add(new XElement("ResponseTimeWarn", "250"));
            xml.Add(new XElement("ResponseTimeError", "1500"));

            return xml;
        }

        private XElement SettingsWithMinimum()
        {
            var xml = new XElement("Settings");
            xml.Add(new XElement("HostName", "127.0.0.1"));

            return xml;
        }
    }
}
