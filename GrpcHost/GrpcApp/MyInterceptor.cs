using Grpc.Core;
using Grpc.Core.Interceptors;

namespace CustomerGrpcService
{
    public class MyInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return base.AsyncUnaryCall(request, context, continuation);
        }
    }
}
