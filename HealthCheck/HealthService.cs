using System;
using System.Collections.Generic;
using System.IO;
using Common.Logging;
using HealthCheck.Framework;

namespace HealthCheck
{
    /// <summary>
    /// Health Check Windows Service
    /// </summary>
    public class HealthService
    {
        private static ILog _log = LogManager.GetLogger<HealthService>();
        private PluginManager _manager;
        private IConfigReader _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class.
        /// </summary>
        public HealthService()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            _reader = new ConfigurationReader();
            _manager = new PluginManager(null);
            Groups = new List<HealthCheckGroup>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class. Optionally providing
        /// instances of a ConfigReader and PluginManager
        /// </summary>
        /// <param name="reader">The implementation of a configuration reader.</param>
        /// <param name="manager">The implementation of the plug-in manager.</param>
        public HealthService(IConfigReader reader, PluginManager manager)
        {
            _reader = reader ?? new ConfigurationReader();
            _manager = manager ?? new PluginManager(null);
            Groups = new List<HealthCheckGroup>();
        }

        /// <summary>
        /// Gets or sets the list of configuration groups
        /// </summary>
        public List<HealthCheckGroup> Groups { get; set; }

        /// <summary>
        /// Starts the health check service.
        /// </summary>
        public void Start()
        {
            _log.Info(m => m("----> HealthCheck Service is Starting..."));

            Groups = _reader.ReadGroups();
        }

        /// <summary>
        /// Stops the health check service.
        /// </summary>
        public void Stop()
        {
            _log.Info(m => m("----> HealthCheck Service is Stoping..."));
        }
    }
}
