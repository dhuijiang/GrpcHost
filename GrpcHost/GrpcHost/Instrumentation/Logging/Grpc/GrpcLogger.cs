using System;
using Microsoft.Extensions.Logging;
using IGrpcLogger = Grpc.Core.Logging.ILogger;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GrpcHost.Instrumentation.Logging
{
    internal class GrpcLogger : IGrpcLogger
    {
        private ILogger _logger;

        public GrpcLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Debug(string message)
        {
            Debug(message, null);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            _logger.LogDebug(format, null);
        }

        public void Error(string message)
        {
            _logger.LogError(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            _logger.LogError(format, formatArgs);
        }

        public void Error(Exception exception, string message)
        {
            _logger.LogError(exception, message, null);
        }

        public IGrpcLogger ForType<T>()
        {
            return this;
        }

        public void Info(string message)
        {
            Info(message, null);
        }

        public void Info(string format, params object[] formatArgs)
        {
            _logger.LogInformation(format, formatArgs);
        }

        public void Warning(string message)
        {
            Warning(message, null);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            _logger.LogWarning(format, formatArgs);
        }

        public void Warning(Exception exception, string message)
        {
            _logger.LogWarning(exception, message, null);
        }
    }
}