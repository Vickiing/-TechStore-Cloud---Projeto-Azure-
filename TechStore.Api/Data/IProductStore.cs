using TechStore.Api.Models;

namespace TechStore.Api.Data;

public interface IProductStore
{
    IReadOnlyList<Product> GetAll();
    Product? GetById(int id);
    Product Add(ProductInput input);
    Product? Update(int id, ProductInput input);
    bool Delete(int id);
}
