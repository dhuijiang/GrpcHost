using System.Net.Http;
using System.Threading.Tasks;
using Contracts;
using Grpc.Core;

namespace ProductGrpcService
{
    public class ProductServiceImpl : Contracts.ProductService.ProductServiceBase
    {
        private readonly HttpClient _client;

        public ProductServiceImpl(HttpClient client)
        {
            _client = client;
        }

        public override async Task<GetProductsForCustomerResponse> GetProductForCustomer(GetProductsForCustomerRequest request, ServerCallContext context)
        {
            var response = await _client.GetAsync("https://jsonplaceholder.typicode.com/todos/1").ConfigureAwait(false);

            var products = new GetProductsForCustomerResponse();
            products.Products.Add(new Product
            {
                Id = 1,
                Name = "Foo",
                Price = 22,
                Quantity = 3
            });

            return products;
        }
    }
}
