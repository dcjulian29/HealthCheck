using System;
using System.Composition;
using System.Diagnostics;
using System.Xml.Linq;
using NLog;

namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Check free space on a disk.
    /// </summary>
    [Export("DiskSpace", typeof(IHealthCheckPlugin))]
    public class DiskSpacePlugin : IHealthCheckPlugin
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private PerformanceCounter _performanceCounterActual;
        private PerformanceCounter _performanceCounterPercent;

        /// <summary>
        ///   Gets the drive to check
        /// </summary>
        public string Drive { get; private set; }

        /// <summary>
        ///   Gets the free space percent threshold failure.
        /// </summary>
        public int FreeSpacePercentThresholdFailure { get; private set; }

        /// <summary>
        ///   Gets the free space percent threshold warning.
        /// </summary>
        public int FreeSpacePercentThresholdWarning { get; private set; }

        /// <summary>
        ///   Gets or sets the associated health check group
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        ///   Gets the name of the machine.
        /// </summary>
        public string MachineName { get; private set; }

        /// <summary>
        ///   Gets or sets the plugin name or description
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the plugin last reported status.
        /// </summary>
        public PluginStatus PluginStatus { get; set; }

        public IHealthStatus DeterminePluginStatus(float freeSpacePercentage, float freeMegaBytes)
        {
            var status = new DiskSpaceStatus();

            if (freeSpacePercentage <= FreeSpacePercentThresholdFailure)
            {
                status.Status = CheckResult.Error;
                status.Summary = "Drive free space lower than the failure threshold";
            }
            else if (FreeSpacePercentThresholdWarning > 0 && freeSpacePercentage <= FreeSpacePercentThresholdWarning)
            {
                status.Status = CheckResult.Warning;
                status.Summary = "Drive free space lower than the warning threshold";
            }
            else
            {
                status.Status = CheckResult.Success;
                status.Summary = "Drive free space is above thresholds";
            }

            status.Size = (long)((freeMegaBytes / freeSpacePercentage) * 100) / 1024;
            status.FreeSpace = (long)(freeMegaBytes / 1024);
            status.FreeSpacePercent = (int)freeSpacePercentage;

            return status;
        }

        /// <summary>
        ///   Executes task(s) to be executed when the health check provided by this plugin is invoked.
        /// </summary>
        /// <returns>The status of the result of the health check.</returns>
        public IHealthStatus Execute()
        {
            var freeSpacePercentage = _performanceCounterPercent.NextValue();
            var freeMegaBytes = _performanceCounterActual.NextValue();

            return DeterminePluginStatus(freeSpacePercentage, freeMegaBytes);
        }

        /// <summary>
        ///   Load up configuration settings from an XML fragment
        /// </summary>
        /// <param name="configurationElement">
        ///   XML Fragment containing configuration information for the plugin
        /// </param>
        public void SetTaskConfiguration(XElement configurationElement)
        {
            _log.Debug($"Plugin '{Name}' is setting configuration...");

            Drive = configurationElement.Element("Drive")?.Value ?? "C:";
            MachineName = configurationElement.Element("MachineName")?.Value ?? Environment.MachineName;
            FreeSpacePercentThresholdWarning =
                Convert.ToInt32((configurationElement.Element("ThresholdWarning")?.Value) ?? "30");
            FreeSpacePercentThresholdFailure =
                Convert.ToInt32((configurationElement.Element("ThresholdFailure")?.Value) ?? "10");
        }

        /// <summary>
        ///   Shuts down this instance.
        /// </summary>
        public void Shutdown()
        {
            _log.Debug($"Plugin '{Name}' is shutting down...");

            _performanceCounterPercent.Close();
            _performanceCounterPercent.Dispose();
            _performanceCounterPercent = null;
            _performanceCounterActual.Close();
            _performanceCounterActual.Dispose();
            _performanceCounterActual = null;
        }

        /// <summary>
        ///   Start the plugin, allowing it to initialize before being executed by the framework scheduler.
        /// </summary>
        public void Startup()
        {
            _log.Debug($"Plugin '{Name}' is starting...");

            _performanceCounterPercent =
                new PerformanceCounter("LogicalDisk", "% Free Space", Drive, MachineName);

            _performanceCounterActual =
                new PerformanceCounter("LogicalDisk", "Free Megabytes", Drive, MachineName);

            PluginStatus = PluginStatus.Idle;
        }
    }
}
