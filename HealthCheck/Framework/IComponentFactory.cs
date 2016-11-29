using System.Xml.Linq;
using Quartz;

namespace HealthCheck.Framework
{
    /// <summary>
    /// Provides factory methods for creating plugins based off a type/key.
    /// </summary>
    public interface IComponentFactory
    {
        /// <summary>
        /// Create an instance of a IStatusListener from the MEF assembly catalog
        /// </summary>
        /// <param name="typeName">The name matching export of the plug-in.</param>
        /// <returns>An instance of a health check status listener.</returns>
        /// <exception cref="System.ArgumentNullException">The name of the type is required.</exception>
        IStatusListener GetListener(string typeName);

        /// <summary>
        /// Create an instance of a IHealthCheckPlugin from the MEF assembly catalog
        /// </summary>
        /// <param name="typeName">The name matching export of the plug-in.</param>
        /// <returns>An instance of a health check plug-in.</returns>
        /// <exception cref="System.ArgumentNullException">The name of the type is required.</exception>
        IHealthCheckPlugin GetPlugin(string typeName);

        /// <summary>
        /// Create an instance of a Quartz trigger.
        /// </summary>
        /// <param name="node">The XML node containing the trigger information.</param>
        /// <returns>a Quartz trigger.</returns>
        ITrigger GetTrigger(XElement node);
    }
}
