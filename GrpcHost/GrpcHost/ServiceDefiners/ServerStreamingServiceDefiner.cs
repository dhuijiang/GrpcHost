using System;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcHost.ServiceDefiners
{
    public class ServerStreamingServiceDefiner<TRequest, TResponse> : ServiceDefiner<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        private MethodDescriptor methodDescriptor;

        public ServerStreamingServiceDefiner(MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target)
            : this(descriptor, target, null)
        {
        }

        public ServerStreamingServiceDefiner(MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target, params Interceptor[] interceptors)
            : base(MethodType.ServerStreaming, descriptor, target, interceptors)
        {
        }
    }
}