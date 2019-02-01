using System;
using System.Threading.Tasks;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcHost.ServiceDefiners
{
    public class DuplexStreamingServiceDefiner<TRequest, TResponse> : ServiceDefiner<TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        public DuplexStreamingServiceDefiner(MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target)
            : this(descriptor, target, null)
        {
        }

        public DuplexStreamingServiceDefiner(MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target, params Interceptor[] interceptors)
            : base(MethodType.DuplexStreaming, descriptor, target, interceptors)
        {
        }
    }
}