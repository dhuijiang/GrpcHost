using System.Threading.Tasks;

namespace GrpcHost.Server
{
    public interface IHealthCheckOverrider
    {
        Task<bool> IsHealthy();
    }
}
