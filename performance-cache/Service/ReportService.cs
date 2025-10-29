using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Repository;

namespace performance_cache.Service
{
    public class ReportService : IReportService
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public ReportService(IProductRepository productRepository, IStockMovementRepository stockMovementRepository)
        {
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
        }
        public async Task<double> GetTotalStockValueAsync()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return products.Sum(p => p.Quantity * p.UnitPrice);
        }

        public async Task<IEnumerable<Product>> GetProductsExpiringSoonAsync()
        {
            var products = await _productRepository.GetAllProductsAsync();

            var today = DateTime.Now;
            var limit = today.AddDays(7);

            return products
                .Where(p => p.Category == Category.PERISHABLE
                            && p.ExpirationDate.HasValue
                            && p.ExpirationDate.Value <= limit
                            && p.ExpirationDate.Value > today)
                .ToList();
        }

        public async Task<IEnumerable<Product>> GetProductsBelowMinimumStockAsync()
        {
            return await _productRepository.GetProductsBelowMinimumStockAsync();
        }

        public async Task<IEnumerable<StockMovement>> GetStockMovementsAsync()
        {
            return await _stockMovementRepository.GetAllStockMovementsAsync();
        }
    }
}
