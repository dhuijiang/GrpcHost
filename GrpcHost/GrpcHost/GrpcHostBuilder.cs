using System;
using System.IO;
using Grpc.HealthCheck;
using GrpcHost.Health;
using GrpcHost.Interceptors;
using GrpcHost.Logging;
using GrpcHost.Methods;
using GrpcHost.Server;
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
                .ConfigureServices((hostContext, configSvc) =>
                {
                    configSvc.Configure<LoggingOptions>(hostContext.Configuration.GetSection("LoggingOptions"));
                    configSvc.Configure<HostOptions>(hostContext.Configuration.GetSection("HostOptions"));

                    configSvc.AddSingleton<ICallContext, CallContext>();
                    configSvc.AddSingleton<IClientFactory, ClientFactory>();

                    configSvc.AddSingleton<CorrelationEnricher>();
                    configSvc.AddSingleton<ILoggerProvider, SplunkSerilogLoggerProvider>();
                    configSvc.AddSingleton<ILogger, Logger<T>>();

                    configSvc.AddSingleton<HealthServiceImpl, ExtendedHealthServiceImpl>();
                    configSvc.AddSingleton<GlobalInterceptor>();
                    configSvc.AddSingleton<GrpcServer>();
                    configSvc.AddSingleton(x => new MethodRegistry { RegisteredMethods = x.GetServices<IMethodContext>() });

                    configSvc.AddHostedService<GrpcHostedService>();
                })
                .ConfigureServices(configureServices)
                .UseConsoleLifetime()
                .Build();

            return host;
        }
    }
}
