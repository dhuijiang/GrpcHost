﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        private readonly string _methodName;
        private readonly TServiceImpl _instance;
        private readonly Interceptor[] _interceptors;

        private readonly MethodType _methodType;
        private readonly MethodDescriptor _descriptor;

        public MethodContext(TServiceImpl instance) : this(instance, null)
        {
        }

        public MethodContext(TServiceImpl instance, params Interceptor[] interceptors)
        {
            _instance = instance;
            _interceptors = interceptors;

            (_descriptor, _methodType, _methodName) = GetMethodMetadata<TServiceImpl>();
        }


        public string GetServiceName() => _descriptor.Service.FullName;

        public ServerServiceDefinition GetDefinition()
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            var method = CreateMethod(_descriptor, _methodType);

            switch (_methodType)
            {
                case MethodType.Unary:
                    builder.AddMethod(method, CreateDelegate<UnaryServerMethod<TRequest, TResponse>>(_instance, _methodName));
                    break;

                case MethodType.ClientStreaming:
                    builder.AddMethod(method, CreateDelegate<ClientStreamingServerMethod<TRequest, TResponse>>(_instance, _methodName));

                    break;
                case MethodType.ServerStreaming:
                    builder.AddMethod(
                        method, CreateDelegate<ServerStreamingServerMethod<TRequest, TResponse>>(_instance, _methodName));

                    break;
                case MethodType.DuplexStreaming:
                    builder.AddMethod(
                        method, CreateDelegate<DuplexStreamingServerMethod<TRequest, TResponse>>(_instance, _methodName));

                    break;
            }

            var definition = builder.Build();

            if (_interceptors == null)
                return definition;

            return definition.Intercept(_interceptors);
        }

        private static TDel CreateDelegate<TDel>(TServiceImpl instance, string methodName) where TDel : Delegate
        {
            var del = Delegate.CreateDelegate(typeof(TDel), instance, methodName);

            return (TDel)del;
        }

        private static (MethodDescriptor, MethodType, string) GetMethodMetadata<TService>()
        {
            var baseServiceType = GetBaseType(typeof(TService));
            var serviceContainerType = baseServiceType.DeclaringType;
            var serviceDescriptor = (ServiceDescriptor)serviceContainerType.GetProperty("Descriptor").GetValue(serviceContainerType);
            var methodDescriptor = serviceDescriptor.Methods.FirstOrDefault(x => x.InputType.ClrType == typeof(TRequest) && x.OutputType.ClrType == typeof(TResponse));

            return (methodDescriptor, ResolveMethodType(), methodDescriptor.Name);

            Type GetBaseType(Type type)
            {
                var baseType = type;

                if (!baseType.BaseType.Equals(typeof(object)))
                    return GetBaseType(baseType.BaseType);

                return baseType;
            }

            MethodType ResolveMethodType()
            {
                if (methodDescriptor.IsClientStreaming && methodDescriptor.IsServerStreaming)
                    return MethodType.DuplexStreaming;
                else if (methodDescriptor.IsClientStreaming)
                    return MethodType.ClientStreaming;
                else if (methodDescriptor.IsServerStreaming)
                    return MethodType.ServerStreaming;
                else
                    return MethodType.Unary;
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

        private static TDelegate BuildHandler<TDelegate>(TServiceImpl instance, string methodName)
        {
            MethodInfo method = instance.GetType().GetMethod(methodName);
            IEnumerable<ParameterExpression> parameters = method.GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToList();

            var call = Expression.Call(Expression.Constant(instance), method, parameters);
            var func = Expression.Lambda<TDelegate>(call, false, parameters).Compile();

            return func;
        }
    }
}