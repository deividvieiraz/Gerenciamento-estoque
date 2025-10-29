namespace Domain
{
    public class StockMovement
    {
        public MovementType MovementType { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public int Year { get; set; }
        public string Batch { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
