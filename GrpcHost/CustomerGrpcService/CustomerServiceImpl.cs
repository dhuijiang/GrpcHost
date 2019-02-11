using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts;
using Grpc.Core;
using Services;

namespace CustomerGrpcService
{
    public class CustomerServiceImpl : Contracts.CustomerService.CustomerServiceBase
    {
        private readonly ICustomerService _customerService;

        public CustomerServiceImpl(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public override Task<DeleteCustomerByIdResponse> DeleteCustomerById(DeleteCustomerByIdRequest request, ServerCallContext context)
        {
            return Task.FromResult(new DeleteCustomerByIdResponse());
        }

        public override async Task<GetCustomerByIdResponse> GetCustomerById(GetCustomerByIdRequest request, ServerCallContext context)
        {
            var client = new Contracts.CustomerService.CustomerServiceClient(new Channel("localhost:5000", ChannelCredentials.Insecure));
            await client.DeleteCustomerByIdAsync(new DeleteCustomerByIdRequest { Id = request.Id }).ResponseAsync.ConfigureAwait(false);

            var customerEntity = await _customerService.GetById(request.Id);

            return new GetCustomerByIdResponse
            {
                Customer = new Customer
                {
                    Id = customerEntity.Id,
                    FirstName = customerEntity.FirstName,
                    LastName = customerEntity.LastName
                }
            };
        }

        public override async Task ListCustomers(CustomerSearch request, IServerStreamWriter<Customer> responseStream, ServerCallContext context)
        {
            var customers = new List<Customer> { new Customer(), new Customer() };

            foreach(var customer in customers)
            {
                await responseStream.WriteAsync(customer);
            }
        }
    }
}