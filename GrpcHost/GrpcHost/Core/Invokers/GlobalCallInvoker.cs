using System;
using Grpc.Core;
using GrpcHost.Instrumentation;

namespace GrpcHost.Core.Invokers
{
    internal class GlobalCallInvoker : DefaultCallInvoker
    {
        private readonly ICorrelationContext _context;

        public GlobalCallInvoker(Channel channel, ICorrelationContext context) : base(channel)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return base.AsyncClientStreamingCall(method, host, options.WithCorrelationHeader(_context));
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return base.AsyncDuplexStreamingCall(method, host, options.WithCorrelationHeader(_context));
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return base.AsyncServerStreamingCall(method, host, options.WithCorrelationHeader(_context), request);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return base.AsyncUnaryCall(method, host, options.WithCorrelationHeader(_context), request);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return base.BlockingUnaryCall(method, host, options.WithCorrelationHeader(_context), request);
        }
    }

    internal static class CallOptionsExtensions
    {
        internal static CallOptions WithCorrelationHeader(this CallOptions options, ICorrelationContext context)
        {
            options = options.Headers == null
                ? options.WithHeaders(new Metadata())
                : options;

            (string name, string value) = context.CreateCorrelationHeader();
            options.Headers.Add(new Metadata.Entry(name, value));

            return options;
        }
    }
}