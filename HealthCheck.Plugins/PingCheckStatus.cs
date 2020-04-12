using System;

namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Result from Ping Check plugin
    /// </summary>
    public class PingCheckStatus : HealthStatus
    {
        /// <summary>
        ///   Gets or sets the response time, if any.
        /// </summary>
        public long ResponseTime { get; set; }
    }
}
