using System;
using System.Collections.Generic;
using System.IO;
using HealthCheck.Framework;
using NLog;

namespace HealthCheck
{
    /// <summary>
    ///   Health Check Windows Service
    /// </summary>
    public class HealthService
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private PluginManager _manager;
        private IHealthCheckConfigurationReader _reader;

        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthService" /> class.
        /// </summary>
        public HealthService()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            _reader = new ConfigurationFileReader();
            _manager = new PluginManager(new ComponentFactory());
            Groups = new List<HealthCheckGroup>();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthService" /> class. Optionally
        ///   providing instances of a ConfigReader and PluginManager
        /// </summary>
        /// <param name="reader">The implementation of a configuration reader.</param>
        /// <param name="manager">The implementation of the plug-in manager.</param>
        public HealthService(IHealthCheckConfigurationReader reader, PluginManager manager)
        {
            _reader = reader ?? new ConfigurationFileReader();
            _manager = manager ?? new PluginManager(new ComponentFactory());
            Groups = new List<HealthCheckGroup>();
        }

        /// <summary>
        ///   Gets or sets the list of configuration groups
        /// </summary>
        public List<HealthCheckGroup> Groups { get; set; }

        /// <summary>
        ///   Starts the health check service.
        /// </summary>
        public void Start()
        {
            _log.Info("----> HealthCheck Service is Starting...");

            Groups = _reader.ReadAll();
        }

        /// <summary>
        ///   Stops the health check service.
        /// </summary>
        public void Stop()
        {
            _log.Info("----> HealthCheck Service is Stoping...");
        }
    }
}
