using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.GrpcHost.Interceptors
{
    public class ExceptionInterceptor : Interceptor
    {
        private readonly ILogger _logger;

        public ExceptionInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await base.UnaryServerHandler(request, context, continuation).ConfigureAwait(false);
            }
            catch(RpcException ex)
            {
                _logger.LogError(ex, "{Method} {ErrorMessage}", ex.TargetSite?.Name ?? "Not set", ex.Message);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message), context.ResponseTrailers, $"{context.Method} failed");
            }
        }
    }
}