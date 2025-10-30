namespace performance_cache.Exceptions
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string message) : base(message) { }
    }

    public class StockMovementException : Exception
    {
        public StockMovementException(string message) : base(message) { }
    }

    public class InvalidStockQuantityException : Exception
    {
        public InvalidStockQuantityException(string message) : base(message) { }
    }

    public class ProductValidationException : Exception
    {
        public ProductValidationException(string message) : base(message) { }
    }

    public class MovementValidationException : Exception
    {
        public MovementValidationException(string message) : base(message) { }
    }
}
