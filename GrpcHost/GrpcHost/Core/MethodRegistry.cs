using System.Collections.Generic;
using System.Linq;

namespace GrpcHost.Core
{
    internal class MethodRegistry
    {
        public static MethodRegistry Empty => new MethodRegistry();

        public IEnumerable<IMethodContext> RegisteredMethods { get; internal set; } = Enumerable.Empty<IMethodContext>();
    }
}