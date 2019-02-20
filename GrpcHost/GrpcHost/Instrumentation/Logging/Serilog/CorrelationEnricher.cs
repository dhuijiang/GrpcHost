using System;
using Serilog.Core;
using Serilog.Events;

namespace GrpcHost.Instrumentation.Logging
{
    internal class CorrelationEnricher : ILogEventEnricher
    {
        private readonly ICorrelationContext _context;

        public CorrelationEnricher(ICorrelationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationid = _context.GetCorrelationId();
            if (string.IsNullOrWhiteSpace(correlationid))
                return;

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationid));
        }
    }
}