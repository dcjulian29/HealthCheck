namespace HealthCheck
{
    /// <summary>
    /// List of possible health check results.
    /// </summary>
    public enum CheckResult
    {
        /// <summary>
        /// The health check has finished executing and returned a successful check. This is similar
        /// to a green light on a stop light.
        /// </summary>
        Success,

        /// <summary>
        /// The health check has finished executing and returned a successful check with at least one
        /// warning. This is similar to a yellow light on a stop light.
        /// </summary>
        Warning,

        /// <summary>
        /// The health check has finished executing and return a failed check. This is similar to a
        /// red light on a stop light.
        /// </summary>
        Error
    }
}
