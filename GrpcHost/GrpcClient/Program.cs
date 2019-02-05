using System;
using System.Threading;
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
            Channel channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
            var healthClient = new Health.HealthClient(channel);
            var healthResponse = await healthClient.CheckAsync(new HealthCheckRequest());
            Console.WriteLine($"Server is: {healthResponse.Status}");

            healthResponse = await healthClient.CheckAsync(new HealthCheckRequest { Service = Contracts.CustomerService.Descriptor.FullName });
            Console.WriteLine($"CustomerService is: {healthResponse.Status}");

            var customerClient = new CustomerService.CustomerServiceClient(channel);
            var customerResponse = await customerClient.GetCustomerByIdAsync(new GetCustomerByIdRequest { Id = 1 });
            Console.WriteLine($"Customer: {customerResponse.Customer.Id} retrieved.");

            var customerResponse2 = customerClient.DeleteCustomerById(new DeleteCustomerByIdRequest { Id = 1 });

            var customerResponse3 = customerClient.ListCustomers(new CustomerSearch { FirstName = "test" });
            while(await customerResponse3.ResponseStream.MoveNext(CancellationToken.None))
            {
                var response = customerResponse3.ResponseStream.Current;
            }

            
            await channel.ShutdownAsync();

            Console.ReadKey();
        }
    }
}
