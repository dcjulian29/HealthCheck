namespace HealthCheck
{
    /// <summary>
    /// A status listener listens for and processes the result of an execution of a Health Check Plug-in.
    /// </summary>
    public interface IStatusListener
    {
        /// <summary>
        /// Gets or sets the threshold of this listener.
        /// </summary>
        CheckResult Threshold { get; set; }

        /// <summary>
        /// Perform any startup initialization.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Process a health check result.
        /// </summary>
        bool Process(IHealthStatus status);
    }
}
