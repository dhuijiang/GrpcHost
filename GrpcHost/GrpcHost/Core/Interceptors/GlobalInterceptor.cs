using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcHost.Instrumentation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenTracing;

namespace GrpcHost.Core.Interceptors
{
    // Note: When chaining the interceptors (usually: Program.cs of gRPC service project) the last in the chain will be executed first - mime01
    internal class GlobalInterceptor : Interceptor
    {
        private readonly ILogger _logger;
        private readonly LoggingOptions _options;
        private readonly IInstrumentationContext _instrumentation;

        public GlobalInterceptor(
            ILogger logger,
            IOptionsMonitor<LoggingOptions> options,
            IInstrumentationContext instrumentation)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.CurrentValue ?? new LoggingOptions();
            _instrumentation = instrumentation ?? throw new ArgumentNullException(nameof(instrumentation));
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _instrumentation.RegisterCorellationId(context);

            LogContract(request, context.Method, _options.RequestLoggingOptions);

            TResponse response;

            try
            {
                using (IScope scope = _instrumentation.CreateServerSpan(context))
                {
                    response = await base.UnaryServerHandler(request, context, continuation).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Method} {ErrorMessage}", ex.TargetSite?.Name ?? "Not set", ex.Message);

                throw;
            }

            LogContract(response, context.Method, _options.ResponseLoggingOptions, false);

            return response;
        }

        private void LogContract<T>(T instance, string methodName, LoggingOptions.TypeLoggingOptions loggingOptions, bool isRequest = true)
        {
            var type = instance.GetType();
            var typeToLog = loggingOptions.PropertyFilter.FirstOrDefault(x => x.TypeName == type.Name);

            var propertyNames = new List<string>();

            if (typeToLog != null || loggingOptions.LogAll)
                propertyNames.AddRange(loggingOptions.WellKnownProperties);

            if (typeToLog != null)
                propertyNames.AddRange(typeToLog.PropertyNames);

            (string message, List<object> parameters) = ("{Method} ", new List<object> { methodName });

            if (!propertyNames.Any())
            {
                _logger.LogInformation(message, parameters.ToArray());

                return;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

            if (!properties.Any())
            {
                _logger.LogInformation(message, parameters.ToArray());

                return;
            }

            foreach (var name in propertyNames)
            {
                if (name == "*")
                {
                    message += isRequest ? "{Request} " : "{Response }";
                    parameters.Add(JsonConvert.SerializeObject(instance, Formatting.None));

                    break;
                }

                var property = properties.FirstOrDefault(x => x.Name == name);

                if (property == null)
                    continue;

                object value = property?.GetValue(instance);

                if (value == null)
                    continue;

                message += "{" + name + "}";

                if (property.PropertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Length > 0)
                    parameters.Add(JsonConvert.SerializeObject(value, Formatting.None));
                else
                    parameters.Add(value.ToString());
            }

            _logger.LogInformation(message, parameters.ToArray());
        }
    }
}
