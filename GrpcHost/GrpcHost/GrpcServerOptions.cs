using System.Collections.Generic;

namespace GrpcHost
{
    public class GrpcServerOptions
    {
        public List<IMethodContext> Definers { get; } = new List<IMethodContext>(0);
    }
}