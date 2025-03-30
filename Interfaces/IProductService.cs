using CharityEventApp.Models;
using CharityEventApp.Models.DTOs;

namespace CharityEventApp.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> UpdateStockAsync(int id, int quantity);
        Task<(bool success, string message, int transactionId, int remainingQuantity)> AddToCartAsync(AddToCartRequest request);
        Task<(bool success, string message, decimal change)> CheckoutAsync(CheckoutRequest request);
        Task<(bool success, string message)> ResetTransactionAsync(int transactionId);
        Task<int?> GetTransactionIdAsync(string sessionId);
    }
}