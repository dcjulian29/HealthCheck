using System;
using System.Reflection;
using Common.Logging;
using Topshelf;
using Topshelf.Common.Logging;

namespace HealthCheck
{
    internal class Program
    {
        private static ILog _log = LogManager.GetLogger<Program>();

        private static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly();

            _log.Info(m => m(
                "{0} {1} initialized on {2}",
                assembly.GetName().Name,
                assembly.GetName().Version.ToString(),
                Environment.MachineName));

            HostFactory.Run(x =>
            {
                x.Service<HealthService>(s =>
                {
                    s.ConstructUsing(n => new HealthService());
                    s.WhenStarted(h => h.Start());
                    s.WhenStopped(h => h.Stop());
                });

                x.UseCommonLogging();

                x.RunAsNetworkService();

                x.SetDescription("Serice Runtime for the Health Check Service");
                x.SetDisplayName("Health Check Service");
                x.SetServiceName("HealthCheck");
            });
        }
    }
}
