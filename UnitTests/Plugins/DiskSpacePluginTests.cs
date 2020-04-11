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
    public class DiskSpacePluginTests
    {
        [Fact]
        public void DeterminPluginStatus_Should_ReturnCorrectDriveSpace()
        {
            // Arrange
            var plugin = new DiskSpacePlugin()
            {
                GroupName = "UnitTest"
            };

            // Act
            var status = plugin.DeterminePluginStatus(50, 256000) as DiskSpaceStatus;

            // Assert
            Assert.Equal(500, status.Size);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnCorrectFreeSpacePercentage()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            var status = plugin.DeterminePluginStatus(50, 256) as DiskSpaceStatus;

            // Assert
            Assert.Equal(50, status.FreeSpacePercent);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnError_WhenFreeSpaceIsBelowErrorThreshold()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.DeterminePluginStatus(5, 256);

            // Assert
            Assert.Equal(CheckResult.Error, status.Status);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnFreeSpace()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            var status = plugin.DeterminePluginStatus(50, 262144) as DiskSpaceStatus;

            // Assert
            Assert.Equal(256, status.FreeSpace);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnSuccess_WhenFreeSpaceIsAboveAllThresholds()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.DeterminePluginStatus(50, 256);

            // Assert
            Assert.Equal(CheckResult.Success, status.Status);
        }

        [Fact]
        public void DeterminPluginStatus_Should_ReturnWarning_WhenFreeSpaceIsAboveWarningThreshold()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            var status = plugin.DeterminePluginStatus(25, 256);

            // Assert
            Assert.Equal(CheckResult.Warning, status.Status);
        }

        [Fact]
        public void Drive_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal("C:", plugin.Drive);
        }

        [Fact]
        public void Drive_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal("D:", plugin.Drive);
        }

        [Fact]
        public void Execute_Should_ReturnAHealthCheckStatus()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());
            plugin.Startup();

            // Act
            var status = plugin.Execute();

            // Assert
            Assert.IsAssignableFrom<IHealthStatus>(status);
            plugin.Shutdown();
        }

        [Fact]
        public void FreeSpacePercentThresholdFailure_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(10, plugin.FreeSpacePercentThresholdFailure);
        }

        [Fact]
        public void FreeSpacePercentThresholdFailure_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(20, plugin.FreeSpacePercentThresholdFailure);
        }

        [Fact]
        public void FreeSpacePercentThresholdWarning_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(30, plugin.FreeSpacePercentThresholdWarning);
        }

        [Fact]
        public void FreeSpacePercentThresholdWarning_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal(40, plugin.FreeSpacePercentThresholdWarning);
        }

        [Fact]
        public void MachineName_Should_ExpectedValue_When_SettingIsNotProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Assert
            Assert.Equal(Environment.MachineName, plugin.MachineName);
        }

        [Fact]
        public void MachineName_Should_ExpectedValue_When_SettingIsProvided()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();

            // Act
            plugin.SetTaskConfiguration(SettingsWithEverything());

            // Assert
            Assert.Equal("NONEXIST", plugin.MachineName);
        }

        [Fact]
        public void Shutdown_Should_ReturnNothing()
        {
            // Arrange
            var plugin = new DiskSpacePlugin();
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
            var plugin = new DiskSpacePlugin();
            plugin.SetTaskConfiguration(SettingsWithMinimum());

            // Act
            plugin.Startup();

            // Assert
            Assert.Equal(PluginStatus.Idle, plugin.PluginStatus);
        }

        private XElement SettingsWithEverything()
        {
            var xml = new XElement("Settings");
            xml.Add(new XElement("Drive", "D:"));
            xml.Add(new XElement("MachineName", "NONEXIST"));
            xml.Add(new XElement("ThresholdWarning", "40"));
            xml.Add(new XElement("ThresholdFailure", "20"));

            return xml;
        }

        private XElement SettingsWithMinimum()
        {
            var xml = new XElement("Settings");

            return xml;
        }
    }
}
