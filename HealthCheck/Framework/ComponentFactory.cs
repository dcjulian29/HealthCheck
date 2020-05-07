using System;
using System.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using NLog;
using Quartz;
using Quartz.Impl.Triggers;
using ToolKit;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   Provides factory methods for creating plugins based off a type/key.
    /// </summary>
    public class ComponentFactory : DisposableObject, IComponentFactory
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly string _pluginLocation;
        private CompositionHost _container;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ComponentFactory" /> class.
        /// </summary>
        public ComponentFactory()
        {
            _pluginLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (Directory.Exists(Path.Combine(_pluginLocation, "plugins")))
            {
                _pluginLocation = Path.Combine(_pluginLocation, "plugins");
            }

            _log.Debug("Will look in '{0}' for plug-ins...", _pluginLocation);

            CreatePluginContainer();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ComponentFactory" /> class.
        /// </summary>
        /// <param name="pluginLocation">The directory that the plugin are located.</param>
        public ComponentFactory(string pluginLocation)
        {
            _pluginLocation = Path.GetFullPath(pluginLocation);

            _log.Debug("Will look in '{0}' for plug-ins...", _pluginLocation);

            CreatePluginContainer();
        }

        /// <summary>
        ///   Create an instance of a IStatusListener from the MEF assembly catalog
        /// </summary>
        /// <param name="typeName">The name matching export of the plug-in.</param>
        /// <returns>An instance of a health check status listener.</returns>
        /// <exception cref="System.ArgumentNullException">The name of the type is required.</exception>
        public IStatusListener GetListener(string typeName)
        {
            return GetExportInstance<IStatusListener>(typeName);
        }

        /// <summary>
        ///   Create an instance of a IHealthCheckPlugin from the MEF assembly catalog
        /// </summary>
        /// <param name="typeName">The name matching export of the plug-in.</param>
        /// <returns>An instance of a health check plug-in.</returns>
        /// <exception cref="System.ArgumentNullException">The name of the type is required.</exception>
        public IHealthCheckPlugin GetPlugin(string typeName)
        {
            return GetExportInstance<IHealthCheckPlugin>(typeName);
        }

        /// <summary>
        ///   Create an instance of a Quartz trigger.
        /// </summary>
        /// <param name="node">The XML node containing the trigger information.</param>
        /// <returns>a Quartz trigger.</returns>
        public ITrigger GetTrigger(XElement node)
        {
            if (node == null)
            {
                return null;
            }

            ITrigger trigger = null;

            var triggerType = String.Empty;

            try
            {
                triggerType = node.Attribute("Type").Value.ToLowerInvariant();
            }
            catch (NullReferenceException)
            {
                _log.Warn("This trigger node did not provide a type...");
            }

            switch (triggerType)
            {
                case "simple":
                    trigger = CreateSimpleTrigger(node);

                    break;

                case "cron":
                    if (node.Attribute("Expression") != null)
                    {
                        trigger = new CronTriggerImpl(
                            Guid.NewGuid().ToString(), "Project", node.Attribute("Expression").Value);
                    }
                    else
                    {
                        _log.Warn("Missing cron expression in this trigger...");
                    }

                    break;

                default:
                    _log.Warn($"'{triggerType}' is an unknown trigger type.");
                    break;
            }

            return trigger;
        }

        /// <summary>
        ///   Disposes the resources used by this component factory.
        /// </summary>
        /// <param name="disposing">
        ///   if set to <c>false</c>, this method has been called by the runtime.
        /// </param>
        protected override void DisposeResources(bool disposing)
        {
            if (disposing && _container != null)
            {
                _container.Dispose();
                _container = null;
            }
        }

        private void CreatePluginContainer()
        {
            var configuration = new ContainerConfiguration();
            var files = Directory.GetFiles(_pluginLocation, "*.dll")
                    .Where(n => !n.Contains("System."))
                    .Where(n => !n.Contains("TopShelf."))
                    .ToList();

            if (files.Count > 0)
            {
                var assemblies = files.Select(AssemblyLoadContext.Default.LoadFromAssemblyPath).ToList();

                configuration = configuration.WithAssemblies(assemblies);
            }

            _container = configuration.CreateContainer();
        }

        /// <summary>
        ///   Create a Quartz simple trigger based off the XML configuration.
        /// </summary>
        /// <param name="node">XML configuration node.</param>
        /// <returns>A simple Quartz trigger.</returns>
        private ITrigger CreateSimpleTrigger(XElement node)
        {
            if (node.Attribute("Repeat") == null)
            {
                return null;
            }

            var repeatcount = -1;
            var newId = Guid.NewGuid().ToString();
            var repeatinterval = TimeSpan.Parse(node.Attribute("Repeat").Value);

            // wait for 10 seconds + random amount before running the first job
            var startupDelay = DateTime.Now.AddSeconds(10 + new Random().Next(0, 60));

            return new SimpleTriggerImpl(
                newId,
                "Project",
                startupDelay,
                null,
                repeatcount,
                repeatinterval);
        }

        private T GetExportInstance<T>(string typeName) where T : class
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            _log.Debug("Attempting to create a {0} named {1}.", typeof(T), typeName);

            T instance = default(T);

            try
            {
                instance = _container.GetExport<T>(typeName);
            }
            catch (CompositionFailedException ex)
            {
                _log.Error(ex.Message);
                return null;
            }

            var instanceType = instance.GetType();
            var location = instanceType.Assembly.Location;
            var versionInfo = FileVersionInfo.GetVersionInfo(location);

            _log.Debug("Loaded {0} [v{1}]", instanceType.AssemblyQualifiedName, versionInfo.FileVersion);

            return instance;
        }
    }
}
