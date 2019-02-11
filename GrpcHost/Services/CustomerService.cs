using System.Threading.Tasks;
using Contracts;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Services.Entities;

namespace Services
{
    public interface ICustomerService
    {
        Task<CustomerEntity> GetById(int id);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ILogger _logger;

        public CustomerService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<CustomerEntity> GetById(int id)
        {
            _logger.LogInformation($"Getting customer by id: {id}");

            var client = new ProductService.ProductServiceClient(new Channel("localhost:5000", ChannelCredentials.Insecure));
            
            var response = await client.GetProductForCustomerAsync(new GetProductsForCustomerRequest { CustomerId = id }).ResponseAsync.ConfigureAwait(false);

            return new CustomerEntity
            {
                Id = id,
                FirstName = "Gordon",
                LastName = "Ramsey"
            };
        }
    }
}