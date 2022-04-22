namespace MessageContracts
{
    public record OrderPaid(int OrderID, DateTimeOffset PaymentDate);

}