using System;

namespace HealthCheck
{
    /// <summary>
    /// The result of an execution of the health check.
    /// </summary>
    public interface IHealthStatus
    {
        /// <summary>
        /// Gets or sets the details of the result of the health check
        /// </summary>
        string Details { get; set; }

        /// <summary>
        /// Gets or sets the instance id of the component being health checked.
        /// </summary>
        int InstanceId { get; set; }

        /// <summary>
        /// Gets or sets the plugin that generated the status message
        /// </summary>
        IHealthCheckPlugin Plugin { get; set; }

        /// <summary>
        /// Gets or sets the status of the health check.
        /// </summary>
        CheckResult Status { get; set; }

        /// <summary>
        /// Gets or sets the status code of the result of the health check.
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the summary of the result of the health check.
        /// </summary>
        string Summary { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the health check status.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
