using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using NLog;
using Topshelf;

namespace HealthCheck
{
    [ExcludeFromCodeCoverage]
    internal static class Program
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static HealthService _service;

        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            _log.Info("Shutting down...");
            _service.Stop();
        }

        private static void Main(string[] args)
        {
            var assembly = Assembly.GetEntryAssembly().GetName();

            _log.Info($"{assembly.Name} {assembly.Version} initialized on {Environment.MachineName}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _log.Debug("Running on Non-Windows. Running as console style application.");

                _service = new HealthService();
                _service.Start();

                AssemblyLoadContext.Default.Unloading += SigTermEventHandler;
                Console.CancelKeyPress += CancelHandler;

                while (true)
                {
                    Console.Read();
                }
            }
            else
            {
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

        private static void SigTermEventHandler(AssemblyLoadContext context)
        {
            _log.Info("Caught SigTerm... Shutting down...");
            _service.Stop();
        }
    }
}
