using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CustomerGrpcService;
using Grpc.Core;
using GrpcHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.GrpcHost;
using Microsoft.Extensions.Logging;
using ProductGrpcService;
using Services;

namespace GrpcServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var server = new Server();
            server.Ports.Add("localhost", 5000, ServerCredentials.Insecure);

            await
                GrpcHostBuilder.BuildHost(args, (ctx, svcs) =>
                {
                    svcs.AddSingleton<HttpClient>();
                    svcs.AddTransient<ICustomerService, CustomerService>();
                    svcs.AddSingleton(server);
                    svcs.AddHostedService<GrpcHostedService>();
                    svcs.AddTransient<CustomerServiceImpl>();
                    svcs.AddTransient<ProductServiceImpl>();
                    svcs.AddTransient(x =>
                        new Dictionary<string, ServerServiceDefinition>
                        {
                            ["CustomerService"] = Contracts.CustomerService.BindService(x.GetService<CustomerServiceImpl>()),
                            ["ProductService"] = Contracts.ProductService.BindService(x.GetService<ProductServiceImpl>())
                        });
                })
                .RunAsync();
        }
    }
}