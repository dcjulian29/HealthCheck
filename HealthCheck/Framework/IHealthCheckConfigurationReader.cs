using System.Collections.Generic;

namespace HealthCheck.Framework
{
    /// <summary>
    /// This interface allows for a pluggable provider that is responsible for loading and parsing
    /// health check configuration files.
    /// </summary>
    public interface IHealthCheckConfigurationReader
    {
        /// <summary>
        /// Load and read a configuration file located in the 'configuration' folder and builds a
        /// list of groups containing health checks in the processed configuration file.
        /// </summary>
        /// <param name="configurationFile">The configuration file path.</param>
        /// <returns>
        /// A list of groups containing health checks in the processed configuration file.
        /// </returns>
        /// <exception cref="System.ApplicationException">
        /// Occurs when a duplicate group name is used
        /// </exception>
        List<HealthCheckGroup> Read(string configurationFile);

        /// <summary>
        /// Read the loaded configuration file and builds a list of groups containing health checks
        /// in the processed configuration file.
        /// </summary>
        /// <returns>
        /// A list of groups containing health checks in the processed configuration file.
        /// </returns>
        /// <exception cref="System.ApplicationException">
        /// Occurs when a duplicate group name is used
        /// </exception>
        List<HealthCheckGroup> ReadAll();
    }
}
