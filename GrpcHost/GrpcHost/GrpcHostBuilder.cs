using System;
using System.Collections.Generic;
using System.IO;
using GrpcHost.Interceptors;
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
                    //configSvc.Configure<LoggingOptions>(hostContext.Configuration.GetSection("LoggingOptions"));
                    configSvc.Configure<HostOptions>(hostContext.Configuration.GetSection("HostOptions"));
                    configSvc.Configure<IEnumerable<DiagnosticInterceptorOption>>(hostContext.Configuration.GetSection("DiagnosticInterceptorOptions"));

                    //configSvc.AddSingleton<ILoggerProvider, GameplaySerilogLoggerProvider>();
                    configSvc.AddSingleton<ILogger, Logger<T>>();
                    
                    configSvc.AddTransient(x => new ExtendedHealthServiceImpl(null));
                    configSvc.AddSingleton<IMethodResolver, MethodResolver>();
                    configSvc.AddSingleton<ExceptionInterceptor>();
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