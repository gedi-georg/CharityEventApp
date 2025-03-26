using CharityEventApp.Data;
using CharityEventApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CharityEventApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Products.ToListAsync());
        }

        // ostu vormistamine
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            decimal total = 0;

            // Läbime kõik ostukorvi tooted ja arvutame kogusumma
            foreach (var item in request.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                // Kui toodet ei leita või laoseis on väiksem kui soetatud kogus, siis tagastame vea
                if (product == null || product.Quantity < item.Quantity)
                    return BadRequest("Not enough stock or product not found.");

                // Arvutame toote kogumaksumuse (hind * kogus) ja liidame kogusummale
                total += product.Price * item.Quantity;

                // Vähendame laoseisu müüdud koguse võrra
                product.Quantity -= item.Quantity;
            }

            // Kontrollime, et klient maksis piisavalt, kui mitte, siis tagastame vea
            if (request.CashPaid < total)
                return BadRequest("Not enough cash.");

            await _context.SaveChangesAsync();

            // Arvutame tagasiantava vahetusraha
            var change = CalculateChange(request.CashPaid - total);
            
            // Tagastame vastuse, kus on kokku makstud summa ja vahetusraha
            return Ok(new { Total = total, Change = change });
        }

        private Dictionary<decimal, int> CalculateChange(decimal change)
        {
            // Vääringud, mida kasutame vahetusraha arvutamiseks
            var denominations = new List<decimal> { 50, 20, 10, 5, 2, 1, 0.5m, 0.2m, 0.1m, 0.05m, 0.02m, 0.01m };
            var result = new Dictionary<decimal, int>();

            // Läbime kõik vääringud ja arvutame, mitu münti iga vääringuga on vaja
            foreach (var denom in denominations)
            {
                int count = (int)(change / denom);
                if (count > 0)
                {
                    result[denom] = count;
                    // Eemaldame vastava summa vahetusrahast
                    change -= count * denom;
                }
            }

            // Tagastame sõnastiku, kus on igale vääringule vastav müntide arv
            return result;
        }

        // Laoseisu uuendamine
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
