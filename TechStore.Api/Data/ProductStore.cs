using System.Text.Json;
using TechStore.Api.Models;

namespace TechStore.Api.Data;

public sealed class ProductStore : IProductStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly object _gate = new();
    private readonly string _filePath;
    private readonly List<Product> _products;

    public ProductStore(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _filePath = configuration["Products:FilePath"]
            ?? Path.Combine(environment.ContentRootPath, "Data", "products.json");
        _products = File.Exists(_filePath)
            ? JsonSerializer.Deserialize<List<Product>>(File.ReadAllText(_filePath), JsonOptions) ?? []
            : [];
    }

    public IReadOnlyList<Product> GetAll()
    {
        lock (_gate)
            return _products.OrderBy(product => product.Id).ToArray();
    }

    public Product? GetById(int id)
    {
        lock (_gate)
            return _products.FirstOrDefault(product => product.Id == id);
    }

    public Product Add(ProductInput input)
    {
        lock (_gate)
        {
            var id = _products.Count == 0 ? 1 : checked(_products.Max(product => product.Id) + 1);
            var product = new Product(id, input.Name.Trim(), input.Description?.Trim() ?? string.Empty, input.Price, input.StockQuantity);
            _products.Add(product);
            Save();
            return product;
        }
    }

    public Product? Update(int id, ProductInput input)
    {
        lock (_gate)
        {
            var index = _products.FindIndex(product => product.Id == id);
            if (index < 0) return null;

            var product = new Product(id, input.Name.Trim(), input.Description?.Trim() ?? string.Empty, input.Price, input.StockQuantity);
            _products[index] = product;
            Save();
            return product;
        }
    }

    public bool Delete(int id)
    {
        lock (_gate)
        {
            var removed = _products.RemoveAll(product => product.Id == id) > 0;
            if (removed) Save();
            return removed;
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath))!;
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(_filePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(_products, JsonOptions));
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
