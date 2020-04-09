using System.Composition;
using NLog;

namespace HealthCheck.Listeneners
{
    /// <summary>
    ///   A Null Listener does not care what the status is.
    /// </summary>
    [Export("NullListener", typeof(IStatusListener))]
    public class NullListener : IStatusListener
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   Gets or sets the threshold of this listener.
        /// </summary>
        public CheckResult Threshold { get; set; }

        /// <summary>
        ///   Perform any startup initialization.
        /// </summary>
        public void Initialize()
        {
            _log.Debug("NullListener Initialized");
        }

        /// <summary>
        ///   Process a health check result.
        /// </summary>
        /// <param name="status">The status of a health check.</param>
        /// <returns><c>true</c> if the listener processed the status; otherwise, <c>false</c></returns>
        public bool Process(IHealthStatus status)
        {
            _log.Debug($"NullListener: {status.Plugin.Name}: {status.StatusCode} - {status.Status}");
            return true;
        }
    }
}
