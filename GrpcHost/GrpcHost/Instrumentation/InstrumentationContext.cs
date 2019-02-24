using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Grpc.Core;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace GrpcHost.Instrumentation
{
    public interface IInstrumentationContext
    {
        void RegisterCorellationId(ServerCallContext context);

        IScope CreateServerSpan(ServerCallContext context);

        IDictionary<string, string> GetTracingHeaders();

        string GetCorrelationId();

        (string name, string value) GetCorrelationHeader();
    }

    public sealed class InstrumentationContext : IInstrumentationContext
    {
        private const string HeaderName = "correlation-id";
        private readonly AsyncLocal<string> _id = new AsyncLocal<string>();
        private readonly ITracer _tracer;

        public InstrumentationContext(ITracer tracer)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public void RegisterCorellationId(ServerCallContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            if (!string.IsNullOrWhiteSpace(_id.Value))
                throw new ArgumentException("Correlation Id is already initialized.");

            var correlationId = context.RequestHeaders.FirstOrDefault(x => x.Key == HeaderName);

            if (correlationId == null)
            {
                _id.Value = Random().ToString("x");
                context.RequestHeaders.Add(HeaderName, _id.Value);
            }

            if (string.IsNullOrWhiteSpace(_id.Value))
                _id.Value = correlationId.Value;
        }

        public IScope CreateServerSpan(ServerCallContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            var operationName = context.Method.Split('/').Last();
            ISpanBuilder spanBuilder;

            try
            {
                var headers = context.RequestHeaders.ToDictionary(x => x.Key, x => x.Value);
                ISpanContext parentSpanCtx = _tracer.Extract(BuiltinFormats.HttpHeaders, new TextMapExtractAdapter(headers));

                spanBuilder = _tracer.BuildSpan(operationName);
                spanBuilder = parentSpanCtx != null ? spanBuilder.AsChildOf(parentSpanCtx) : spanBuilder;
            }
            catch (Exception)
            {
                spanBuilder = _tracer.BuildSpan(operationName);
            }

            return
                spanBuilder
                    .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                    .WithTag(Tags.PeerHostname, context.Host)
                    .WithTag("correlation-id", GetCorrelationId())
                    .StartActive(true);
        }

        public string GetCorrelationId() => _id.Value;

        public (string, string) GetCorrelationHeader() => (HeaderName, GetCorrelationId());

        public IDictionary<string, string> GetTracingHeaders()
        {
            var dictionary = new Dictionary<string, string>();
            _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));

            return dictionary;
        }

        private static ulong Random()
        {
            var guid = Guid.Parse(Guid.NewGuid().ToString());
            var bytes = guid.ToByteArray();

            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
