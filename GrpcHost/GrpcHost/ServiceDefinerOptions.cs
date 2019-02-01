using System.Collections.Generic;

namespace GrpcHost
{
    public class ServiceDefinerOptions
    {
        public List<IServiceDefiner> Definers { get; } = new List<IServiceDefiner>(0);
    }
}