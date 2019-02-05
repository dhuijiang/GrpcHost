using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Contracts;
using CustomerGrpcService;
using Grpc.Core;
using GrpcHost;
using GrpcHost.Interceptors;
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
                GrpcHostBuilder.BuildHost<Program>(args, (ctx, svcs) =>
                {
                    svcs.AddSingleton<HttpClient>();
                    svcs.AddTransient<ICustomerService, Services.CustomerService>();

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<GetCustomerByIdRequest, GetCustomerByIdResponse, CustomerServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x),
                                ActivatorUtilities.GetServiceOrCreateInstance<ExceptionInterceptor>(x)));

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<DeleteCustomerByIdRequest, DeleteCustomerByIdResponse, CustomerServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x)));

                    svcs.AddSingleton<IMethodContext>(
                        x =>
                            new MethodContext<CustomerSearch, Customer, CustomerServiceImpl>(
                                ActivatorUtilities.GetServiceOrCreateInstance<CustomerServiceImpl>(x)));
                })
                .RunAsync();
        }
    }
}