using CharityEventApp.Data;
using CharityEventApp.Models;
using CharityEventApp.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CharityEventApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // PUT: api/products/update-stock/{id}
        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Quantity = quantity;
            await _context.SaveChangesAsync();

            return Ok(product);
        }


        // POST: api/products/add-to-cart
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (string.IsNullOrEmpty(request.SessionId))
                return BadRequest("Session ID is required.");

            var sessionId = request.SessionId;
            var productId = request.ProductId;

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
                return NotFound($"Product with ID {productId} not found.");

            if (product.Quantity <= 0)
                return BadRequest($"Product {product.Name} is out of stock.");

            // Kontrollime, kas on juba olemas jooksva sessiooni tehing (ostukorv)
            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.SessionId == sessionId && t.Status == Status.InProgress);

            // Kui ei ole aktiivset tehingut, loome uue
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

            // Kontrollime, kas toode on juba ostukorvis
            var existingItem = transaction.TransactionItems.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                // Kui toode on juba ostukorvis, suurendame ainult kogust
                existingItem.Quantity += 1;
            }
            else
            {
                // Kui toode ei ole veel ostukorvis, lisame uue kirje
                var newTransactionItem = new TransactionItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Price = product.Price
                };
                transaction.TransactionItems.Add(newTransactionItem);
            }

            // Vähendame toote laoseisu
            product.Quantity -= 1;

            // Save changes in the transaction and product
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Product added to the cart",
                remaining = product.Quantity,
                transactionId = transaction.Id
            });
        }


        // POST: api/products/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return BadRequest("No items in the request.");

            if (request.CashPaid <= 0)
                return BadRequest("Invalid cash amount.");

            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.SessionId == request.SessionId && t.Status == Status.InProgress);

            if (transaction == null)
                return BadRequest("Transaction not found or already completed.");

            decimal total = 0;

            foreach (var item in request.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product == null)
                    return BadRequest($"Product with ID {item.ProductId} not found.");

                if (product.Quantity < item.Quantity)
                    return BadRequest($"Not enough quantity for product {product.Name}.");

                product.Quantity -= item.Quantity;
                var itemTotal = product.Price * item.Quantity;
                total += itemTotal;

                var existingItem = transaction.TransactionItems.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
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
                return BadRequest("Not enough cash paid.");

            var change = request.CashPaid - total;
            transaction.Status = Status.Completed;
            transaction.Total = total;
            transaction.CashPaid = request.CashPaid;
            transaction.Change = change;

            await _context.SaveChangesAsync();

            return Ok(new { change });
        }

        [HttpPost("reset/{transactionId}")]
        public async Task<IActionResult> ResetTransaction(int transactionId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.TransactionItems)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || transaction.Status != Status.InProgress)
                return NotFound("Transaction not found or already completed/cancelled.");

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

            return Ok(new { message = "Transaction reset and stock restored." });
        }


        [HttpGet("transaction-id/{sessionId}")]
        public async Task<IActionResult> GetTransactionId(string sessionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.SessionId == sessionId && t.Status == Status.InProgress);

            if (transaction == null)
                return NotFound("No active transaction found.");

            return Ok(new { transactionId = transaction.Id });
        }


    }
}
