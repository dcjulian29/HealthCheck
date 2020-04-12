using System;
using System.Diagnostics.CodeAnalysis;
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
    public class ProcessorPluginTests
    {
        [Fact]
        public void DeterminPluginStatus_Should_ReturnError_WhenUsageIsAboveErrorThreshold()
        {
            // Arrange
            var plugin = new ProcessorPlugin()
            {
                GroupName = "UnitTest"
            };

            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.DeterminePluginStatus(96);

            // Assert
            Assert.Equal(CheckResult.Error, status.Status);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnSuccess_WhenUsageIsBelowAllThresholds()
        {
            // Arrange
            var plugin = new ProcessorPlugin()
            {
                GroupName = "UnitTest"
            };

            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.DeterminePluginStatus(50);

            // Assert
            Assert.Equal(CheckResult.Success, status.Status);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnWarning_WhenUsageIsAboveWarningThreshold()
        {
            // Arrange
            var plugin = new ProcessorPlugin()
            {
                GroupName = "UnitTest"
            };

            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.DeterminePluginStatus(85);

            // Assert
            Assert.Equal(CheckResult.Warning, status.Status);
        }

        [Fact]
        public void Execute_Should_ReturnAHealthCheckStatus()
        {
            // Arrange
            var plugin = new ProcessorPlugin()
            {
                GroupName = "UnitTest"
            };

            plugin.SetTaskConfiguration(SettingsWithMinimum());

            plugin.Startup();

            // Act
            var status = plugin.Execute();

            // Assert
            Assert.IsAssignableFrom<IHealthStatus>(status);
            plugin.Shutdown();
        }

        [Fact]
        public void MachineName_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new ProcessorPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(Environment.MachineName, plugin.MachineName);
        }

        [Fact]
        public void MachineName_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new ProcessorPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal("NONEXIST", plugin.MachineName);
        }

        [Fact]
        public void ProcessorUsedThresholdFailure_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new ProcessorPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(90, plugin.ProcessorUsedThresholdFailure);
        }

        [Fact]
        public void ProcessorUsedThresholdFailure_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new ProcessorPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(20, plugin.ProcessorUsedThresholdFailure);
        }

        [Fact]
        public void ProcessorUsedThresholdWarning_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new ProcessorPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(75, plugin.ProcessorUsedThresholdWarning);
        }

        [Fact]
        public void ProcessorUsedThresholdWarning_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new ProcessorPlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(40, plugin.ProcessorUsedThresholdWarning);
        }

        [Fact]
        public void Shutdown_Should_ReturnNothing()
        {
            // Arrange
            var plugin = new ProcessorPlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());
            plugin.Startup();

            // Act & Assert
            plugin.Shutdown();
            Assert.IsAssignableFrom<IHealthCheckPlugin>(plugin);
        }

        [Fact]
        public void Startup_Should_SetPluginStatusToIdle()
        {
            // Arrange
            var plugin = new ProcessorPlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            plugin.Startup();

            // Assert
            Assert.Equal(PluginStatus.Idle, plugin.PluginStatus);
        }

        private XElement SettingsWithEverything()
        {
            var xml = new XElement("Settings");
            xml.Add(new XElement("MachineName", "NONEXIST"));
            xml.Add(new XElement("ThresholdWarning", "40"));
            xml.Add(new XElement("ThresholdFailure", "20"));

            return xml;
        }

        private XElement SettingsWithMinimum()
        {
            return new XElement("Settings");
        }
    }
}
