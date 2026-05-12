using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers.Api;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        var products = await query.Select(p => new
        {
            p.Id, p.Name, p.Category, p.ProductionTimePerUnit, p.MinimalStock, p.Description
        }).ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}/materials")]
    public async Task<IActionResult> GetMaterials(int id)
    {
        var exists = await _db.Products.AnyAsync(p => p.Id == id);
        if (!exists) return NotFound();

        var materials = await _db.ProductMaterials
            .Where(pm => pm.ProductId == id)
            .Include(pm => pm.Material)
            .Select(pm => new
            {
                pm.MaterialId,
                pm.Material.Name,
                pm.Material.UnitOfMeasure,
                pm.QuantityNeeded
            }).ToListAsync();

        return Ok(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var product = new Product
        {
            Name = dto.Name,
            ProductionTimePerUnit = dto.ProdTime,
            Category = dto.Category
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
    }
}

public record CreateProductDto(string Name, int ProdTime, string? Category);
