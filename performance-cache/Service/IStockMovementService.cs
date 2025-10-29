using Domain;

namespace performance_cache.Service
{
    public interface IStockMovementService
    {
        void RegisterMovement(Product product, StockMovement movement);
    }
}
