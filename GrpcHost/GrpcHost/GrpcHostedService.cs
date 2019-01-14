using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.GrpcHost.Interceptors;
using Microsoft.Extensions.Logging;

namespace GrpcHost
{
    public class GrpcHostedService : IHostedService
    {
        private readonly HealthServiceImpl _healthServiceImpl = new HealthServiceImpl();

        private readonly Server _server;
        private readonly Dictionary<string, ServerServiceDefinition> _definitions;
        private readonly IApplicationLifetime _applicationLifetime;
        private readonly ILogger<GrpcHostedService> _logger;

        public GrpcHostedService(
            Server server,
            Dictionary<string, ServerServiceDefinition> definitions,
            IApplicationLifetime applicationLifetime,
            ILogger<GrpcHostedService> logger)
        {
            _server = server;
            _definitions = definitions;
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);
            _applicationLifetime.ApplicationStopped.Register(OnStopped);

            _logger.LogInformation("Server is starting.");

            _healthServiceImpl.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);

            foreach (var definition in _definitions ?? new Dictionary<string, ServerServiceDefinition>())
            {
                _logger.LogInformation($"Setting service: {definition.Key} status to: Serving");

                _healthServiceImpl.SetStatus(definition.Key, HealthCheckResponse.Types.ServingStatus.Serving);
                definition.Value.Intercept(new ExceptionInterceptor(_logger));
                _server.Services.Add(definition.Value);
            }

            _server.Services.Add(Health.BindService(_healthServiceImpl));

            _server.Start();
            _logger.LogInformation($"Server running on: {_server.Ports.Select(p => $"{ p.Host}:{ p.BoundPort}")}");

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
            _healthServiceImpl.SetStatus("", HealthCheckResponse.Types.ServingStatus.NotServing);

            foreach (var definition in _definitions)
            {
                _logger.LogInformation($"Setting service: {definition.Key} status to: NotServing");
                _healthServiceImpl.SetStatus(definition.Key, HealthCheckResponse.Types.ServingStatus.NotServing);
            }
        }
    }
}