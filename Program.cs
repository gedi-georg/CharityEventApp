using CharityEventApp.Data;
using CharityEventApp.Interfaces;
using CharityEventApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register DbContext with retry logic
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        )));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy to allow any origin, method, and header
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IProductService, ProductService>();

// Build the application
var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        Console.WriteLine("?? Running migrations...");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();  // Applies any pending migrations automatically
        Console.WriteLine("Database migrated.");

        Console.WriteLine("?? Seeder running...");
        DatabaseSeeder.SeedProducts(app);  // Seed the database with products
        Console.WriteLine("Database seeded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("? Migration failed: " + ex.Message);
    }
}


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use CORS policy after the app is built
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();