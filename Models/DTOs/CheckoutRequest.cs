namespace CharityEventApp.Models.DTOs
{
    public class CheckoutRequest
    {
        public decimal CashPaid { get; set; }
        public List<CheckoutItem> Items { get; set; }
        public string SessionId { get; set; } = "";
    }

    public class CheckoutItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
