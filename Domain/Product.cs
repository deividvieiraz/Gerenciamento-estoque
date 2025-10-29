using System.Reflection;

namespace Domain
{
    public class Product
    {
        public int SKUCode { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
        public DateTime CreationDate { get; set; }

        public string? LotNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
