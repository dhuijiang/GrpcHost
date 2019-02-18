using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using GrpcHost.Interceptors;
using GrpcHost.Methods;
using GrpcHost.Server;
using Microsoft.Extensions.Options;

using GrpcHealth = Grpc.Health.V1.Health;

namespace GrpcHost
{
    // This supports only one global interceptor, a better solution is needed.
    internal sealed class GrpcServer : Grpc.Core.Server
    {
        private readonly HostOptions _options;
        private readonly IEnumerable<IMethodContext> _contexts;
        private readonly HealthServiceImpl _healthService;
        private readonly GlobalInterceptor _globalInterceptor;

        public GrpcServer(IOptions<HostOptions> options, HealthServiceImpl healthService, GlobalInterceptor globalInterceptor, MethodRegistry methodRegistry)
        {
            _options = options.Value ?? new HostOptions();
            _contexts = methodRegistry == null ? Enumerable.Empty<IMethodContext>() : methodRegistry.RegisteredMethods;
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

            Services.Add(GrpcHealth.BindService(_healthService));

            base.Start();
        }
    }
}