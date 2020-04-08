using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using Quartz;

namespace HealthCheck.Framework
{
    /// <summary>
    ///   Quartz job runner for health check plugins.
    /// </summary>
    public class HealthCheckJob : IJob, IHealthCheckJob
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthCheckJob" /> class.
        /// </summary>
        public HealthCheckJob()
        {
            Listeners = new List<IStatusListener>();
            QuietPeriods = new QuietPeriods();
            Triggers = new List<ITrigger>();
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///   Gets or sets the unique identifier of this health check job.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///   Gets or sets the XML configuration node for this heath check job.
        /// </summary>
        public JobConfiguration JobConfiguration { get; set; }

        /// <summary>
        ///   Gets or sets the listeners that should respond to the result of a health check.
        /// </summary>
        public List<IStatusListener> Listeners { get; set; }

        /// <summary>
        ///   Gets or sets the health check plugin to run.
        /// </summary>
        public IHealthCheckPlugin Plugin { get; set; }

        /// <summary>
        ///   Gets or sets the quiet periods for this health check.
        /// </summary>
        public QuietPeriods QuietPeriods { get; set; }

        /// <summary>
        ///   Gets or sets the list of Quartz triggers.
        /// </summary>
        public List<ITrigger> Triggers { get; set; }

        /// <summary>
        ///   Execute the plugin via the Quartz job execution handler.
        /// </summary>
        /// <param name="context">A Quartz context containing handles to various information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.Now;
            if (QuietPeriods.IsQuietPeriod(new DateTimeOffset(now)))
            {
                _log.Info($"{Plugin.Name} was not executed because {now} is within a quiet period.");
                return Task.FromResult<object>(null);
            }

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var status = Plugin.Execute();

                sw.Stop();

                status.Plugin = Plugin;

                _log.Debug(
                    "Executed plugin '{0}' - {1} [{2}ms]", Plugin.Name, status.Status, sw.ElapsedMilliseconds);

                NotifyListeners(status);
            }
            catch (Exception ex)
            {
                _log.Error("Exception Executing {0}: {1}", Plugin.Name, ex.ToString(), ex);
                Plugin.PluginStatus = PluginStatus.TaskExecutionFailure;
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        ///   Write out any notifications to listeners.
        /// </summary>
        /// <param name="status">The result of a health check execution.</param>
        public void NotifyListeners(IHealthStatus status)
        {
            if (status.Timestamp == DateTime.MinValue)
            {
                status.Timestamp = DateTime.Now;
            }

            foreach (var listener in Listeners)
            {
                try
                {
                    if (status.Status >= listener.Threshold)
                    {
                        listener.Process(status);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Error Publishing Status To Listener: {0}", ex.ToString(), ex);
                }
            }
        }
    }
}
