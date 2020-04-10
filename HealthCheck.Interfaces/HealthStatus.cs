using System;

namespace HealthCheck
{
    /// <summary>
    ///   The result of an execution of the health check.
    /// </summary>
    public class HealthStatus : IHealthStatus
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthStatus" /> class.
        /// </summary>
        public HealthStatus()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        ///   Gets or sets the details of the result of the health check
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        ///   Gets or sets the instance id of the component being health checked.
        /// </summary>
        public int InstanceId { get; set; }

        /// <summary>
        ///   Gets or sets the plugin that generated the status message
        /// </summary>
        public IHealthCheckPlugin Plugin { get; set; }

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
