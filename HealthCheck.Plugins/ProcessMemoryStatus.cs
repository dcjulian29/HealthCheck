namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Result from Free Memory plugin
    /// </summary>
    public class ProcessMemoryStatus : HealthStatus
    {
        /// <summary>
        ///   Gets or sets the amount of memory being used by process.
        /// </summary>
        public long MemoryUsed { get; set; }

        /// <summary>
        ///   Gets or sets the amount of memory being used by process in MB.
        /// </summary>
        public long MemoryUsedMB => MemoryUsed / 1048576;

        /// <summary>
        ///   Gets or sets the name of the process.
        /// </summary>
        public string ProcessName { get; set; }
    }
}
