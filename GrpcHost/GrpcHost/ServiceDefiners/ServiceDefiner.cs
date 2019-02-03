using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public static Func<TService, ServerServiceDefinition> GetServiceBinder<TService>()
        {
            var serviceType = typeof(TService);
            var baseServiceType = GetBaseType(serviceType);
            var serviceDefinitionType = typeof(ServerServiceDefinition);

            var serviceContainerType = baseServiceType.DeclaringType;
            var methods = serviceContainerType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            var binder =
                (from m in methods
                 let parameters = m.GetParameters()
                 where m.Name.Equals("BindService") && parameters.Length == 1 && parameters.First()
                 .ParameterType.Equals(baseServiceType) && m.ReturnType.Equals(serviceDefinitionType)
                 select m)
            .SingleOrDefault();

            if (binder == null)
            {
                throw new InvalidOperationException($"Could not find service binder for provided service {serviceType.Name}");
            }

            var serviceParameter = Expression.Parameter(serviceType);

            var invocation = Expression.Call(null, binder, new[] { serviceParameter });

            var func =
                Expression.Lambda<Func<TService, ServerServiceDefinition>>(
                    invocation,
                    false,
                    new[] { serviceParameter })
                .Compile();

            return func;

            Type GetBaseType(Type type)
            {
                var objectType = typeof(object);
                var baseType = type;

                while (!baseType.BaseType.Equals(objectType))
                {
                    baseType = baseType.BaseType;
                }

                return baseType;
            }
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