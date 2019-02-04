using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrpcHost
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
                    svcs.Configure<GrpcServerOptions>(o =>
                    {
                        using (var provider = svcs.BuildServiceProvider())
                        {
                            o.Definers.AddRange(provider.GetServices<IMethodContext>());
                        }
                    });
                    svcs.AddSingleton<GrpcServer>();
                })
                .ConfigureServices(configureServices)
                .UseConsoleLifetime()
                .Build();

            return host;
        }
    }
}