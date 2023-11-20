using System;
using GrpcHost;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.Options;
using OpenTracing;
using OpenTracing.Util;

namespace Techsson.Gaming.Infrastructure.Grpc.Instrumentation.Tracing
{
    internal class JaegerTracerFactory : ITracerFactory
    {
        private readonly JaegerOptions _options;

        public JaegerTracerFactory(IOptions<JaegerOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public ITracer Create()
        {
            return CreateTracer();

            ITracer CreateTracer()
            {
                var tracer =
                    new Tracer.Builder(_options.ServiceName)
                        .WithReporter(new RemoteReporter.Builder().WithSender(CreateSender()).Build())
                        .WithSampler(new ConstSampler(sample: true))
                        .Build();

                if (!GlobalTracer.IsRegistered())
                    GlobalTracer.Register(tracer);

                return GlobalTracer.Instance;

                ISender CreateSender()
                {
                    if(_options.IsUdpSender)
                        return new UdpSender(_options.Host, _options.Port, 0);

                    return new HttpSender(_options.Url);
                }
            }
        }
    }
}