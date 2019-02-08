using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;
using ISeriLogger = Serilog.ILogger;

namespace GrpcHost.Logging
{
    internal class SplunkSerilogLoggerProvider : SerilogLoggerProvider
    {
        public SplunkSerilogLoggerProvider(IOptionsMonitor<LoggingOptions> loggingOptions)
            : base(ConfigureSerilogLogger(SetLogEventLevel(loggingOptions.CurrentValue.MinimumLogLevel)), false)
        {
        }

        private static ISeriLogger ConfigureSerilogLogger(LogEventLevel minimulLogLevel)
        {
            var config = new LoggerConfiguration().MinimumLevel.Is(minimulLogLevel);

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
