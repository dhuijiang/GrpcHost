using System.Net.Http;
using System.Threading.Tasks;
using Contracts;
using CustomerGrpcService;
using Grpc.Core;
using GrpcHost;
using GrpcHost.Interceptors;
using GrpcHost.ServiceDefiners;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;

namespace GrpcServer
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            await
                GrpcHostBuilder.BuildHost(args, (ctx, svcs) =>
                {
                    svcs.AddSingleton<HttpClient>();
                    svcs.AddTransient<ICustomerService, Services.CustomerService>();

                    svcs.AddSingleton<IServiceDefiner>(
                        x =>
                            new UnaryServiceDefiner<GetCustomerByIdRequest, GetCustomerByIdResponse>(
                                Contracts.CustomerService.Descriptor.FindMethodByName("GetCustomerById"),
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x).GetCustomerById,
                                ActivatorUtilities.GetServiceOrCreateInstance<ExceptionInterceptor>(x)));

                    svcs.AddSingleton<IServiceDefiner>(
                        x =>
                            new UnaryServiceDefiner<DeleteCustomerByIdRequest, DeleteCustomerByIdResponse>(
                                Contracts.CustomerService.Descriptor.FindMethodByName("DeleteCustomerById"),
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x).DeleteCustomerById));

                    //svcs.AddSingleton<IServiceDefiner>(
                    //    x =>
                    //        new ServerStreamingServiceDefiner<CustomerSearch, IServerStreamWriter<Customer>>(
                    //            Contracts.CustomerService.Descriptor.FindMethodByName("ListCustomers"),
                    //            ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x).ListCustomers));
                })
                .RunAsync();
        }
    }
}