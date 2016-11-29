using System;
using Common.Logging;
using static System.IO.Directory;

namespace HealthCheck
{
    /// <summary>
    /// Health Check Windows Service
    /// </summary>
    public class HealthService
    {
        private static ILog _log = LogManager.GetLogger<HealthService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class.
        /// </summary>
        public HealthService()
        {
            SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        }

        public void Start()
        {
            _log.Info(m => m("----> HealthCheck Service is Starting..."));
        }

        public void Stop()
        {
            _log.Info(m => m("----> HealthCheck Service is Stoping..."));
        }
    }
}
