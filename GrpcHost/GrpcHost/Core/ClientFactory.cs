using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using Grpc.Core;
using GrpcHost.Core.Invokers;
using GrpcHost.Instrumentation;
using Microsoft.Extensions.Options;

namespace GrpcHost.Core
{
    public interface IClientFactory
    {
        T GetOrAd<T>(string name) where T : ClientBase<T>;
    }

    internal class ClientFactory : IClientFactory
    {
        private readonly ConcurrentDictionary<string, ClientBase> _clientCache = new ConcurrentDictionary<string, ClientBase>();

        private readonly ICorrelationContext _callContext;
        private readonly Collection<ChannelOptions> _channelOptions;

        public ClientFactory(IOptions<Collection<ChannelOptions>> channelOptions, ICorrelationContext callContext)
        {
            _channelOptions = channelOptions.Value ?? new Collection<ChannelOptions>();
            _callContext = callContext;
        }

        public T GetOrAd<T>(string name) where T : ClientBase<T>
        {
            if (_clientCache.ContainsKey(name))
                return (T)_clientCache[name];

            return (T)CreateClient<T>(name);
        }

        private ClientBase CreateClient<T>(string name)
        {
            var options = _channelOptions.FirstOrDefault(x => x.ServiceName.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (options == null)
                throw new ArgumentOutOfRangeException($"Channel: {name} not registered.");

            var channel = new Channel($"{options.Host}:{options.Port}", ChannelCredentials.Insecure);
            var invoker = new GlobalCallInvoker(channel, _callContext);

            ClientBase client = (ClientBase)Activator.CreateInstance(typeof(T), new object[] { invoker });
            _clientCache[name] = client;

            return client;
        }
    }
}