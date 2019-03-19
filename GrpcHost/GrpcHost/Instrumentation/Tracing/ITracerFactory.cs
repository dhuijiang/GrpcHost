using OpenTracing;

namespace Techsson.Gaming.Infrastructure.Grpc.Instrumentation.Tracing
{
    public interface ITracerFactory
    {
        ITracer Create(string name);
    }
}