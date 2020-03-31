using System.Collections.Generic;
using System.Xml.Linq;
using Quartz;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   Represents a group of health check tasks. Shares common schedules and listeners.
    /// </summary>
    public class HealthCheckGroup
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthCheckGroup" /> class.
        /// </summary>
        public HealthCheckGroup()
        {
            Checks = new List<IHealthCheckJob>();
        }

        /// <summary>
        ///   Gets or sets the list of health check jobs in this group.
        /// </summary>
        public List<IHealthCheckJob> Checks { get; set; }

        /// <summary>
        ///   Gets or sets the configuration settings for this group of health checks.
        /// </summary>
        public XElement ConfigurationNode { get; set; }

        /// <summary>
        ///   Gets or sets the name of the group of health checks.
        /// </summary>
        public string Name { get; set; }
    }
}
