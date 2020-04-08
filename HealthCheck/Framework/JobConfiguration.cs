using System.Collections.Generic;
using System.Xml.Linq;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   Represents the configuration settings for one health check job
    /// </summary>
    public class JobConfiguration
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="JobConfiguration" /> class.
        /// </summary>
        public JobConfiguration()
        {
            Listeners = new List<XElement>();
            Triggers = new List<XElement>();
        }

        /// <summary>
        ///   Gets or sets the listeners that will get the result of this health check.
        /// </summary>
        public List<XElement> Listeners { get; set; }

        /// <summary>
        ///   Gets or sets the name of this health check.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the settings that can be used by this health check.
        /// </summary>
        public XElement Settings { get; set; }

        /// <summary>
        ///   Gets or sets the triggers for this health check.
        /// </summary>
        public List<XElement> Triggers { get; set; }

        /// <summary>
        ///   Gets or sets the type of plug-in this health check uses.
        /// </summary>
        public string Type { get; set; }
    }
}
