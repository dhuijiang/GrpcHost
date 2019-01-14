using System;
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

        public override async Task<GetCustomerByIdResponse> GetCustomerById(GetCustomerByIdRequest request, ServerCallContext context)
        {
            var customerEntity = await _customerService.GetById(request.Id).ConfigureAwait(false);

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
    }
}