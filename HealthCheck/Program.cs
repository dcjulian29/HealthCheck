using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NLog;
using Topshelf;

namespace HealthCheck
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly().GetName();

            _log.Info($"{assembly.Name} {assembly.Version} initialized on {Environment.MachineName}");

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
