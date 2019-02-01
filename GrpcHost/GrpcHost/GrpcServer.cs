using System;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Options;

namespace GrpcHost
{
    public class GrpcServer : Server
    {
        private readonly ServiceDefinerOptions _options;
        private readonly HealthServiceImpl _healthServiceImpl = new HealthServiceImpl();

        public GrpcServer(IOptions<ServiceDefinerOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public new void Start()
        {
            foreach(var definer in _options.Definers)
            {
                Services.Add(definer.Definition);
                _healthServiceImpl.SetStatus(definer.ServiceName, HealthCheckResponse.Types.ServingStatus.Serving);
            }

            // TODO: Move to options.
            Ports.Add("localhost", 5000, ServerCredentials.Insecure);

            _healthServiceImpl.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);
            Services.Add(Health.BindService(_healthServiceImpl));

            base.Start();
        }
    }
}
