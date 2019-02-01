using System;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcHost.ServiceDefiners
{
    public class ClientStreamingServiceDefiner<TRequest, TResponse> : ServiceDefiner<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        public ClientStreamingServiceDefiner(MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target)
            : this(descriptor, target, null)
        {
        }

        public ClientStreamingServiceDefiner(MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target, params Interceptor[] interceptors)
            : base(MethodType.ClientStreaming, descriptor, target, interceptors)
        {
        }
    }
}