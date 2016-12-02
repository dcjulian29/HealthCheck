using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common.Logging;
using Quartz;
using Quartz.Impl.Calendar;

namespace HealthCheck.Framework
{
    /// <summary>
    /// This class is responsible for loading and parsing health check configuration files.
    /// </summary>
    public class ConfigurationFileReader : IHealthCheckConfigurationReader
    {
        private static ILog _log = LogManager.GetLogger<ConfigurationFileReader>();
        private string _configurationLocation;
        private XElement _rootNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFileReader"/> class.
        /// </summary>
        public ConfigurationFileReader()
        {
            _configurationLocation = Path.Combine(Environment.CurrentDirectory, "conf");
            _log.Debug(m => m("Will look in '{0}' for configuration files...", _configurationLocation));
        }

        /// <summary>
        /// Parses configuration and creates a Quartz Calendar (including nested calendars) that
        /// contain dates/times where the health check should not be preformed.
        /// </summary>
        /// <param name="node">XML node containing 0 or more exclusions</param>
        /// <returns>
        /// a Quartz Calendar contain dates/times where the health check should not be preformed.
        /// </returns>
        /// <remarks>
        /// Calendars are configured at the group level and apply to all checks within a group.
        /// </remarks>
        public ICalendar GetExclusions(XElement node)
        {
            ICalendar calendar = null;

            var exclusionsNode = node.Element("Calendar");

            if (exclusionsNode == null)
            {
                return null;
            }

            var exclusions = exclusionsNode.Elements().ToList();

            foreach (var exclusion in exclusions)
            {
                if (calendar == null)
                {
                    calendar = GetCalendar(exclusion);
                }
                else
                {
                    calendar.CalendarBase = GetCalendar(exclusion);
                }
            }

            return calendar;
        }

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
        public List<HealthCheckGroup> Read(string configurationFile)
        {
            var groups = new List<HealthCheckGroup>();

            if (!Directory.Exists(_configurationLocation))
            {
                return groups;
            }

            _log.Info(m => m("Parsing {0} for configuration...", configurationFile));
            groups.AddRange(ReadFile(configurationFile));

            return groups;
        }

        /// <summary>
        /// Load and read all of the configuration files in the 'configuration' folder and builds a
        /// merged list of groups containing health checks in the processed configuration file.
        /// </summary>
        /// <returns>
        /// A list of groups containing health checks in the processed configuration file.
        /// </returns>
        /// <exception cref="System.ApplicationException">
        /// Occurs when a duplicate group name is used
        /// </exception>
        public List<HealthCheckGroup> ReadAll()
        {
            var groups = new List<HealthCheckGroup>();

            if (Directory.Exists(_configurationLocation))
            {
                foreach (var file in Directory.GetFiles(_configurationLocation, "*.config"))
                {
                    _log.Info(m => m("Parsing {0} for configuration...", file));
                    groups.AddRange(ReadFile(file));
                }
            }

            return groups;
        }

        private ICalendar GetCalendar(XElement node)
        {
            var exclusionType = ReadAttribute(node, "Type");

            switch (exclusionType)
            {
                case "cron":
                    var calendar = new CronCalendar(ReadAttribute(node, "Expression"));
                    return calendar;

                default:
                    _log.Warn(m => m("Unrecognized quiet period type: {0}", exclusionType));
                    return null;
            }
        }

        private string ReadAttribute(XElement node, string name)
        {
            return node.Attributes().First(a => a.Name.LocalName == name).Value;
        }

        private Dictionary<string, string> ReadAttributes(XElement node)
        {
            return node.Attributes().ToDictionary(x => x.Name.ToString(), x => x.Value);
        }

        private List<HealthCheckGroup> ReadFile(string file)
        {
            var groups = new List<HealthCheckGroup>();

            _rootNode = XElement.Load(file);

            var groupNodes = _rootNode.Elements("Group").ToList();

            foreach (var node in groupNodes)
            {
                var group = new HealthCheckGroup()
                {
                    Name = ReadAttribute(node, "Name")
                };

                var dupeCount = groups.Count(h => h.Name == @group.Name);

                if (dupeCount > 0)
                {
                    _log.Error(m => m("Duplicate Group Name: {0}", group.Name));
                    throw new ApplicationException("Duplicate Group Name: " + group.Name);
                }

                groups.Add(group);
                group.ConfigurationNode = node;

                ReadHealthChecks(group);
            }

            return groups;
        }

        private void ReadHealthChecks(HealthCheckGroup group)
        {
            if (group.ConfigurationNode.Element("HealthChecks") == null)
            {
                return;
            }

            var healthChecksRoot = group.ConfigurationNode.Element("HealthChecks");

            foreach (var configXml in healthChecksRoot?.Elements("Check").ToList())
            {
                var configNode = new JobConfiguration()
                {
                    Name = ReadAttribute(configXml, "Name"),
                    Type = ReadAttribute(configXml, "Type")
                };

                if (@group.Checks.Count(h => h.JobConfiguration.Name == configNode.Name) > 0)
                {
                    _log.Warn(m => m("Duplicate Health Check Name: {0}", configNode.Name));
                    throw new ApplicationException("Duplicate Health Check Name: " + configNode.Name);
                }

                configNode.Listeners = configXml.Elements("Listener").ToList();
                configNode.Triggers = configXml.Elements("Trigger").ToList();
                configNode.Settings = configXml.Element("Settings");

                var check = new HealthCheckJob()
                {
                    JobConfiguration = configNode
                };

                group.Checks.Add(check);
            }

            group.CalendarExclusion = GetExclusions(group.ConfigurationNode);
        }
    }
}
