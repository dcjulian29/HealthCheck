namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Result from Free Memory plugin
    /// </summary>
    public class FreeMemoryStatus : HealthStatus
    {
        /// <summary>
        ///   Gets or sets the free space of memory.
        /// </summary>
        public long FreeSpace { get; set; }

        /// <summary>
        ///   Gets or sets the percentage of free space of memory.
        /// </summary>
        public int FreeSpacePercent { get; set; }

        /// <summary>
        ///   Gets or sets the total size of memory.
        /// </summary>
        public long Size { get; set; }
    }
}
