using System;
using System.IO;
using GrpcHost.Interceptors;
using GrpcHost.Logging;
using GrpcHost.Methods;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Techsson.Gaming.Infrastructure.Grpc.Host;

namespace GrpcHost
{
    public static class GrpcHostBuilder
    {
        public static IHost BuildHost<T>(string[] args, Action<HostBuilderContext, IServiceCollection> configureServices)
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
                .ConfigureServices((hostContext, configSvc) =>
                {
                    configSvc.Configure<LoggingOptions>(hostContext.Configuration.GetSection("LoggingOptions"));
                    configSvc.Configure<HostOptions>(hostContext.Configuration.GetSection("HostOptions"));

                    // TODO: Try and refactor this to something better
                    configSvc.Configure<HostOptions>(x =>
                    {
                        using (var provider = configSvc.BuildServiceProvider())
                        {
                            x.RegisteredMethods = provider.GetServices<IMethodContext>();
                        }
                    });

                    configSvc.AddSingleton<ILoggerProvider, SplunkSerilogLoggerProvider>();
                    configSvc.AddSingleton<ILogger, Logger<T>>();

                    configSvc.AddSingleton<ExtendedHealthServiceImpl>();
                    configSvc.AddSingleton<ExceptionInterceptor>();
                    configSvc.AddSingleton<GlobalInterceptor>();
                    configSvc.AddSingleton<GrpcServer>();

                    configSvc.AddHostedService<GrpcHostedService>();
                })
                .ConfigureServices(configureServices)
                .UseConsoleLifetime()
                .Build();

            return host;
        }
    }
}
