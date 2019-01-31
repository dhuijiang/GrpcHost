using System.Net.Http;
using System.Threading.Tasks;
using Contracts;
using CustomerGrpcService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.GrpcHost;
using Microsoft.Extensions.Hosting.GrpcHost.Interceptors;
using Microsoft.Extensions.Logging;
using ProductGrpcService;
using Services;

namespace GrpcServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var descriptor = Contracts.CustomerReflection.Descriptor;


            await
                GrpcHostBuilder.BuildHost(args, (ctx, svcs) =>
                {
                    svcs.AddSingleton<HttpClient>();
                    svcs.AddTransient<ICustomerService, Services.CustomerService>();
                    svcs.AddSingleton(
                        x =>
                        new GrpcHost.GrpcServer()
                            .InjectImplementation<GetCustomerByIdRequest, GetCustomerByIdResponse>(
                                Contracts.CustomerService.Descriptor.FindMethodByName("GetCustomerById"),
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x).GetCustomerById,
                                ActivatorUtilities.GetServiceOrCreateInstance<ExceptionInterceptor>(x))
                            .InjectImplementation<GetProductsForCustomerRequest, GetProductsForCustomerResponse>(
                                Contracts.ProductService.Descriptor.FindMethodByName("GetProductForCustomer"),
                                ActivatorUtilities.GetServiceOrCreateInstance<ProductServiceImpl>(x).GetProductForCustomer,
                                null));
                })
                .RunAsync();
        }
    }
}