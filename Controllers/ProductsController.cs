using CharityEventApp.Data;
using CharityEventApp.Interfaces;
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
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        // PUT: api/products/update-stock/{id}
        [HttpPut("update-stock/{id}")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int quantity)
        {
            var product = await _productService.UpdateStockAsync(id, quantity);
            if (product == null) return NotFound();

            return Ok(product);
        }

        // POST: api/products/add-to-cart
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var (success, message, transactionId, remainingQuantity) = await _productService.AddToCartAsync(request);
            if (!success) return BadRequest(new { message });

            return Ok(new { message, transactionId, remainingQuantity });
        }

        // POST: api/products/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var (success, message, change) = await _productService.CheckoutAsync(request);
            if (!success) return BadRequest(new { message });

            return Ok(new { message, change });
        }

        // POST: api/products/reset/{transactionId}
        [HttpPost("reset/{transactionId}")]
        public async Task<IActionResult> ResetTransaction(int transactionId)
        {
            var (success, message) = await _productService.ResetTransactionAsync(transactionId);
            if (!success) return NotFound(new { message });

            return Ok(new { message });
        }

        // GET: api/products/transaction-id/{sessionId}
        [HttpGet("transaction-id/{sessionId}")]
        public async Task<IActionResult> GetTransactionId(string sessionId)
        {
            var transactionId = await _productService.GetTransactionIdAsync(sessionId);
            if (transactionId == null) return NotFound(new { message = "No active transaction found." });

            return Ok(new { transactionId });
        }
    }
}
