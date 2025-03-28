using CharityEventApp.Data;
using CharityEventApp.Models;
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

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] TransactionDto transactionDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                decimal total = 0;
                var transactionRecord = new Transaction
                {
                    Date = DateTime.UtcNow,
                    CashPaid = transactionDto.CashPaid,
                    TransactionItems = new List<TransactionItem>()
                };

                foreach (var item in transactionDto.Items)
                {
                    // Lock the product using SELECT FOR UPDATE
                    var product = await _context.Products
                        .FromSqlRaw("SELECT * FROM \"Products\" WHERE \"Id\" = {0} FOR UPDATE", item.ProductId)
                        .FirstOrDefaultAsync();

                    if (product == null || product.Quantity < item.Quantity)
                        return BadRequest("Not enough stock or product not found.");

                    product.Quantity -= item.Quantity;
                    total += product.Price * item.Quantity;

                    transactionRecord.TransactionItems.Add(new TransactionItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        Price = product.Price
                    });
                }

                if (transactionDto.CashPaid < total)
                    return BadRequest("Not enough cash.");

                transactionRecord.Total = total;
                transactionRecord.Change = transactionDto.CashPaid - total;

                _context.Transactions.Add(transactionRecord);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { Total = total, Change = CalculateChange(transactionRecord.Change) });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Checkout failed: {ex.Message}");
            }
        }

        // Calculate change in smallest possible denominations
        private decimal CalculateChange(decimal change)
        {
            // Simple logic for change calculation (for example, using available coin denominations)
            // Implement the change calculation logic
            return Math.Round(change, 2);  // Round to the nearest cent
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
    }
}
