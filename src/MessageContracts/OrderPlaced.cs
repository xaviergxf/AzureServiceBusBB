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

    public record OrderPlacedPaymentInfo(CreditCardType CreditCardType, string CreditCardNumber, int CreditCardExpMonth, int CreditCardExpYear, int CreditCardCvc);
}