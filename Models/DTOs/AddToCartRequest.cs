namespace CharityEventApp.Models.DTOs
{
    public class AddToCartRequest
    {
        public string SessionId { get; set; } = "";
        public int ProductId { get; set; }
    }
}
