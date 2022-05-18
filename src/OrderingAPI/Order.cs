using MessageContracts;
using System.ComponentModel.DataAnnotations;

namespace OrderingAPI
{
    public record Order
    {
        public int OrderID { get; set; }
        public Guid CustomerID { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        [Required]
        public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
        public CreditCardType CreditCardType { get; set; }

        [Required]
        public string CreditCardCountry { get; init; } = default!;

        [Required]
        public string CreditCardNumber { get; init; } = default!;

        public int CreditCardExpMonth { get; set; }
        public int CreditCardExpYear { get; set; }
        public int CreditCardCvc { get; set; }
    }

    public record OrderItem(Guid ProductID, [Required] string ProductName, decimal Price, int Quantity);
}