using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcHost
{
    public interface IMethodContext
    {
        string GetServiceName();
        ServerServiceDefinition GetDefinition();
    }

    public class MethodContext<TRequest, TResponse, TServiceImpl> : IMethodContext
        where TRequest : class
        where TResponse : class
    {
        private readonly MethodType _methodType;
        private readonly string _methodName;
        private readonly TServiceImpl _instance;
        private readonly Interceptor[] _interceptors;

        private readonly MethodDescriptor _descriptor;

        public MethodContext(MethodType methodType, string methodName, TServiceImpl instance, params Interceptor[] interceptors)
        {
            _methodType = methodType;
            _methodName = methodName;
            _instance = instance;
            _interceptors = interceptors;

            _descriptor = GetDesciptor<TServiceImpl>(methodName);
        }

        public string GetServiceName() => _descriptor.Service.FullName;

        public ServerServiceDefinition GetDefinition()
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var method = CreateMethod(_methodType, _descriptor);

            if (!_descriptor.IsClientStreaming && !_descriptor.IsServerStreaming)
                builder.AddMethod(
                    method,
                    new UnaryServerMethod<TRequest, TResponse>(BuildHandler<Func<TRequest, ServerCallContext, Task<TResponse>>>(_instance, _methodName, GetUnaryParameters())));

            if (_descriptor.IsClientStreaming && !_descriptor.IsServerStreaming)
                builder.AddMethod(
                    method, 
                    new ClientStreamingServerMethod<TRequest, TResponse>(BuildHandler<Func<IAsyncStreamReader<TRequest>, ServerCallContext, Task<TResponse>>>(_instance, _methodName, GetClientStreamingParameters())));

            if (!_descriptor.IsClientStreaming && _descriptor.IsServerStreaming)
                builder.AddMethod(
                    method, 
                    new ServerStreamingServerMethod<TRequest, TResponse>(BuildHandler<Func<TRequest, IServerStreamWriter<TResponse>, ServerCallContext, Task>>(_instance, _methodName, GetServerStreamingParameters())));

            if (_descriptor.IsClientStreaming && _descriptor.IsServerStreaming)
                builder.AddMethod(
                    method, 
                    new DuplexStreamingServerMethod<TRequest, TResponse>(BuildHandler<Func<IAsyncStreamReader<TRequest>, IServerStreamWriter<TResponse>, ServerCallContext, Task>>(_instance, _methodName, GetDuplexStreamingParameters())));

            var definition = builder.Build();

            if (_interceptors == null)
                return definition;

            return definition.Intercept(_interceptors);
        }

        private static ParameterExpression[] GetUnaryParameters()
        {
            return
                new ParameterExpression[]
                {
                    Expression.Parameter(typeof(TRequest)),
                    Expression.Parameter(typeof(ServerCallContext))
                };
        }

        private static ParameterExpression[] GetClientStreamingParameters()
        {
            return new ParameterExpression[]
            {
                Expression.Parameter(typeof(IAsyncStreamReader<TRequest>)),
                Expression.Parameter(typeof(ServerCallContext))
            };
        }

        private static ParameterExpression[] GetServerStreamingParameters()
        {
            return new ParameterExpression[]
            {
                Expression.Parameter(typeof(TRequest)),
                Expression.Parameter(typeof(IServerStreamWriter<TResponse>)),
                Expression.Parameter(typeof(ServerCallContext))
            };
        }

        private static ParameterExpression[] GetDuplexStreamingParameters()
        {
            return new ParameterExpression[]
            {
                Expression.Parameter(typeof(IAsyncStreamReader<TRequest>)),
                Expression.Parameter(typeof(IServerStreamWriter<TResponse>)),
                Expression.Parameter(typeof(ServerCallContext))
            };
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

        private static MethodDescriptor GetDesciptor<TService>(string methodName)
        {
            var serviceType = typeof(TService);
            var baseServiceType = GetBaseType(serviceType);
            var serviceDefinitionType = typeof(MethodDescriptor);
            var serviceContainerType = baseServiceType.DeclaringType;
            var descriptor = (Google.Protobuf.Reflection.ServiceDescriptor)serviceContainerType.GetProperty("Descriptor").GetValue(serviceContainerType);
            var methodDescriptor = descriptor.FindMethodByName(methodName);

            return methodDescriptor;

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

        private static TDelegate BuildHandler<TDelegate>(TServiceImpl instance, string methodName, ParameterExpression[] parameters)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName);

            var call = Expression.Call(Expression.Constant(instance), method, parameters);

            var func =
                Expression.Lambda<TDelegate>(
                    call,
                    false,
                    parameters)
                .Compile();

            return func;
        }
    }
}