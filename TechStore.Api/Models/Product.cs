namespace TechStore.Api.Models;

public sealed record Product(int Id, string Name, string Description, decimal Price, int StockQuantity);
