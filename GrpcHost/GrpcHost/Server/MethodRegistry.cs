using System.Collections.Generic;
using System.Linq;
using GrpcHost.Methods;

namespace GrpcHost.Server
{
    internal class MethodRegistry
    {
        public IEnumerable<IMethodContext> RegisteredMethods { get; internal set; } = Enumerable.Empty<IMethodContext>();
    }
}
