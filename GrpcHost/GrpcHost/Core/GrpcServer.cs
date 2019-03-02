using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using GrpcHost.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using GrpcHealth = Grpc.Health.V1.Health;

namespace GrpcHost.Core
{
    // TODO:  This supports only one global interceptor, a better solution is needed.
    internal sealed class GrpcServer : Server
    {
        private readonly HostOptions _options;
        private readonly MethodRegistry _methodRegistry;
        private readonly HealthServiceImpl _healthService;
        private readonly GlobalInterceptor _globalInterceptor;
        private readonly ILogger _logger;

        public GrpcServer(HealthServiceImpl healthService, GlobalInterceptor globalInterceptor, ILogger logger)
            : this(healthService, globalInterceptor, logger, Options.Create(new HostOptions()), MethodRegistry.Empty)
        {
        }

        public GrpcServer(HealthServiceImpl healthService, GlobalInterceptor globalInterceptor, ILogger logger, IOptions<HostOptions> options, MethodRegistry methodRegistry)
        {
            _healthService = healthService ?? throw new ArgumentNullException(nameof(healthService));
            _globalInterceptor = globalInterceptor ?? throw new ArgumentNullException(nameof(globalInterceptor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _methodRegistry = methodRegistry ?? throw new ArgumentNullException(nameof(methodRegistry));
        }

        public new void Start()
        {
            Ports.Add(_options.Host, _options.Port, ServerCredentials.Insecure);
            _healthService.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);

            foreach (IMethodContext context in _methodRegistry.RegisteredMethods)
            {
                foreach (var (methodName, definition) in context.GetDefinitions())
                {
                    Services.Add(definition.Intercept(_globalInterceptor));
                    _logger.LogDebug("Method {grpc-diag-method} registered.", methodName);
                    _healthService.SetStatus(context.GetServiceName(), HealthCheckResponse.Types.ServingStatus.Serving);
                }
            }

            Services.Add(GrpcHealth.BindService(_healthService));

            base.Start();
        }
    }
}