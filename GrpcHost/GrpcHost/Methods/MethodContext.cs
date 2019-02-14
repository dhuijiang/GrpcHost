using System;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceDescriptor = Google.Protobuf.Reflection.ServiceDescriptor;

namespace GrpcHost.Methods
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
        private static readonly Lazy<ServiceDescriptor> ServiceDescriptor = new Lazy<ServiceDescriptor>(GetServiceDescriptor);
        private static readonly Lazy<MethodDescriptor> MethodDescriptor = new Lazy<MethodDescriptor>(GetMethodDescriptor);

        private readonly TServiceImpl _instance;
        private readonly Interceptor[] _interceptors;

        public MethodContext(TServiceImpl instance) : this(instance, null)
        {
        }

        public MethodContext(TServiceImpl instance, params Interceptor[] interceptors)
        {
            _instance = instance;
            _interceptors = interceptors;
        }


        public string GetServiceName()
        {
            return ServiceDescriptor.Value.Name;
        }

        public ServerServiceDefinition GetDefinition()
        {
            var descriptor = MethodDescriptor.Value;
            var methodType = descriptor.GetMethodType();
            var method = CreateMethod(descriptor, methodType);

            var builder = ServerServiceDefinition.CreateBuilder();

            switch (methodType)
            {
                case MethodType.Unary:
                    builder.AddMethod(method, CreateGrpcDelegate<UnaryServerMethod<TRequest, TResponse>>(_instance, descriptor.Name));
                    break;

                case MethodType.ClientStreaming:
                    builder.AddMethod(method, CreateGrpcDelegate<ClientStreamingServerMethod<TRequest, TResponse>>(_instance, descriptor.Name));
                    break;

                case MethodType.ServerStreaming:
                    builder.AddMethod(
                        method, CreateGrpcDelegate<ServerStreamingServerMethod<TRequest, TResponse>>(_instance, descriptor.Name));
                    break;

                case MethodType.DuplexStreaming:
                    builder.AddMethod(method, CreateGrpcDelegate<DuplexStreamingServerMethod<TRequest, TResponse>>(_instance, descriptor.Name));
                    break;
            }

            var definition = builder.Build();

            if (_interceptors == null)
                return definition;

            return definition.Intercept(_interceptors);
        }

        private static TDelegate CreateGrpcDelegate<TDelegate>(TServiceImpl instance, string methodName) where TDelegate : Delegate
        {
            var grpcDelegate = Delegate.CreateDelegate(typeof(TDelegate), instance, methodName);

            return (TDelegate)grpcDelegate;
        }

        private static MethodDescriptor GetMethodDescriptor()
        {
            var serviceDescriptor = ServiceDescriptor.Value;
            var methodDescriptor = serviceDescriptor.Methods.FirstOrDefault(x => x.InputType.ClrType == typeof(TRequest) && x.OutputType.ClrType == typeof(TResponse));

            if (methodDescriptor == null)
                throw new ArgumentException($"Method descriptor for: {typeof(TRequest).Name} and {typeof(TResponse).Name} couldn't be resolved.");

            return methodDescriptor;
        }

        private static ServiceDescriptor GetServiceDescriptor()
        {
            var serviceType = typeof(TServiceImpl);
            var baseServiceType = GetBaseType(serviceType);
            var serviceContainerType = baseServiceType.DeclaringType;
            var serviceDescriptor = (ServiceDescriptor)serviceContainerType.GetProperty("Descriptor").GetValue(serviceContainerType);

            return serviceDescriptor;

            Type GetBaseType(Type type)
            {
                var baseType = type;

                if (!baseType.BaseType.Equals(typeof(object)))
                    return GetBaseType(baseType.BaseType);

                return baseType;
            }
        }

        private static Method<TRequest, TResponse> CreateMethod(MethodDescriptor descriptor, MethodType methodType)
        {
            _ = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

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

    internal static class MethodDescriptorExtensions
    {
        internal static MethodType GetMethodType(this MethodDescriptor descriptor)
        {
            _ = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.IsClientStreaming && descriptor.IsServerStreaming)
                return MethodType.DuplexStreaming;

            if (descriptor.IsClientStreaming)
                return MethodType.ClientStreaming;

            if (descriptor.IsServerStreaming)
                return MethodType.ServerStreaming;

            return MethodType.Unary;
        }
    }
}
