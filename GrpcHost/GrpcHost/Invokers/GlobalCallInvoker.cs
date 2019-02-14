using System;
using Grpc.Core;
using GrpcHost.Server;

namespace GrpcHost.Invokers
{
    internal class GlobalCallInvoker : DefaultCallInvoker
    {
        private readonly ICallContext _context;

        public GlobalCallInvoker(Channel channel, ICallContext context) 
            : base(channel)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            if (options.Headers == null)
                options = options.WithHeaders(new Metadata());

            options.Headers.Add(AddCorelationHeaderFromLocalStorage(_context));

            return base.AsyncClientStreamingCall(method, host, options);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            if (options.Headers == null)
                options = options.WithHeaders(new Metadata());

            options.Headers.Add(AddCorelationHeaderFromLocalStorage(_context));

            return base.AsyncDuplexStreamingCall(method, host, options);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            if (options.Headers == null)
                options = options.WithHeaders(new Metadata());

            options.Headers.Add(AddCorelationHeaderFromLocalStorage(_context));

            return base.AsyncServerStreamingCall(method, host, options, request);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            if (options.Headers == null)
                options = options.WithHeaders(new Metadata());

            options.Headers.Add(AddCorelationHeaderFromLocalStorage(_context));

            return base.AsyncUnaryCall(method, host, options, request);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            if (options.Headers == null)
                options = options.WithHeaders(new Metadata());

            options.Headers.Add(AddCorelationHeaderFromLocalStorage(_context));

            return base.BlockingUnaryCall(method, host, options, request);
        }

        private static Metadata.Entry AddCorelationHeaderFromLocalStorage(ICallContext context)
        {
            var (name, value) = context.CreateCorrelationHeader();

            return new Metadata.Entry(name, value);
        }
    }
}