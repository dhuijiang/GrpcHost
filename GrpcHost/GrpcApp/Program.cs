using System.Net.Http;
using System.Threading.Tasks;
using Contracts;
using CustomerGrpcService;
using GrpcHost;
using GrpcHost.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductGrpcService;
using Services;

namespace GrpcServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await
                GrpcHostBuilder.BuildHost<Program>(args, (ctx, svcs) =>
                {
                    svcs.AddSingleton<HttpClient>();
                    svcs.AddTransient<ICustomerService, Services.CustomerService>();

                    svcs.AddSingleton<CustomerServiceImpl>();
                    svcs.AddSingleton<ProductServiceImpl>();

                    svcs.AddSingleton<IMethodContext>(x =>
                    new MethodContext<GetCustomerByIdRequest, GetCustomerByIdResponse, CustomerServiceImpl>(
                        ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x),
                        ActivatorUtilities.GetServiceOrCreateInstance<MyInterceptor>(x)));

                    svcs.AddSingleton<IMethodContext, MethodContext<DeleteCustomerByIdRequest, DeleteCustomerByIdResponse, CustomerServiceImpl>>();
                    svcs.AddSingleton<IMethodContext, MethodContext<CustomerSearch, Customer, CustomerServiceImpl>>();
                    svcs.AddSingleton<IMethodContext, MethodContext<GetProductsForCustomerRequest, GetProductsForCustomerResponse, ProductServiceImpl>>();
                })
                .RunAsync().ConfigureAwait(false);
        }
    }
}