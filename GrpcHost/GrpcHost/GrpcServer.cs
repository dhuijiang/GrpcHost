using System;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Options;

namespace GrpcHost
{
    public class GrpcServer : Server
    {
        private readonly ServerServiceDefinition _definition;
        private readonly HealthServiceImpl _healthServiceImpl = new HealthServiceImpl();

        public GrpcServer(ServerServiceDefinition definition)
        {
            _definition = definition;
        }

        public new void Start()
        {
            Services.Add(_definition);
            //foreach(var definer in _definition)
            //{
            //    Services.Add(definer);
            //    //_healthServiceImpl.SetStatus(definer.ServiceName, HealthCheckResponse.Types.ServingStatus.Serving);
            //}

            // TODO: Move to options.
            Ports.Add("localhost", 5000, ServerCredentials.Insecure);

            _healthServiceImpl.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);
            Services.Add(Health.BindService(_healthServiceImpl));

            base.Start();
        }
    }
}
