using System;
using System.Reflection;
using NLog;
using Topshelf;

namespace HealthCheck
{
    internal static class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly();

            _log.Info("{0} {1} initialized on {2}",
                assembly.GetName().Name,
                assembly.GetName().Version.ToString(),
                Environment.MachineName);

            HostFactory.Run(x =>
            {
                x.Service<HealthService>(s =>
                {
                    s.ConstructUsing(n => new HealthService());
                    s.WhenStarted(h => h.Start());
                    s.WhenStopped(h => h.Stop());
                });

                x.UseNLog();

                x.RunAsNetworkService();

                x.SetDescription("Service Runtime for the Health Check Service");
                x.SetDisplayName("Health Check Service");
                x.SetServiceName("HealthCheck");
            });
        }
    }
}
