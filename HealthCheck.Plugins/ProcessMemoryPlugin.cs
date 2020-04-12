using System;
using System.Composition;
using System.Diagnostics;
using System.Xml.Linq;
using NLog;

namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Check The amount of memory being used by a process.
    /// </summary>
    [Export("ProcessMemory", typeof(IHealthCheckPlugin))]
    public class ProcessMemoryPlugin : IHealthCheckPlugin
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
        ///   Gets the memory used threshold failure.
        /// </summary>
        public int MemoryUsedThresholdFailure { get; private set; }

        /// <summary>
        ///   Gets the memory used threshold warning.
        /// </summary>
        public int MemoryUsedThresholdWarning { get; private set; }

        /// <summary>
        ///   Gets or sets the plugin name or description
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the plugin last reported status.
        /// </summary>
        public PluginStatus PluginStatus { get; set; }

        /// <summary>
        ///   Gets or sets the name of the process.
        /// </summary>
        public string ProcessName { get; set; }

        public IHealthStatus DeterminePluginStatus(long spaceUsed)
        {
            var status = new ProcessMemoryStatus
            {
                MemoryUsed = spaceUsed,
                ProcessName = ProcessName
            };

            if (spaceUsed > MemoryUsedThresholdFailure)
            {
                status.Status = CheckResult.Error;
                status.Summary = $"'{ProcessName}' is using more memory than the failure threshold";
            }
            else if (MemoryUsedThresholdWarning > 0 && spaceUsed > MemoryUsedThresholdWarning)
            {
                status.Status = CheckResult.Warning;
                status.Summary = $"'{ProcessName}' is using more memory than the warning threshold";
            }
            else
            {
                status.Status = CheckResult.Success;
                status.Summary = $"'{ProcessName}' is using less memory below thresholds";
            }

            return status;
        }

        /// <summary>
        ///   Executes task(s) to be executed when the health check provided by this plugin is invoked.
        /// </summary>
        /// <returns>The status of the result of the health check.</returns>
        public IHealthStatus Execute()
        {
            var spaceUsed = (long)_performanceCounterActual.NextValue();

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
            ProcessName = configurationElement.Element("ProcessName")?.Value;

            // Warning Default = 25 MB
            MemoryUsedThresholdWarning =
                Convert.ToInt32((configurationElement.Element("ThresholdWarning")?.Value) ?? "26214400");

            // Error Default = 100 MB
            MemoryUsedThresholdFailure =
                Convert.ToInt32((configurationElement.Element("ThresholdFailure")?.Value) ?? "104857600");

            if (string.IsNullOrEmpty(ProcessName))
            {
                throw new MissingRequiredSettingException("Process name is required");
            }
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
                new PerformanceCounter("Process", "Working Set - Private", ProcessName, MachineName);

            PluginStatus = PluginStatus.Idle;
        }
    }
}
