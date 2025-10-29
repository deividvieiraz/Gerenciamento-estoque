using Domain;

namespace performance_cache.Service
{
    public interface IProductService
    {
        void AddProduct(Product product);
        IEnumerable<Product> GetAllProducts();
        IEnumerable<Product> GetProductsBelowMinimumStock();
        bool ValidateProduct(Product product);
    }
}
