using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;

namespace GrpcHost
{
    public delegate Task<HealthCheckResponse> HealthCheckOverride();

    /// <summary>
    /// Represents the class that overrides the behavior of <see cref="Health.HealthBase.Check(HealthCheckRequest, ServerCallContext)".
    /// </summary>
    public class ExtendedHealthServiceImpl : HealthServiceImpl
    {
        private readonly HealthCheckOverride _checkOverride;

        /// <summary>
        /// Initializes new instance of <see cref="ExtendedHealthServiceImpl"/>.
        /// </summary>
        /// <param name="check">If provided it will override the logic of <see cref="Health.HealthBase.Check(HealthCheckRequest, ServerCallContext)" method./></param>
        public ExtendedHealthServiceImpl(HealthCheckOverride checkOverride = null)
        {
            _checkOverride = checkOverride;
        }

        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            return
                _checkOverride == null
                ? base.Check(request, context)
                : _checkOverride();
        }
    }
}
