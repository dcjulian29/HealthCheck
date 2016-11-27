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
        /// <param name="status">The status of a health check.</param>
        /// <returns><c>true</c> if the listener processed the status; otherwise, <c>false</c></returns>
        bool Process(IHealthStatus status);
    }
}
