using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Propagation;

namespace GrpcHost.Instrumentation
{
    public interface IInstrumentationContext
    {
        IDictionary<string, string> GetTracingHeaders();

        string GetTraceId();

        (string name, string value) GetCorrelationHeader();
    }

    public sealed class InstrumentationContext : IInstrumentationContext
    {
        private readonly ICorrelationContext _correlation;
        private readonly ITracer _tracer;

        public InstrumentationContext(ICorrelationContext correlation, ITracer tracer)
        {
            _correlation = correlation ?? throw new ArgumentNullException(nameof(correlation));
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public IDictionary<string, string> GetTracingHeaders()
        {
            var dictionary = new Dictionary<string, string>();
            _tracer.Inject(_tracer.ActiveSpan.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));

            return dictionary;
        }

        public string GetTraceId()
        {
            return _tracer.ActiveSpan.Context.TraceId;
        }

        public (string, string) GetCorrelationHeader()
        {
            return ("correlation-id", GetTraceId());
        }
    }
}
