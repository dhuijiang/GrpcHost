using System.Threading.Tasks;
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

        public CustomerService(ILogger<CustomerService> logger)
        {
            _logger = logger;
        }

        public Task<CustomerEntity> GetById(int id)
        {
            _logger.LogInformation($"Getting customer by id: {id}");

            return Task.FromResult(new CustomerEntity
            {
                Id = id,
                FirstName = "Gordon",
                LastName = "Ramsey"
            });
        }
    }
}