using System;
using System.Composition;
using System.Diagnostics;
using System.Xml.Linq;
using NLog;

namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Check The amount of CPU being used by a machine.
    /// </summary>
    [Export("Processor", typeof(IHealthCheckPlugin))]
    public class ProcessorPlugin : IHealthCheckPlugin
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private PerformanceCounter _performanceCounterActual;

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

        /// <summary>
        ///   Gets the processor used threshold failure.
        /// </summary>
        public int ProcessorUsedThresholdFailure { get; private set; }

        /// <summary>
        ///   Gets the processor used threshold warning.
        /// </summary>
        public int ProcessorUsedThresholdWarning { get; private set; }

        public IHealthStatus DeterminePluginStatus(int percentUsed)
        {
            var status = new ProcessorStatus
            {
                ProcessorPercentUsed = percentUsed
            };

            if (percentUsed > ProcessorUsedThresholdFailure)
            {
                status.Status = CheckResult.Error;
                status.Summary = $"'{MachineName}' is using more processor than the failure threshold";
            }
            else if (ProcessorUsedThresholdWarning > 0 && percentUsed > ProcessorUsedThresholdWarning)
            {
                status.Status = CheckResult.Warning;
                status.Summary = $"'{MachineName}' is using more processor than the warning threshold";
            }
            else
            {
                status.Status = CheckResult.Success;
                status.Summary = $"'{MachineName}' is using the processor below thresholds";
            }

            return status;
        }

        /// <summary>
        ///   Executes task(s) to be executed when the health check provided by this plugin is invoked.
        /// </summary>
        /// <returns>The status of the result of the health check.</returns>
        public IHealthStatus Execute()
        {
            var spaceUsed = (int)_performanceCounterActual.NextValue();

            return DeterminePluginStatus(spaceUsed);
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

            MachineName = configurationElement.Element("MachineName")?.Value ?? Environment.MachineName;

            // Warning Default = 75%
            ProcessorUsedThresholdWarning =
                Convert.ToInt32((configurationElement.Element("ThresholdWarning")?.Value) ?? "75");

            // Error Default = 90%
            ProcessorUsedThresholdFailure =
                Convert.ToInt32((configurationElement.Element("ThresholdFailure")?.Value) ?? "90");
        }

        /// <summary>
        ///   Shuts down this instance.
        /// </summary>
        public void Shutdown()
        {
            _log.Debug($"Plugin '{Name}' is shutting down...");

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

            _performanceCounterActual =
                new PerformanceCounter("Processor", "% Processor Time", "_Total", MachineName);

            PluginStatus = PluginStatus.Idle;
        }
    }
}
