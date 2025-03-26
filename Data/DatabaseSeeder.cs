using System.Text.Json;
using CharityEventApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CharityEventApp.Data
{
    public class DatabaseSeeder
    {
        public static void SeedProducts(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure DB is created
            context.Database.Migrate();

            // Only seed if DB is empty
            if (context.Products.Any()) return;

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "products.json");

            Console.WriteLine("🟡 Seeder running...");
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine("❌ JSON file not found at: " + jsonPath);
                return;
            }
            Console.WriteLine("✅ File found. Reading...");

            var jsonData = File.ReadAllText(jsonPath);
            var products = JsonSerializer.Deserialize<List<Product>>(jsonData);

            if (products != null)
            {
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
