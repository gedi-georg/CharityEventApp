namespace CharityEventApp.Models.DTOs
{
    public class CheckoutRequest
    {
        public decimal CashPaid { get; set; }
        public List<CartItem> Items { get; set; }
        public string SessionId { get; set; } = "";
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
