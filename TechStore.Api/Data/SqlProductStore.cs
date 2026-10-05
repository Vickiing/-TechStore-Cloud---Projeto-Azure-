using System.Data;
using Microsoft.Data.SqlClient;
using TechStore.Api.Models;

namespace TechStore.Api.Data;

public sealed class SqlProductStore(string connectionString) : IProductStore
{
    public IReadOnlyList<Product> GetAll()
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(
            "SELECT Id, Name, Description, Price, StockQuantity FROM dbo.Products ORDER BY Id;", connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var products = new List<Product>();
        while (reader.Read())
            products.Add(ReadProduct(reader));
        return products;
    }

    public Product? GetById(int id)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(
            "SELECT Id, Name, Description, Price, StockQuantity FROM dbo.Products WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadProduct(reader) : null;
    }

    public Product Add(ProductInput input)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(
            "INSERT INTO dbo.Products (Name, Description, Price, StockQuantity) " +
            "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Description, INSERTED.Price, INSERTED.StockQuantity " +
            "VALUES (@Name, @Description, @Price, @StockQuantity);", connection);
        AddProductParameters(command, input);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read())
            throw new InvalidOperationException("The product was not returned after insertion.");
        return ReadProduct(reader);
    }

    public Product? Update(int id, ProductInput input)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(
            "UPDATE dbo.Products SET Name = @Name, Description = @Description, " +
            "Price = @Price, StockQuantity = @StockQuantity " +
            "OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Description, INSERTED.Price, INSERTED.StockQuantity " +
            "WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        AddProductParameters(command, input);
        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadProduct(reader) : null;
    }

    public bool Delete(int id)
    {
        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand("DELETE FROM dbo.Products WHERE Id = @Id;", connection);
        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        connection.Open();
        return command.ExecuteNonQuery() > 0;
    }

    private static void AddProductParameters(SqlCommand command, ProductInput input)
    {
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 120).Value = input.Name.Trim();
        command.Parameters.Add("@Description", SqlDbType.NVarChar, 1000).Value = input.Description?.Trim() ?? string.Empty;
        var price = command.Parameters.Add("@Price", SqlDbType.Decimal);
        price.Precision = 18;
        price.Scale = 2;
        price.Value = input.Price;
        command.Parameters.Add("@StockQuantity", SqlDbType.Int).Value = input.StockQuantity;
    }

    private static Product ReadProduct(SqlDataReader reader) => new(
        reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
        reader.GetDecimal(3), reader.GetInt32(4));
}
