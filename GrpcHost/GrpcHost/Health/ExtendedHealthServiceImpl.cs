using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;

namespace GrpcHost.Health
{
    /// <summary>
    /// Represents the class that overrides the behavior of <see cref="Health.HealthBase.Check(HealthCheckRequest, ServerCallContext)".
    /// </summary>
    internal sealed class ExtendedHealthServiceImpl : HealthServiceImpl
    {
        private readonly IHealthCheckOverrider _overrider;

        /// <summary>
        /// Initializes new instance of <see cref="ExtendedHealthServiceImpl"/>.
        /// </summary>
        /// <param name="check">If provided it will override the logic of <see cref="Health.HealthBase.Check(HealthCheckRequest, ServerCallContext)" method./></param>
        public ExtendedHealthServiceImpl(IHealthCheckOverrider overrider = null)
        {
            _overrider = overrider;
        }

        public override async Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            return
                _overrider == null
                ? await base.Check(request, context)
                : new HealthCheckResponse { Status = await GetStatusAsync() };

            async Task<HealthCheckResponse.Types.ServingStatus> GetStatusAsync()
            {
                return
                    await _overrider.IsHealthy()
                        ? HealthCheckResponse.Types.ServingStatus.Serving
                        : HealthCheckResponse.Types.ServingStatus.NotServing;
            }
        }
    }
}
