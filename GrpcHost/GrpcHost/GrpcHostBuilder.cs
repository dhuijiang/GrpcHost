using System;
using System.IO;
using GrpcHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.GrpcHost
{
    public static class GrpcHostBuilder
    {
        public static IHost BuildHost(string[] args, Action<HostBuilderContext, IServiceCollection> configureServices)
        {
            _ = configureServices ?? throw new ArgumentNullException(nameof(configureServices));

            IHost host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("hostsettings.json", optional: true);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                })
                .ConfigureServices(svcs =>
                {
                    svcs.AddLogging();
                    svcs.AddHostedService<GrpcHostedService>();
                })
                .ConfigureServices(configureServices)
                .UseConsoleLifetime()
                .Build();

            return host;
        }
    }
}