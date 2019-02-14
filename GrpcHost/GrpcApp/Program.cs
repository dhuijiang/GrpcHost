using System.Net.Http;
using System.Threading.Tasks;
using Contracts;
using CustomerGrpcService;
using GrpcHost;
using GrpcHost.Methods;
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

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<GetCustomerByIdRequest, GetCustomerByIdResponse, CustomerServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x)));

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<DeleteCustomerByIdRequest, DeleteCustomerByIdResponse, CustomerServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x)));

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<CustomerSearch, Customer, CustomerServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x)));

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<GetProductsForCustomerRequest, GetProductsForCustomerResponse, ProductServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<ProductServiceImpl>(x)));
                })
                .RunAsync().ConfigureAwait(false);
        }
    }
}