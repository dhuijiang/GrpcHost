using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Hosting.GrpcHost.Interceptors;
namespace GrpcHost
{
    public class GrpcServer : Server
    {
        private readonly HealthServiceImpl _healthServiceImpl = new HealthServiceImpl();

        public GrpcServer()
        {
            Ports.Add("localhost", 5000, ServerCredentials.Insecure);

            _healthServiceImpl.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);
            Services.Add(Health.BindService(_healthServiceImpl));
        }

        public GrpcServer InjectImplementation<TRequest, TResponse>(MethodDescriptor descriptor, UnaryServerMethod<TRequest, TResponse> handler, params Interceptor[] interceptors)
            where TRequest : class, IMessage<TRequest>
            where TResponse : class, IMessage<TResponse>
        {
            var definition = ServerServiceDefinition.CreateBuilder().AddMethod(CreateMethod<TRequest, TResponse>(descriptor), handler).Build();

            if(interceptors != null)
                definition = definition.Intercept(interceptors);

            Services.Add(definition);

            _healthServiceImpl.SetStatus(descriptor.Service.FullName, HealthCheckResponse.Types.ServingStatus.Serving);


            return this;
        }

        private static Method<TRequest, TResponse> CreateMethod<TRequest, TResponse>(MethodDescriptor descriptor) where TRequest : IMessage<TRequest> where TResponse : IMessage<TResponse>
        {
            return
                new Method<TRequest, TResponse>(
                    MethodType.Unary,
                    descriptor.Service.FullName,
                    descriptor.Name,
                    CreateMarshaller<TRequest>(descriptor.InputType.Parser),
                    CreateMarshaller<TResponse>(descriptor.OutputType.Parser));

            Marshaller<TContract> CreateMarshaller<TContract>(MessageParser parser) where TContract : IMessage<TContract>
            {
                return Marshallers.Create<TContract>(x => x.ToByteArray(), d => (TContract)parser.ParseFrom(d));
            }
        }
    }

    public class GrpcHostedService : IHostedService
    {
        private readonly GrpcServer _server;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger<GrpcHostedService> _logger;

        public GrpcHostedService(
            GrpcServer server,
            IApplicationLifetime applicationLifetime,
            ILogger<GrpcHostedService> logger)
        {
            _server = server;
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);

            _logger.LogInformation("Server is starting.");

            _server.Start();
            _logger.LogInformation($"Server running on: {_server.Ports.Select(p => $"{ p.Host}:{ p.BoundPort}").First()}");

            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            _logger.LogInformation("OnStarted triggered.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Server is stopping.");

            await StopAction(cancellationToken).ConfigureAwait(false);
        }

        public virtual Task StopAction(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public virtual void PreServerStopAction()
        {
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");

            // OnStopping get called before StopAsync
            UpdateHealthStatusToNotServing();

            PreServerStopAction();

            _server.ShutdownAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            _logger.LogInformation("OnStopping ended.");
        }

        // In case we need to send some sort of notification towards outside, otherwise we probably are never going to need this.
        private void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }

        private void UpdateHealthStatusToNotServing()
        {
            //_healthServiceImpl.SetStatus("", HealthCheckResponse.Types.ServingStatus.NotServing);

            ////foreach (var(serviceName, _) in GetDefinitions())
            ////{
            ////    _logger.LogInformation($"Setting service: {serviceName} status to: NotServing");
            ////    _healthServiceImpl.SetStatus(serviceName, HealthCheckResponse.Types.ServingStatus.NotServing);
            ////}
        }
    }
}