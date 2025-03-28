namespace CharityEventApp.Models
{
    public class TransactionDto
    {
        public decimal CashPaid { get; set; }
        public List<TransactionItemDto> Items { get; set; } = new();
    }

    public class TransactionItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
