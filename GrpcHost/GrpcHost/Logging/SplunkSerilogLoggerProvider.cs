using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using Techsson.Gaming.Infrastructure.Consul;
using Techsson.Platform.Configuration.Contracts.Entities;
using ISeriLogger = Serilog.ILogger;

namespace GrpcHost.Logging
{
    internal class SplunkSerilogLoggerProvider : SerilogLoggerProvider
    {
        public GameplaySerilogLoggerProvider(IConsulConfigurationProvider configuration, IOptionsMonitor<LoggingOptions> loggingOptions)
            : base(ConfigureSerilogLogger(configuration, SetLogEventLevel(loggingOptions.CurrentValue.MinimumLogLevel)), false)
        {
        }

        private static ISeriLogger ConfigureSerilogLogger(IConsulConfigurationProvider configurationProvider, LogEventLevel minimulLogLevel)
        {
            var config = new LoggerConfiguration().MinimumLevel.Is(minimulLogLevel);

            ISeriLogger logger;
            try
            {
                logger = ConfigureSerilogSplunk();
            }
            catch(Exception e)
            {
                logger = ConfigureSerilogConsole(e);
            }

            return logger;

            ISeriLogger ConfigureSerilogSplunk()
            {
                var splunkConfig = configurationProvider.Get<GeneralConfiguration>("SplunkEventCollector");

                var splunkHost = splunkConfig.Configuration["httpEndpoint"].ToString();
                var splunkToken = splunkConfig.Configuration["token"].ToString();

                if(string.IsNullOrEmpty(splunkHost) == false && string.IsNullOrEmpty(splunkToken) == false)
                {
                    config = config.Enrich.With<CorrelationEnricher>().Enrich.With<ExceptionEnricher>();

                    logger =
                        config.WriteTo.EventCollector(splunkHost, splunkToken).CreateLogger();
                    logger.Information($"Splunk sink has been configured. {splunkHost}{Environment.NewLine}");

                    return logger;
                }

                throw new Exception($"Splunk event collector has an invalid configuration: {splunkHost}:{splunkToken}");
            }

            ISeriLogger ConfigureSerilogConsole(Exception exception)
            {
                logger = config.WriteTo.Console(formatter: new JsonFormatter(renderMessage: false)).CreateLogger();
                logger.Information("Console logging has been configured");

                logger.Warning("Splunk sink configuration failed, Console logger will be use instead.", exception);

                return logger;
            }
        }

        private static LogEventLevel SetLogEventLevel(LogLevel minimumLogLevel)
        {
            return
                minimumLogLevel == LogLevel.None
                ? LogEventLevel.Verbose
                : (LogEventLevel)minimumLogLevel;
        }
    }
}
