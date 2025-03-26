namespace CharityEventApp.Models
{
    public class CheckoutRequest
    {
        public List<CartItem> Items { get; set; }
        public decimal CashPaid { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
