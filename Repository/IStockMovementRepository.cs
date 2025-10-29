using Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public interface IStockMovementRepository
    {
        Task<int> AddStockMovementAsync(StockMovement movement);
        Task<IEnumerable<StockMovement>> GetAllStockMovementsAsync();
        Task<IEnumerable<StockMovement>> GetStockMovementsByProductAsync(int productSkuCode);
        Task UpdateStockMovementAsync(StockMovement movement);
        Task DeleteStockMovementAsync(int movementId);
    }
}
