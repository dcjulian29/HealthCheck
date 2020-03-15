using System;
using System.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Xml.Linq;
using Common.Logging;
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
        private static ILog _log = LogManager.GetLogger<ComponentFactory>();

        private readonly string _pluginLocation;
        private CompositionHost _container;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ComponentFactory" /> class.
        /// </summary>
        public ComponentFactory()
        {
            _pluginLocation = Path.Combine(Environment.CurrentDirectory, "plugins");

            _log.Debug(m => m("Will look in '{0}' for plug-ins...", _pluginLocation));

            CreatePluginContainer();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ComponentFactory" /> class.
        /// </summary>
        public ComponentFactory(string pluginLocation)
        {
            _pluginLocation = pluginLocation;

            _log.Debug(m => m("Will look in '{0}' for plug-ins...", _pluginLocation));

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
            ITrigger trigger = null;

            var triggerType = String.Empty;

            try
            {
                triggerType = node?.Attribute("Type")?.Value.ToLowerInvariant();
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
                    if (node?.Attribute("Expression") != null)
                    {
                        trigger = new CronTriggerImpl(
                            Guid.NewGuid().ToString(), "Project", node?.Attribute("Expression")?.Value);
                    }
                    else
                    {
                        _log.Warn("Missing cron expression in this trigger...");
                    }

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
            if (disposing)
            {
                if (_container != null)
                {
                    _container.Dispose();
                    _container = null;
                }
            }
        }

        private void CreatePluginContainer()
        {
            var assemblies = Directory.GetFiles(_pluginLocation, "*.dll")
                .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath);

            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);

            _container = configuration.CreateContainer();
        }

        /// <summary>
        ///   Create a Quartz simple trigger based off the XML configuration.
        /// </summary>
        /// <param name="node">XML configuration node.</param>
        /// <returns>A simple Quartz trigger.</returns>
        private ITrigger CreateSimpleTrigger(XElement node)
        {
            var repeatcount = -1;
            var newId = Guid.NewGuid().ToString();
            var repeatinterval = TimeSpan.Parse(node?.Attribute("Repeat")?.Value);

            // wait for 10 seconds + random amount before running the first job
            var startupDelay = DateTime.Now.AddSeconds(10 + new Random().Next(0, 60));

            ITrigger trigger = new SimpleTriggerImpl(
                newId,
                "Project",
                startupDelay,
                null,
                repeatcount,
                repeatinterval);

            return trigger;
        }

        private T GetExportInstance<T>(string typeName) where T : class
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            _log.Debug(m => m("Attempting to create a {0} named {1}.", typeof(T), typeName));

            var instance = _container.GetExport<T>(typeName);

            if (instance == null)
            {
                _log.Error("MEF did not error, but returned a null object...");
                return null;
            }

            var instanceType = instance.GetType();
            var location = instanceType.Assembly.Location;

            if (location != null)
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(location);

                _log.Debug(
                    m => m("Loaded {0} [v{1}]", instanceType.AssemblyQualifiedName, versionInfo.FileVersion));
            }

            return instance;
        }
    }
}
