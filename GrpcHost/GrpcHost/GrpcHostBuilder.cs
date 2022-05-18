using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Grpc.HealthCheck;
using GrpcHost.Core;
using GrpcHost.Core.Interceptors;
using GrpcHost.Health;
using GrpcHost.Instrumentation;
using GrpcHost.Instrumentation.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Techsson.Gaming.Infrastructure.Grpc.Host;
using Techsson.Gaming.Infrastructure.Grpc.Instrumentation.Tracing;

namespace GrpcHost
{
    public class GrpcHostBuilder : HostBuilder
    {
        public GrpcHostBuilder()
        {
            ConfigureServices((hostContext, configSvc) =>
            {
                configSvc.Configure<LoggingOptions>(hostContext.Configuration.GetSection("LoggingOptions"));
                configSvc.Configure<HostOptions>(hostContext.Configuration.GetSection("HostOptions"));
                configSvc.Configure<Collection<ChannelOptions>>(hostContext.Configuration.GetSection("ChannelOptions"));
                configSvc.Configure<JaegerOptions>(hostContext.Configuration.GetSection("JaegerOptions"));

                configSvc.AddSingleton<ITracerFactory, JaegerTracerFactory>();
                configSvc.AddSingleton<IInstrumentationContext, InstrumentationContext>();

                configSvc.AddSingleton<CorrelationEnricher>();
                configSvc.AddSingleton<ILoggerProvider, SplunkSerilogLoggerProvider>();

                configSvc.AddSingleton<IClientFactory, ClientFactory>();

                configSvc.AddSingleton<HealthServiceImpl, ExtendedHealthServiceImpl>();
                configSvc.AddSingleton<GlobalInterceptor>();
                configSvc.AddSingleton<GrpcServer>();
                configSvc.AddSingleton(x => new MethodRegistry { RegisteredMethods = x.GetServices<IMethodContext>() });

                configSvc.AddHostedService<GrpcHostedService>();
            });
        }

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
                    configSvc.Configure<Collection<ChannelOptions>>(hostContext.Configuration.GetSection("ChannelOptions"));
                    configSvc.Configure<JaegerOptions>(hostContext.Configuration.GetSection("JaegerOptions"));

                    configSvc.AddSingleton<ITracerFactory, JaegerTracerFactory>();
                    configSvc.AddSingleton<IInstrumentationContext, InstrumentationContext>();

                    configSvc.AddSingleton<CorrelationEnricher>();
                    configSvc.AddSingleton<ILoggerProvider, SplunkSerilogLoggerProvider>();
                    configSvc.AddSingleton<ILogger, Logger<T>>();

                    configSvc.AddSingleton<IClientFactory, ClientFactory>();

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