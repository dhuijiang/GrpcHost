using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using GrpcHost.Interceptors;
using Microsoft.Extensions.Options;

namespace GrpcHost
{
    public sealed class GrpcServer : Server
    {
        private readonly HostOptions _options;
        private readonly IMethodResolver _methodResolver;
        private readonly HealthServiceImpl _healthService;
        private readonly ExceptionInterceptor _globalInterceptor;

        public GrpcServer(IOptions<HostOptions> options, IMethodResolver methodResolver, ExtendedHealthServiceImpl healthService, ExceptionInterceptor globalInterceptor)
        {
            _options = options.Value ?? new HostOptions();
            _methodResolver = methodResolver;
            _healthService = healthService;
            _globalInterceptor = globalInterceptor;
        }

        public new void Start()
        {
            Ports.Add(_options.Host, _options.Port, ServerCredentials.Insecure);
            _healthService.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);

            foreach (var context in _methodResolver.RegisteredMethods)
            {
                Services.Add(context.GetDefinition().Intercept(_globalInterceptor));

                _healthService.SetStatus(context.GetServiceName(), HealthCheckResponse.Types.ServingStatus.Serving);
            }

            Services.Add(Health.BindService(_healthService));

            base.Start();
        }
    }
}