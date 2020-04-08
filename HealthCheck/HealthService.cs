using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HealthCheck.Framework;
using NLog;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace HealthCheck
{
    /// <summary>
    ///   Health Check Windows Service
    /// </summary>
    public class HealthService : IJobFactory
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly PluginManager _manager;
        private readonly IHealthCheckConfigurationReader _reader;
        private readonly IScheduler _scheduler;

        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthService" /> class.
        /// </summary>
        public HealthService() : this(new ConfigurationFileReader(), new PluginManager(new ComponentFactory()))
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="HealthService" /> class. Optionally
        ///   providing instances of a ConfigReader and PluginManager
        /// </summary>
        /// <param name="reader">The implementation of a configuration reader.</param>
        /// <param name="manager">The implementation of the plug-in manager.</param>
        public HealthService(IHealthCheckConfigurationReader reader, PluginManager manager)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            _reader = reader ?? new ConfigurationFileReader();
            _manager = manager ?? new PluginManager(new ComponentFactory());
            Groups = new List<HealthCheckGroup>();

            _scheduler = new StdSchedulerFactory().GetScheduler().Result;
            _scheduler.JobFactory = this;
        }

        /// <summary>
        ///   Gets or sets the list of configuration groups
        /// </summary>
        public List<HealthCheckGroup> Groups { get; set; }

        /// <summary>
        ///   Creates new job.
        /// </summary>
        /// <param name="bundle">
        ///   The TriggerFiredBundle from which the <see cref="T:Quartz.IJobDetail" /> and other
        ///   info relating to the trigger firing can be obtained.
        /// </param>
        /// <param name="scheduler">a handle to the scheduler that is about to execute the job</param>
        /// <returns>the newly instantiated Job</returns>
        /// <throws>SchedulerException if there is a problem instantiating the Job.</throws>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            _log.Info($"Searching for job '{bundle.JobDetail.Key}' ...");

            foreach (var group in Groups)
            {
                var job = group.Checks.FirstOrDefault(c => c.Id == bundle.JobDetail.Key.Name) as IJob;

                if (job != null)
                {
                    _log.Info($"Found job. Id: {bundle.JobDetail.Key.Name}");
                    return job;
                }
            }

            _log.Error($"Unable to find a matching job. Id: {bundle.JobDetail.Key.Name}");

            return null;
        }

        /// <summary>
        ///   Allows the job factory to destroy/cleanup the job if needed.
        /// </summary>
        /// <param name="job">A class that implements the IJob interface.</param>
        public void ReturnJob(IJob job)
        {
            _log.Debug($"{job} is returning...");
        }

        /// <summary>
        ///   Starts the health check service.
        /// </summary>
        public void Start()
        {
            _log.Info("----> HealthCheck Service is Starting...");

            Groups = _reader.ReadAll();

            _log.Debug("Initializing Plugins...");

            foreach (var group in Groups)
            {
                foreach (var check in group.Checks)
                {
                    _manager.InitializeCheckJob(check, group);
                }
            }

            _log.Debug("Configuring job schedules and triggers...");

            var counter = 0;

            foreach (var group in Groups)
            {
                foreach (var check in group.Checks)
                {
                    counter++;

                    var detail = new JobDetailImpl(check.Id, group.Name, typeof(HealthCheckJob));

                    foreach (var trigger in check.Triggers)
                    {
                        _scheduler.ScheduleJob(detail, trigger);
                    }
                }
            }

            _log.Info($"Found {counter} health check jobs");

            _log.Info("Starting Scheduler...");

            _scheduler.Start();
        }

        /// <summary>
        ///   Stops the health check service.
        /// </summary>
        public void Stop()
        {
            _log.Info("----> HealthCheck Service is Stoping...");

            _ = _scheduler.Shutdown(true);
        }
    }
}
