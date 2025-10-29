using Domain;

namespace performance_cache.Service
{
    public class ProductService : IProductService
    {

        private readonly List<Product> _products = new List<Product>();

        public void AddProduct(Product product)
        {
            if (!ValidateProduct(product))
            {
                throw new ArgumentException("Invalid Product. Verify mandatory data fields.");
            }

            product.CreationDate = DateTime.Now;
            _products.Add(product);
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _products;
        }

        public IEnumerable<Product> GetProductsBelowMinimumStock()
        {
            return _products.Where(p => p.Quantity < p.MinimumQuantity);
        }

        public bool ValidateProduct(Product product)
        {
            if (product.Category == Category.PERISHABLE)
            {
                if (string.IsNullOrWhiteSpace(product.LotNumber))
                    return false;

                if (product.ExpirationDate == null || product.ExpirationDate <= DateTime.Now)
                    return false;
            }

            return true;
        }

    }
}
