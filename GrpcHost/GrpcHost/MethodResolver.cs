using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace GrpcHost
{
    // Hacky way to avoid a need to initialize GrpcSerer directly in DI, open for ideas -mime01
    public interface IMethodResolver
    {
        IEnumerable<IMethodContext> RegisteredMethods { get; }
    }

    public class MethodResolver : IMethodResolver
    {
        public IEnumerable<IMethodContext> RegisteredMethods { get; }

        public MethodResolver(IServiceProvider provider)
        {
            RegisteredMethods = provider.GetServices<IMethodContext>() ?? Enumerable.Empty<IMethodContext>();
        }
    }
}
