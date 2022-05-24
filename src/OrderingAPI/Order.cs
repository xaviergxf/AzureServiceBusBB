using MessageContracts;
using System.ComponentModel.DataAnnotations;

namespace OrderingAPI
{
    public record Order : IValidatableObject
    {
        public int OrderID { get; set; }
        public Guid CustomerID { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        [Required]
        public ICollection<OrderItem> Items { get; } = new List<OrderItem>();
        public CreditCardType CreditCardType { get; set; }

        [Required]
        public string PaymentType { get; init; } = default!;

        public string? CreditCardNumber { get; init; }

        public int CreditCardExpMonth { get; set; }
        public int CreditCardExpYear { get; set; }
        public int CreditCardCvc { get; set; }

        public string? BTCPublicAddress { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentType != PaymentTypes.Bitcoin && PaymentType != PaymentTypes.CreditCard)
            {
                yield return new ValidationResult($"Invalid payment type: {PaymentType}");
            }
            else
            {
                if (PaymentType == PaymentTypes.CreditCard && string.IsNullOrEmpty(CreditCardNumber))
                    yield return new ValidationResult($"Invalid credit card number: {CreditCardNumber}");

                if (PaymentType == PaymentTypes.Bitcoin && string.IsNullOrEmpty(BTCPublicAddress))
                    yield return new ValidationResult($"Invalid BTC address: {BTCPublicAddress}");
            }
        }
    }

    public record OrderItem(Guid ProductID, [Required] string ProductName, decimal Price, int Quantity);
}