using System;
using System.Collections.Concurrent;
using Grpc.Core;
using GrpcHost.Invokers;

namespace GrpcHost.Server
{
    public interface IClientFactory
    {
        T GetOrAd<T>(string name) where T : ClientBase<T>;
    }

    internal class ClientFactory : IClientFactory
    {
        private ConcurrentDictionary<string, ClientBase> _clientCache = new ConcurrentDictionary<string, ClientBase>();
        private readonly ICallContext _callContext;

        public ClientFactory(ICallContext callContext)
        {
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
            var channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            var invoker = new GlobalCallInvoker(channel, _callContext);

            ClientBase client = (ClientBase)Activator.CreateInstance(typeof(T), new object[] { invoker });
            _clientCache[name] = client;

            return client;
        }
    }
}
