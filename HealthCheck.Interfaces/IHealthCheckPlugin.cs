using System.Xml.Linq;

namespace HealthCheck
{
    /// <summary>
    /// A Health Check Plugin contains code that executes a check and returns a status to the
    /// framework running the plugin.
    /// </summary>
    public interface IHealthCheckPlugin
    {
        /// <summary>
        /// Gets or sets the associated health check group
        /// </summary>
        string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the plugin name or description
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the node id
        /// </summary>
        int NodeId { get; set; }

        /// <summary>
        /// Gets or sets the plugin last reported status.
        /// </summary>
        PluginStatus PluginStatus { get; set; }

        /// <summary>
        /// Executes task(s) to be executed when the health check provided by this plugin is invoked.
        /// </summary>
        /// <returns>The status of the result of the health check.</returns>
        IHealthStatus Execute();

        /// <summary>
        /// Load up configuration settings from an XML fragment
        /// </summary>
        /// <param name="configurationElement"></param>
        void SetTaskConfiguration(XElement configurationElement);

        /// <summary>
        /// Shut down the plugin, allowing it to clean up any resources before being unloaded from memory.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Start the plugin, allowing it to initialize before being executed by the framework scheduler.
        /// </summary>
        void Startup();
    }
}
