using Microsoft.AspNetCore.Mvc;
using TechStore.Api.Data;
using TechStore.Api.Models;

namespace TechStore.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Product>> GetAll() => Ok(store.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = store.GetById(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Create(ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return NameRequired();

        var product = store.Add(input);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Product> Update(int id, ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            return NameRequired();

        var product = store.Update(id, input);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) => store.Delete(id) ? NoContent() : NotFound();

    private ActionResult<Product> NameRequired()
    {
        ModelState.AddModelError(nameof(ProductInput.Name), "Name is required.");
        return base.ValidationProblem(ModelState);
    }
}
