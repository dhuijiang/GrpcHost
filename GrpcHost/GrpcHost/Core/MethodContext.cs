using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Core.Interceptors;
using ServiceDescriptor = Google.Protobuf.Reflection.ServiceDescriptor;

namespace GrpcHost.Core
{
    public interface IMethodContext
    {
        string GetServiceName();
        IEnumerable<ServerServiceDefinition> GetDefinitions();
    }

    public sealed class MethodContext<TRequest, TResponse, TServiceImpl> : IMethodContext
        where TRequest : class
        where TResponse : class
        where TServiceImpl : class
    {
        private readonly Lazy<ServiceDescriptor> ServiceDescriptor = new Lazy<ServiceDescriptor>(GetServiceDescriptor);

        private readonly TServiceImpl _instance;
        private readonly Interceptor[] _interceptors;

        public MethodContext(TServiceImpl instance) : this(instance, null)
        {
        }

        public MethodContext(TServiceImpl instance, params Interceptor[] interceptors)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _interceptors = interceptors;
        }

        public string GetServiceName()
        {
            return ServiceDescriptor.Value.Name;
        }

        public IEnumerable<ServerServiceDefinition> GetDefinitions()
        {
            var descriptors = GetMethodDescriptors();

            foreach (var descriptor in descriptors)
            {
                var methodType = descriptor.GetMethodType();
                var method = CreateMethod(descriptor, methodType);

                var definition = BuildDefinitions(methodType, method, _instance);

                if (_interceptors != null)
                    definition = definition.Intercept(_interceptors);

                yield return definition;
            }

            IEnumerable<MethodDescriptor> GetMethodDescriptors()
            {
                return
                    ServiceDescriptor.Value.Methods.Where(
                        x =>
                            x.InputType.ClrType == typeof(TRequest) &&
                            x.OutputType.ClrType == typeof(TResponse));
            }
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

        private static ServerServiceDefinition BuildDefinitions(MethodType methodType, Method<TRequest, TResponse> method, TServiceImpl instance)
        {
            var builder = ServerServiceDefinition.CreateBuilder();

            switch (methodType)
            {
                case MethodType.Unary:
                    builder.AddMethod(method, CreateGrpcDelegate<UnaryServerMethod<TRequest, TResponse>>());
                    break;

                case MethodType.ClientStreaming:
                    builder.AddMethod(method, CreateGrpcDelegate<ClientStreamingServerMethod<TRequest, TResponse>>());
                    break;

                case MethodType.ServerStreaming:
                    builder.AddMethod(
                        method, CreateGrpcDelegate<ServerStreamingServerMethod<TRequest, TResponse>>());
                    break;

                case MethodType.DuplexStreaming:
                    builder.AddMethod(method, CreateGrpcDelegate<DuplexStreamingServerMethod<TRequest, TResponse>>());
                    break;
            }

            return builder.Build();

            TDelegate CreateGrpcDelegate<TDelegate>() where TDelegate : Delegate
            {
                var grpcDelegate = Delegate.CreateDelegate(typeof(TDelegate), instance, method.Name);

                return (TDelegate)grpcDelegate;
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
