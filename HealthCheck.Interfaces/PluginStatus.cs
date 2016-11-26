namespace HealthCheck
{
    /// <summary>
    /// Status of the plug-in.
    /// </summary>
    public enum PluginStatus
    {
        /// <summary>
        /// The plug-in is initializing.
        /// </summary>
        Init,

        /// <summary>
        /// The plug-in has completed initializing but has not executed.
        /// </summary>
        Idle,

        /// <summary>
        /// The plug-in is executing but has not completed.
        /// </summary>
        Executing,

        /// <summary>
        /// The plug-in failed to complete the check due to an exception that could not be handled or
        /// was unexpected.
        /// </summary>
        TaskExecutionFailure
    }
}
