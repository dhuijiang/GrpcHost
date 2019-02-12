using System.Threading.Tasks;

namespace GrpcHost.Health
{
    public interface IHealthCheckOverrider
    {
        Task<bool> IsHealthy();
    }
}
