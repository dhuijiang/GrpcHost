using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.HealthCheck;

namespace GrpcHost
{
    /// <summary>
    /// Represents the class that overrides the behavior of <see cref="Health.HealthBase.Check(HealthCheckRequest, ServerCallContext)".
    /// </summary>
    public class ExtendedHealthServiceImpl : HealthServiceImpl
    {
        private readonly Func<HealthCheckRequest, ServerCallContext, Task<HealthCheckResponse>> _check;

        /// <summary>
        /// Initializes new instance of <see cref="ExtendedHealthServiceImpl"/>.
        /// </summary>
        /// <param name="check">If provided it will override the logic of <see cref="Health.HealthBase.Check(HealthCheckRequest, ServerCallContext)" method./></param>
        public ExtendedHealthServiceImpl(Func<HealthCheckRequest, ServerCallContext, Task<HealthCheckResponse>> check)
        {
            _check = check;
        }

        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            return _check == null ? base.Check(request, context) : _check(request, context);
        }
    }
}