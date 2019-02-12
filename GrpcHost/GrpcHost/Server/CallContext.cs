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
        private const string HeaderName = "correlation-id";
        private readonly AsyncLocal<string> _id;

        public CallContext()
        {
            _id = new AsyncLocal<string>(OnValueChanged);
        }

        private static void OnValueChanged(AsyncLocalValueChangedArgs<string> obj)
        {

        }

        private string _methodName;

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

            var correlationId = context.RequestHeaders.FirstOrDefault(x => x.Key == HeaderName);

            if (correlationId == null)
            {
                _id.Value = Random().ToString("x");
                context.RequestHeaders.Add(HeaderName, _id.Value);
            }

            if (string.IsNullOrWhiteSpace(_id.Value))
                _id.Value = correlationId.Value;
        }

        private static ulong Random()
        {
            var guid = Guid.Parse(Guid.NewGuid().ToString());
            var bytes = guid.ToByteArray();

            return BitConverter.ToUInt64(bytes, 0);
        }
    }
}
