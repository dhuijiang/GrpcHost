using System;
using Grpc.Core;
using GrpcHost.Instrumentation;

namespace GrpcHost.Core.Invokers
{
    internal class GlobalCallInvoker : DefaultCallInvoker
    {
        private readonly IInstrumentationContext _context;

        public GlobalCallInvoker(Channel channel, IInstrumentationContext instrumentation) : base(channel)
        {
            _context = instrumentation ?? throw new ArgumentNullException(nameof(instrumentation));
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return base.AsyncClientStreamingCall(method, host, options = options.WithCorrelationHeader(_context).WithTraceId(_context));
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return base.AsyncDuplexStreamingCall(method, host, options = options.WithCorrelationHeader(_context).WithTraceId(_context));
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return base.AsyncServerStreamingCall(method, host, options = options.WithCorrelationHeader(_context).WithTraceId(_context), request);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return base.AsyncUnaryCall(method, host, options = options.WithCorrelationHeader(_context).WithTraceId(_context), request);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return base.BlockingUnaryCall(method, host, options.WithCorrelationHeader(_context), request);
        }
    }

    internal static class CallOptionsExtensions
    {
        internal static CallOptions WithCorrelationHeader(this CallOptions options, IInstrumentationContext context)
        {
            options = options.Headers == null
                ? options.WithHeaders(new Metadata())
                : options;

            (string name, string value) = context.GetCorrelationHeader();
            options.Headers.Add(new Metadata.Entry(name, value));

            return options;
        }

        internal static CallOptions WithTraceId(this CallOptions options, IInstrumentationContext context)
        {
            var headers = context.GetTracingHeaders();

            foreach (var header in headers)
            {
                options.Headers.Add(header.Key, header.Value);
            }

            return options;
        }
    }
}