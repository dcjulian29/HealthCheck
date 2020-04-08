using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HealthCheck.Framework;
using NLog;
using Quartz;

namespace HealthCheck
{
    /// <summary>
    ///   Used to load and initialize the health checks as defined in configuration
    /// </summary>
    public class PluginManager
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IComponentFactory _factory;

        /// <summary>
        ///   Initializes a new instance of the <see cref="PluginManager" /> class.
        /// </summary>
        /// <param name="factory">The component factory to use.</param>
        public PluginManager(IComponentFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        ///   Create status listeners based off the health check configuration.
        /// </summary>
        /// <param name="configuration">The configuration for the health check.</param>
        /// <returns>A collection of listeners to receive health check results.</returns>
        public List<IStatusListener> CreateListeners(JobConfiguration configuration)
        {
            var listeners = new List<IStatusListener>();

            if (!configuration.Listeners.Any())
            {
                return listeners;
            }

            foreach (var listenerXml in configuration.Listeners)
            {
                var type = ReadAttribute(listenerXml, "Type");
                var listener = _factory.GetListener(type);

                _ = Enum.TryParse(ReadAttribute(listenerXml, "Threshold"), true, out CheckResult threshold);

                listener.Threshold = threshold;

                listener.Initialize();

                listeners.Add(listener);
            }

            return listeners;
        }

        /// <summary>
        ///   Create Quartz triggers based off the health check configuration
        /// </summary>
        /// <param name="configuration">The configuration settings for one health check job.</param>
        /// <returns>A collection of Quartz triggers.</returns>
        public List<ITrigger> CreateTriggers(JobConfiguration configuration)
        {
            var triggers = new List<ITrigger>();

            if (!configuration.Triggers.Any())
            {
                return triggers;
            }

            triggers.AddRange(
                configuration.Triggers.Select(
                    triggerXml => _factory.GetTrigger(triggerXml)).Where(trigger => trigger != null));

            return triggers;
        }

        /// <summary>
        ///   Create the Quartz job based on configuration settings
        /// </summary>
        /// <param name="job">The health check job.</param>
        /// <param name="group">The group this health check job belongs to.</param>
        /// <returns>The Quartz job.</returns>
        public IHealthCheckJob InitializeCheckJob(IHealthCheckJob job, HealthCheckGroup @group)
        {
            _log.Debug("Initializing job: " + job.JobConfiguration.Name);

            var checkConfiguration = job.JobConfiguration;
            var plugin = _factory.GetPlugin(checkConfiguration.Type);

            plugin.Name = checkConfiguration.Name;
            plugin.GroupName = @group.Name;

            job.Plugin = plugin;

            if (checkConfiguration.Settings != null)
            {
                plugin.SetTaskConfiguration(checkConfiguration.Settings);
            }

            job.Listeners = CreateListeners(checkConfiguration);
            job.Triggers = CreateTriggers(checkConfiguration);

            plugin.Startup();

            return job;
        }

        private string ReadAttribute(XElement node, string name)
        {
            return node.Attributes().First(a => a.Name.LocalName == name).Value;
        }
    }
}
