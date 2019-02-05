using System.Collections.Generic;
using System.Linq;
using GrpcHost.Methods;

namespace GrpcHost
{
    public class HostOptions
    {
        public HostOptions()
        {
        }

        public HostOptions(string[] args)
        {
            Host = args.Length >= 1 ? args[0] : Host;
            Port = args.Length >= 2 ? int.Parse(args[1]) : Port;
        }

        public string Host { get; set; } = "0.0.0.0";

        public int Port { get; set; } = 80;
    }

    public class DiagnosticInterceptorOption
    {
        public string ResponseTypeName { get; set; }

        public string[] PropertyNames { get; set; }
    }

    public class GrpcServerOptions
    {
        public IEnumerable<IMethodContext> RegisteredMethods { get; internal set; } = Enumerable.Empty<IMethodContext>();
    }
}