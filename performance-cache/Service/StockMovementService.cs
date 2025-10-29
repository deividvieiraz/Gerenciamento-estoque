using Domain;

namespace performance_cache.Service
{
    public class StockMovementService : IStockMovementService
    {

        private readonly List<StockMovement> _movements = new List<StockMovement>();
        private readonly IProductService _productService;

        public StockMovementService(IProductService productService)
        {
            _productService = productService;
        }

        public void RegisterMovement(Product product, StockMovement movement)
        {
            if (movement.Quantity <= 0)
                throw new ArgumentException("Quantity must be positive.");

            if (product.Category == Category.PERISHABLE)
            {
                if (movement.ExpirationDate <= DateTime.Now)
                    throw new ArgumentException("Perishable products must have expiration date.");
                if (string.IsNullOrWhiteSpace(movement.Batch))
                    throw new ArgumentException("Perishable products must have batch.");
            }

            if (movement.MovementType == MovementType.INBOUND)
            {
                product.Quantity += movement.Quantity;
            } 
            else 
            {
                if (product.Quantity < movement.Quantity)
                    throw new InvalidOperationException("Not enough stock quantity.");

                product.Quantity -= movement.Quantity;
            }

            movement.Date = DateTime.Now;

            _movements.Add(movement);
        }

    }
}
