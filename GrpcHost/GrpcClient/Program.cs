using System;
using System.Threading.Tasks;
using Contracts;
using Grpc.Core;
using Grpc.Health.V1;

namespace GrpcClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:5000", ChannelCredentials.Insecure);
            var healthClient = new Health.HealthClient(channel);
            var healthResponse = await healthClient.CheckAsync(new HealthCheckRequest());
            Console.WriteLine($"Server is: {healthResponse.Status}");

            healthResponse = await healthClient.CheckAsync(new HealthCheckRequest { Service = Contracts.CustomerService.Descriptor.FullName });
            Console.WriteLine($"CustomerService is: {healthResponse.Status}");

            var customerClient = new CustomerService.CustomerServiceClient(channel);
            var customerResponse = await customerClient.GetCustomerByIdAsync(new GetCustomerByIdRequest { Id = 1 });
            Console.WriteLine($"Customer: {customerResponse.Customer.Id} retrieved.");

            healthResponse = await healthClient.CheckAsync(new HealthCheckRequest { Service = Contracts.ProductService.Descriptor.FullName });
            Console.WriteLine($"ProductService is: {healthResponse.Status}");

            var productClient = new ProductService.ProductServiceClient(channel);
            var productResponse = await productClient.GetProductForCustomerAsync(new GetProductsForCustomerRequest { CustomerId = customerResponse.Customer.Id });
            Console.WriteLine("Products retrieved.");

            await channel.ShutdownAsync();

            Console.ReadKey();
        }
    }
}
