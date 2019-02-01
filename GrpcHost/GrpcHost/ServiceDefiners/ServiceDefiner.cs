using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcHost
{
    public static class ServiceDefinerExtensions
    {
        public static IServiceCollection AddServiceDefiners(this IServiceCollection services)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.Configure<ServiceDefinerOptions>(x => x.Definers.AddRange(services.BuildServiceProvider().GetServices<IServiceDefiner>()));

            return services;
        }
    }

    public interface IServiceDefiner
    {
        string ServiceName { get; }
        ServerServiceDefinition Definition { get; }
    }

    public abstract class ServiceDefiner<TRequest, TResponse> : IServiceDefiner where TRequest : class where TResponse : class
    {
        private readonly MethodType _methodType;
        private readonly MethodDescriptor _descriptor;
        private readonly Func<TRequest, ServerCallContext, Task<TResponse>> _target;
        private readonly Interceptor[] _interceptors;

        public string ServiceName => _descriptor.Service.FullName;

        public ServerServiceDefinition Definition => CreateDefinition(_methodType, _descriptor, _target);

        public ServiceDefiner(MethodType methodType, MethodDescriptor descriptor, Func<TRequest, ServerCallContext, Task<TResponse>> target, params Interceptor[] interceptors)
        {
            _methodType = methodType;
            _descriptor = descriptor;
            _target = target;
            _interceptors = interceptors;
        }

        public ServerServiceDefinition CreateDefinition(
            MethodType methodType,
            MethodDescriptor descriptor,
            Func<TRequest, ServerCallContext, Task<TResponse>> target)
        {
            var definition =
                ServerServiceDefinition.CreateBuilder()
                    .AddMethod(CreateMethod(methodType, descriptor), (rq, ctx) => target(rq, ctx)).Build();

            if(_interceptors == null)
                return definition;

            return definition.Intercept(_interceptors);
        }

        private static Method<TRequest, TResponse> CreateMethod(MethodType methodType, MethodDescriptor descriptor)
        {
            return
                new Method<TRequest, TResponse>(
                    methodType,
                    descriptor.Service.FullName,
                    descriptor.Name,
                    CreateMarshaller<TRequest>(descriptor.InputType.Parser),
                    CreateMarshaller<TResponse>(descriptor.OutputType.Parser));

            Marshaller<T> CreateMarshaller<T>(MessageParser parser)
            {
                return Marshallers.Create(x => ((IMessage)x).ToByteArray(), d => (T)parser.ParseFrom(d));
            }
        }
    }
}