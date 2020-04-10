using System;
using System.Composition;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using NLog;

namespace HealthCheck.Plugins
{
    /// <summary>
    ///   Ping a network host by IP or name Configuration parameters -hostname, -timeout
    /// </summary>
    [Export("PingCheck", typeof(IHealthCheckPlugin))]
    public class PingCheckPlugin : IHealthCheckPlugin
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   Gets or sets the associated health check group
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        ///   Gets or sets the hostname or IP address to ping
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        ///   Gets or sets the plugin name or description
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the plugin last reported status.
        /// </summary>
        public PluginStatus PluginStatus { get; set; }

        /// <summary>
        ///   Gets the ping response time threshold for errors
        /// </summary>
        public int ResponseTimeError { get; private set; }

        /// <summary>
        ///   Gets the ping response time threshold for warnings
        /// </summary>
        public int ResponseTimeWarn { get; private set; }

        /// <summary>
        ///   Gets the number of times to retry
        /// </summary>
        public int Retries { get; private set; }

        /// <summary>
        ///   Gets the delay between retries in milliseconds
        /// </summary>
        public int RetryDelay { get; private set; }

        /// <summary>
        ///   Gets the ping timeout in milliseconds
        /// </summary>
        public int TimeOut { get; private set; }

        /// <summary>
        ///   Executes task(s) to be executed when the health check provided by this plugin is invoked.
        /// </summary>
        /// <returns>The status of the result of the health check.</returns>
        public IHealthStatus Execute()
        {
            _log.Debug($"Plugin '{Name}' is executing...");
            PluginStatus = PluginStatus.Executing;

            try
            {
                var ping = new Ping();
                var sw = new Stopwatch();

                sw.Start();

                var data = Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString());
                var options = new PingOptions()
                {
                    DontFragment = true
                };

                var retry = true;
                var counter = 0;
                PingReply reply = null;

                while (retry)
                {
                    reply = ping.Send(HostName, TimeOut, data, options);

                    if (reply.Status == IPStatus.Success)
                    {
                        retry = false;
                    }
                    else
                    {
                        if (counter++ >= Retries)
                        {
                            _log.Info("Ping failed, retries exceeded.");
                            retry = false;
                        }
                        else
                        {
                            _log.Info($"Ping failed, retrying.  Attempt #{counter}");
                            Thread.Sleep(RetryDelay);
                        }
                    }
                }

                sw.Stop();

                _log.Debug(
                    $"Ping {reply.Status} [{reply.RoundtripTime}/{sw.Elapsed.TotalMilliseconds}ms]");

                var status = ProcessPingResponse(reply.Status, reply.RoundtripTime);

                PluginStatus = PluginStatus.Idle;

                return status;
            }
            catch (Exception ex)
            {
                PluginStatus = PluginStatus.TaskExecutionFailure;

                _log.Error("Exception during PingCheck {0}: {1}", HostName, ex.Message, ex);

                return new PingCheckStatus()
                {
                    Status = CheckResult.Error,
                    Summary = ex.Message,
                    Details = ex.ToString()
                };
            }
        }

        /// <summary>
        ///   Processes the ping response.
        /// </summary>
        /// <param name="pingStatus">The ping status.</param>
        /// <param name="roundtripTime">The roundtrip time.</param>
        /// <returns>A PingCheckStatus containing the result of the Ping.</returns>
        public PingCheckStatus ProcessPingResponse(IPStatus pingStatus, long roundtripTime)
        {
            var status = new PingCheckStatus();

            if (pingStatus == IPStatus.Success)
            {
                if ((ResponseTimeError > 0) && (roundtripTime > ResponseTimeError))
                {
                    status.Status = CheckResult.Error;
                    status.Summary = "Ping Check Failed";
                    status.Details = "Response time exceeds error threshold.  Response time: " + roundtripTime;
                }
                else if ((ResponseTimeWarn > 0) && (roundtripTime > ResponseTimeWarn))
                {
                    status.Status = CheckResult.Warning;
                    status.Summary = "Ping Check Warning";
                    status.Details = "Response time exceeds warning threshold.  Response time: " + roundtripTime;
                }
                else
                {
                    status.Summary = "Ping Check Passed.  Response time: " + roundtripTime;
                    status.Status = CheckResult.Success;
                }
            }
            else
            {
                status.Summary = "Ping Check Failed";
                status.Details = "Unsuccessful result code: " + pingStatus;
                status.Status = CheckResult.Error;
            }

            return status;
        }

        /// <summary>
        ///   Load up configuration settings from an XML fragment
        /// </summary>
        /// <param name="configurationElement">
        ///   XML Fragment containing configuration information for the plugin
        /// </param>
        public void SetTaskConfiguration(XElement configurationElement)
        {
            _log.Debug($"Plugin '{Name}' is setting configuration...");

            HostName = configurationElement.Element("HostName")?.Value;
            Retries = Convert.ToInt32((configurationElement.Element("Retries")?.Value) ?? "3");
            RetryDelay = Convert.ToInt32((configurationElement.Element("RetryDelay")?.Value) ?? "2500");
            TimeOut = Convert.ToInt32((configurationElement.Element("TimeOut")?.Value) ?? "5000");
            ResponseTimeWarn =
                Convert.ToInt32((configurationElement.Element("ResponseTimeWarn")?.Value) ?? "500");
            ResponseTimeError =
                Convert.ToInt32((configurationElement.Element("ResponseTimeError")?.Value) ?? "3000");

            if (string.IsNullOrEmpty(HostName))
            {
                throw new MissingRequiredSettingException("Host name is required");
            }
        }

        /// <summary>
        ///   Shuts down this instance.
        /// </summary>
        public void Shutdown()
        {
            _log.Debug($"Plugin '{Name}' is shutting down...");
        }

        /// <summary>
        ///   Start the plugin, allowing it to initialize before being executed by the framework scheduler.
        /// </summary>
        public void Startup()
        {
            _log.Debug($"Plugin '{Name}' is starting...");
            PluginStatus = PluginStatus.Idle;
        }
    }
}
