using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace performance_cache.Service
{
    public interface IReportService
    {
        Task<double> GetTotalStockValueAsync();
        Task<IEnumerable<Product>> GetProductsExpiringSoonAsync();
        Task<IEnumerable<Product>> GetProductsBelowMinimumStockAsync(); 
    }
}
