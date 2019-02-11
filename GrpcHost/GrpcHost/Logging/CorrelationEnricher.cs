using System;
using GrpcHost.Server;
using Serilog.Core;
using Serilog.Events;

namespace GrpcHost.Logging
{
    internal class CorrelationEnricher : ILogEventEnricher
    {
        private readonly ICallContext _context;

        public CorrelationEnricher(ICallContext context)
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