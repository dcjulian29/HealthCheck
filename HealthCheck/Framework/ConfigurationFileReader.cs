using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NLog;
using Quartz.Impl.Calendar;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   This class is responsible for loading and parsing health check configuration files.
    /// </summary>
    public class ConfigurationFileReader : IHealthCheckConfigurationReader
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly string _configurationLocation;
        private readonly List<HealthCheckGroup> _groups = new List<HealthCheckGroup>();

        /// <summary>
        ///   Initializes a new instance of the <see cref="ConfigurationFileReader" /> class.
        /// </summary>
        public ConfigurationFileReader()
        {
            _configurationLocation = Path.Combine(Environment.CurrentDirectory, "conf");
            _log.Debug("Will look in '{0}' for configuration files...", _configurationLocation);
        }

        /// <summary>
        ///   Load and read a configuration file located in the 'configuration' folder and builds a
        ///   list of groups containing health checks in the processed configuration file.
        /// </summary>
        /// <param name="configurationFile">The configuration file path.</param>
        /// <returns>
        ///   A list of groups containing health checks in the processed configuration file.
        /// </returns>
        /// <exception cref="System.ApplicationException">
        ///   Occurs when a duplicate group name is used
        /// </exception>
        public List<HealthCheckGroup> Read(string configurationFile)
        {
            _log.Info("Parsing {0} for configuration...", configurationFile);

            _groups.AddRange(ReadFile(configurationFile));

            return _groups;
        }

        /// <summary>
        ///   Load and read all of the configuration files in the 'configuration' folder and builds
        ///   a merged list of groups containing health checks in the processed configuration file.
        /// </summary>
        /// <returns>
        ///   A list of groups containing health checks in the processed configuration file.
        /// </returns>
        /// <exception cref="System.ApplicationException">
        ///   Occurs when a duplicate group name is used
        /// </exception>
        public List<HealthCheckGroup> ReadAll()
        {
            if (!Directory.Exists(_configurationLocation))
            {
                return _groups;
            }

            foreach (var file in Directory.GetFiles(_configurationLocation, "*.xml"))
            {
                Read(file);
            }

            return _groups;
        }

        private QuietPeriods GetQuietPeriods(XElement node)
        {
            var periods = new QuietPeriods();

            var exclusionsNode = node.Element("QuietPeriods");

            foreach (var exclusion in exclusionsNode.Elements().ToList())
            {
                var exclusionType = ReadAttribute(exclusion, "Type");

                switch (exclusionType)
                {
                    case "cron":
                        periods.AddCalendar(new CronCalendar(ReadAttribute(exclusion, "Expression")));
                        break;

                    default:
                        _log.Warn("Unrecognized quiet period type: {0}", exclusionType);
                        break;
                }
            }

            return periods;
        }

        private string ReadAttribute(XElement node, string name)
        {
            return node.Attributes().First(a => a.Name.LocalName == name).Value;
        }

        private List<HealthCheckGroup> ReadFile(string file)
        {
            var groups = new List<HealthCheckGroup>();

            if (ValidateConfigurationFile(file))
            {
                return groups;
            }

            var rootNode = XElement.Load(file);

            foreach (var node in rootNode.Elements("Group").ToList())
            {
                var group = new HealthCheckGroup()
                {
                    Name = ReadAttribute(node, "Name")
                };

                var dupeCount = _groups.Count(h => h.Name == @group.Name);

                if (dupeCount > 0)
                {
                    _log.Error("Duplicate Group Name: {0}", group.Name);
                    throw new DuplicateHealthCheckException("Duplicate Group Name: " + group.Name);
                }

                group.ConfigurationNode = node;

                ReadHealthChecks(group);

                if (group.Checks.Count > 0)
                {
                    groups.Add(group);
                }
            }

            return groups;
        }

        private void ReadHealthChecks(HealthCheckGroup group)
        {
            var healthChecksRoot = group.ConfigurationNode.Element("HealthChecks");

            foreach (var configXml in healthChecksRoot?.Elements("Check")?.ToList())
            {
                var configNode = new JobConfiguration()
                {
                    Name = ReadAttribute(configXml, "Name"),
                    Type = ReadAttribute(configXml, "Type")
                };

                if (@group.Checks.Any(h => h.JobConfiguration.Name == configNode.Name))
                {
                    _log.Warn("Duplicate Health Check Name: {0}", configNode.Name);
                    throw new DuplicateHealthCheckException("Duplicate Health Check Name: " + configNode.Name);
                }

                configNode.Listeners = configXml.Elements("Listener").ToList();
                configNode.Triggers = configXml.Elements("Trigger").ToList();
                configNode.Settings = configXml.Element("Settings");

                var check = new HealthCheckJob()
                {
                    JobConfiguration = configNode,
                    QuietPeriods = GetQuietPeriods(configXml)
                };

                group.Checks.Add(check);
            }
        }

        private bool ValidateConfigurationFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "HealthCheck.HealthCheckXMLSchema.xsd";
            var schemaSet = new XmlSchemaSet();
            var resource = assembly.GetManifestResourceStream(resourceName);

            using (var reader = new StreamReader(resource))
            {
                var xsd = reader.ReadToEnd();
                _ = schemaSet.Add(string.Empty, XmlReader.Create(new StringReader(xsd)));
            }

            var doc = XDocument.Load(XmlReader.Create(file));

            var error = false;

            doc.Validate(
                schemaSet,
                (sender, e) =>
                {
                    _log.Error("{0} in {1}", e.Message, file);
                    error = true;
                },
                true);

            return error;
        }
    }
}
