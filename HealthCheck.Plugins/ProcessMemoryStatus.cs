using System;

namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Result from Free Memory plugin
    /// </summary>
    public class ProcessMemoryStatus : IHealthStatus
    {
        /// <summary>
        ///   Gets or sets the details of the result of the health check
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        ///   Gets or sets the instance id of the component being health checked.
        /// </summary>
        public int InstanceId { get; set; }

        /// <summary>
        ///   Gets or sets the amount of memory being used by process.
        /// </summary>
        public long MemoryUsed { get; set; }

        /// <summary>
        ///   Gets or sets the amount of memory being used by process in MB.
        /// </summary>
        public long MemoryUsedMB => MemoryUsed / 1048576;

        /// <summary>
        ///   Gets or sets the plugin that generated the status message
        /// </summary>
        public IHealthCheckPlugin Plugin { get; set; }

        /// <summary>
        ///   Gets or sets the name of the process.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        ///   Gets or sets the status of the health check.
        /// </summary>
        public CheckResult Status { get; set; }

        /// <summary>
        ///   Gets or sets the status code of the result of the health check.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        ///   Gets or sets the summary of the result of the health check.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        ///   Gets or sets the date and time of the health check status.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}
