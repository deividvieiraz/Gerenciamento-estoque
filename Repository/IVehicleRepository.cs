using Domain;

namespace Repository
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<StockMovement>> GetAllVehiclesAsync();
        Task<int> AddVehicleAsync(StockMovement vehicle);
        Task UpdateVehicleAsync(StockMovement vehicle);
        Task DeleteVehicleAsync(int id);
    }
}
