using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using ISeriLogger = Serilog.ILogger;

namespace GrpcHost.Instrumentation.Logging
{
    internal class SplunkSerilogLoggerProvider : SerilogLoggerProvider
    {
        public SplunkSerilogLoggerProvider(CorrelationEnricher correlationEnricher, IOptionsMonitor<LoggingOptions> loggingOptions)
            : base(ConfigureSerilogLogger(correlationEnricher, SetLogEventLevel(loggingOptions.CurrentValue.MinimumLogLevel)), false)
        {
        }

        private static ISeriLogger ConfigureSerilogLogger(CorrelationEnricher correlationEnricher, LogEventLevel minimulLogLevel)
        {
            var config =
                new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(new LoggingLevelSwitch(minimulLogLevel))
                    .Enrich.With(correlationEnricher);

            ISeriLogger logger = ConfigureSerilogConsole();

            return logger;

            ISeriLogger ConfigureSerilogConsole()
            {
                logger = config.WriteTo.Console(formatter: new JsonFormatter(renderMessage: false)).CreateLogger();
                logger.Information("Console logging has been configured");

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
