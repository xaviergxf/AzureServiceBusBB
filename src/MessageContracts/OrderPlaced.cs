namespace MessageContracts
{
    public record OrderPlaced(
        int OrderID, 
        Guid CustomerID, 
        DateTimeOffset OrderDate, 
        ICollection<OrderPlacedItem> Items,
        OrderPlacedPaymentInfo PaymentInfo
    );

    public record OrderPlacedItem(
        Guid ProductID, 
        string ProductName, 
        decimal Price, 
        int Quantity
    );

    public record CreditCardPaymentInfo(CreditCardType CreditCardType, string CreditCardNumber, int CreditCardExpMonth, int CreditCardExpYear, int CreditCardCvc);

    public record BTCPaymentInfo(string publicBtcAddress);

    public record OrderPlacedPaymentInfo(string PaymentType, CreditCardPaymentInfo? CreditCardPaymentInfo = null, BTCPaymentInfo? BTCPaymentInfo = null);
}