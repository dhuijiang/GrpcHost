using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using GrpcHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GrpcLogLevel = Grpc.Core.Logging.LogLevel;

namespace Techsson.Gaming.Infrastructure.Grpc.Host
{
    /// <summary>
    /// Represents implementation of <see cref="IHostedService"/> specialized for hosting gRPC services.
    /// </summary>
    internal class GrpcHostedService : IHostedService
    {
        private static readonly Lazy<Process> _bashProcess = new Lazy<Process>(GetBashProcess);

        private readonly IApplicationLifetime _applicationLifetime;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private GrpcServer _server;

        /// <summary>
        /// Initializes new instance of <see cref="GrpcHostedService"/>
        /// </summary>
        /// <param name="server">Instance of <see cref="Server"/>.</param>
        /// <param name="clientCache" Instance of <see cref="IGrpcClientCache"/>.
        /// <param name="definitions">Dictionary that should contain gRPC service implementation that need to be registered with server.</param>
        /// <param name="applicationLifetime">Instance of <see cref="IApplicationLifetime"/>.</param>
        /// <param name="logger">Instance of <see cref="ILogger"/> that will be configured to use Serilog Splunk of Console sink.</param>
        public GrpcHostedService(
            GrpcServer server,
            IApplicationLifetime applicationLifetime,
            Microsoft.Extensions.Logging.ILogger logger)
        {
            _server = server;
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(OnStarted);
            _applicationLifetime.ApplicationStopping.Register(OnStopping);

            GrpcEnvironment.SetLogger(new LogLevelFilterLogger(new GrpcLogger(_logger), GrpcLogLevel.Debug, false));
            GrpcEnvironment.Logger.Info("Testing gRPC Logger.");

            _logger.LogInformation("Server is starting.", null);

            _server.Start();

            string serverAddress = _server.Ports.Select(p => string.Format("{0}:{1}", p.Host, p.Port.ToString())).First();
            _logger.LogInformation($"Server running on: {serverAddress}", null);

            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _logger.LogInformation($"OS: {RuntimeInformation.OSDescription}: {RuntimeInformation.OSArchitecture}{Environment.NewLine} .Net: {RuntimeInformation.FrameworkDescription}", null);

                return;
            }

            ExecuteBashCommand("cat /etc/os-release", _logger);
            ExecuteBashCommand("uname -a", _logger);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Server has stopped.", null);

            return Task.CompletedTask;
        }

        private void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.", null);

            _server.ShutdownAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static void ExecuteBashCommand(string command, Microsoft.Extensions.Logging.ILogger logger)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return;

            var process = _bashProcess.Value;
            process.StartInfo.Arguments = $"-c \"{command}\"";

            try
            {
                process.Start();
                string result = _bashProcess.Value.StandardOutput.ReadToEnd();
                process.WaitForExit();

                logger.LogInformation(result, null);

                process.Close();
            }
            catch
            {
                logger.LogInformation("Bash not installed.", null);
            }
            finally
            {
                process?.Close();
            }

        }

        private static Process GetBashProcess()
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/bash",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}