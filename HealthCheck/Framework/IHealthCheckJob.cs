using System.Collections.Generic;
using Quartz;

namespace HealthCheck.Framework
{
    /// <summary>
    /// This interface represent the concept of health check job. It consist of a configuration, a
    /// plug-in, one or more listeners that respond to the health check, and one or more triggers
    /// that tell the health check framework how to execute the health check job.
    /// </summary>
    public interface IHealthCheckJob
    {
        /// <summary>
        /// Gets or sets the ID of this health check job.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the configuration for this health check plug-in.
        /// </summary>
        JobConfiguration JobConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the list of listeners that respond to the health check. to the result of a
        /// health check.
        /// </summary>
        List<IStatusListener> Listeners { get; set; }

        /// <summary>
        /// Gets or sets the health check plug-in to run.
        /// </summary>
        IHealthCheckPlugin Plugin { get; set; }

        /// <summary>
        /// Gets or sets the list of triggers that tell the health check framework when to execute
        /// the health check job.
        /// </summary>
        List<ITrigger> Triggers { get; set; }

        /// <summary>
        /// Write out any notifications to listeners.
        /// </summary>
        /// <param name="status">The result of a health check execution.</param>
        void NotifyListeners(IHealthStatus status);
    }
}
