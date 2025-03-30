using CharityEventApp.Data;
using CharityEventApp.Interfaces;
using CharityEventApp.Models;
using CharityEventApp.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CharityEventApp.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> UpdateStockAsync(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            product.Quantity = quantity;
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<(bool success, string message, int transactionId, int remainingQuantity)> AddToCartAsync(AddToCartRequest request)
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return (false, "Session ID is required.", 0, 0);

            var sessionId = request.SessionId;
            var productId = request.ProductId;

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return (false, $"Product with ID {productId} not found.", 0, 0);

            if (product.Quantity <= 0)
                return (false, $"Product {product.Name} is out of stock.", 0, 0);

            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.SessionId == sessionId && t.Status == Status.InProgress);

            if (transaction == null)
            {
                transaction = new Transaction
                {
                    SessionId = sessionId,
                    Status = Status.InProgress,
                    TransactionItems = new List<TransactionItem>()
                };
                _context.Transactions.Add(transaction);
            }

            var existingItem = transaction.TransactionItems.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var newTransactionItem = new TransactionItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Price = product.Price
                };
                transaction.TransactionItems.Add(newTransactionItem);
            }

            // Update product stock after the transaction item has been added
            product.Quantity -= 1;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return (true, "Product added to the cart", transaction.Id, product.Quantity);
        }

        public async Task<(bool success, string message, decimal change)> CheckoutAsync(CheckoutRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return (false, "No items in the request.", 0);

            if (request.CashPaid <= 0)
                return (false, "Invalid cash amount.", 0);

            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.SessionId == request.SessionId && t.Status == Status.InProgress);

            if (transaction == null)
                return (false, "Transaction not found or already completed.", 0);

            decimal total = 0;

            foreach (var item in request.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null)
                    return (false, $"Product with ID {item.ProductId} not found.", 0);

                // laoseis on juba AddToCart-is vähenenud, siin lihtsalt kontrollime loogikat
                if (item.Quantity <= 0)
                    return (false, $"Invalid quantity for product {product.Name}.", 0);

                var itemTotal = product.Price * item.Quantity;
                total += itemTotal;

                var existingItem = transaction.TransactionItems.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    // ära tee topelt-lisamist siin
                    // existingItem.Quantity += item.Quantity;
                }
                else
                {
                    transaction.TransactionItems.Add(new TransactionItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = product.Price
                    });
                }
            }

            if (request.CashPaid < total)
                return (false, "Not enough cash paid.", 0);

            var change = request.CashPaid - total;
            transaction.Status = Status.Completed;
            transaction.Total = total;
            transaction.CashPaid = request.CashPaid;
            transaction.Change = change;

            await _context.SaveChangesAsync();

            return (true, "Checkout successful", change);
        }

        public async Task<(bool success, string message)> ResetTransactionAsync(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || transaction.Status != Status.InProgress)
                return (false, "Transaction not found or already completed/cancelled.");

            foreach (var item in transaction.TransactionItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                }
            }

            transaction.Status = Status.Cancelled;
            await _context.SaveChangesAsync();

            return (true, "Transaction reset and stock restored.");
        }

        public async Task<int?> GetTransactionIdAsync(string sessionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.SessionId == sessionId && t.Status == Status.InProgress);

            return transaction?.Id;
        }
    }
}
