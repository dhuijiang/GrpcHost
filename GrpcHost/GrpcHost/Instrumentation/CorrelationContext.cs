using System;
using System.Linq;
using System.Threading;
using Grpc.Core;

namespace GrpcHost.Instrumentation
{
    public interface ICorrelationContext
    {
        void RegisterCorellationId(ServerCallContext context);

        string GetCorrelationId();

        (string name, string value) CreateCorrelationHeader();
    }

    internal class CorrelationContext : ICorrelationContext
    {
        private const string HeaderName = "correlation-id";
        private readonly AsyncLocal<string> _id = new AsyncLocal<string>();

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

        public (string, string) CreateCorrelationHeader()
        {
            return (HeaderName, GetCorrelationId());
        }

        public string GetCorrelationId()
        {
            return _id.Value;
        }

        private static ulong Random()
        {
            var guid = Guid.Parse(Guid.NewGuid().ToString());
            var bytes = guid.ToByteArray();

            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}