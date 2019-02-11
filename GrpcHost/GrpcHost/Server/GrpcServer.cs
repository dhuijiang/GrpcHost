using System.Collections.Generic;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using GrpcHost.Interceptors;
using GrpcHost.Methods;
using Microsoft.Extensions.Options;

namespace GrpcHost
{
    // This supports only one global interceptor, a better solution is needed.
    internal sealed class GrpcServer : Grpc.Core.Server
    {
        private readonly HostOptions _options;
        private readonly IEnumerable<IMethodContext> _contexts;
        private readonly ExtendedHealthServiceImpl _healthService;
        private readonly GlobalInterceptor _globalInterceptor;

        public GrpcServer(IOptions<HostOptions> options, ExtendedHealthServiceImpl healthService, GlobalInterceptor globalInterceptor)
        {
            _options = options.Value ?? new HostOptions();
            _contexts = options.Value.RegisteredMethods;
            _healthService = healthService;
            _globalInterceptor = globalInterceptor;
        }

        public new void Start()
        {
            Ports.Add(_options.Host, _options.Port, ServerCredentials.Insecure);
            _healthService.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);

            foreach (var context in _contexts)
            {
                Services.Add(context.GetDefinition().Intercept(_globalInterceptor));

                _healthService.SetStatus(context.GetServiceName(), HealthCheckResponse.Types.ServingStatus.Serving);
            }

            Services.Add(Health.BindService(_healthService));

            base.Start();
        }
    }
}