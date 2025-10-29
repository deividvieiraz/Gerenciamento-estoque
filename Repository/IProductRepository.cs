using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IProductRepository
    {
        Task<int> AddProductAsync(Product product);
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetProductsBelowMinimumStockAsync();
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int skuCode);
    }
}
