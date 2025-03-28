using System.ComponentModel.DataAnnotations.Schema;

namespace CharityEventApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public List<TransactionItem> TransactionItems { get; set; }
        public decimal Total { get; set; }
        public decimal CashPaid { get; set; }
        public decimal Change { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public Status Status { get; set; }
        public string SessionId { get; set; } = "";
    }

    public enum Status
    {
        InProgress,
        Completed,
        Cancelled
    }

    public class TransactionItem
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public Transaction Transaction { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
