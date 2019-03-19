using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, ITracer> _cache = new ConcurrentDictionary<string, ITracer>();

        private readonly JaegerOptions _options;

        public JaegerTracerFactory(IOptions<JaegerOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public ITracer Create(string name)
        {
            if(_cache.ContainsKey(name))
                return _cache[name];

            _cache[name] = CreateTracer();

            return _cache[name];

            ITracer CreateTracer()
            {
                var tracer =
                    new Tracer.Builder(_options.ServiceName)
                        .WithReporter(new RemoteReporter.Builder().WithSender(CreateSender()).Build())
                        .WithSampler(new ConstSampler(sample: true))
                        .Build();

                GlobalTracer.Register(tracer);

                return tracer;
                
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