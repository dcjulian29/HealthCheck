namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Result from Processor plugin
    /// </summary>
    public class ProcessorStatus : HealthStatus
    {
        /// <summary>
        ///   Gets or sets the percent of the processor being used by all processes.
        /// </summary>
        public int ProcessorPercentUsed { get; set; }
    }
}
