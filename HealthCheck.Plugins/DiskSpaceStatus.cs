namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Result from Disk Space plugin
    /// </summary>
    public class DiskSpaceStatus : HealthStatus
    {
        /// <summary>
        ///   Gets or sets the free space of the disk.
        /// </summary>
        public long FreeSpace { get; set; }

        /// <summary>
        ///   Gets or sets the percentage of free space on the disk.
        /// </summary>
        public int FreeSpacePercent { get; set; }

        /// <summary>
        ///   Gets or sets the total size of the disk.
        /// </summary>
        public long Size { get; set; }
    }
}
