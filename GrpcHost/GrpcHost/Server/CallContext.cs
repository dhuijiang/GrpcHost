using System;
using System.Linq;
using System.Threading;
using Grpc.Core;

namespace GrpcHost.Server
{
    internal interface ICallContext
    {
        void RegisterCorellationId(ServerCallContext context);

        string GetCorrelationId();

        string GetMethodName();
    }

    internal class CallContext : ICallContext
    {
        private static int Counter = 0;
        private static readonly AsyncLocal<string> _id = new AsyncLocal<string>();
        private string _methodName;

        public CallContext()
        {
            Counter++;
        }

        public string GetCorrelationId()
        {
            return _id.Value;
        }

        public string GetMethodName()
        {
            return _methodName;
        }

        public void RegisterCorellationId(ServerCallContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _methodName = context.Method;

            if (!string.IsNullOrWhiteSpace(_id.Value))
                throw new ArgumentException("Correlation Id is already initialized.");

            var correlationId = context.RequestHeaders.FirstOrDefault(x => x.Key == "correlation-id");

            // Header value cannot be null.
            if (correlationId != null)
            {
                _id.Value = correlationId.Value;

                return;
            }

            _id.Value = Guid.NewGuid().ToString();
            context.RequestHeaders.Add("correlation-id", _id.Value);
        }
    }
}
