using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers.Api;

[ApiController]
[Route("api/materials")]
public class MaterialsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public MaterialsApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool low_stock = false)
    {
        var query = _db.Materials.AsQueryable();
        if (low_stock)
            query = query.Where(m => m.Quantity <= m.MinimalStock);

        var materials = await query.Select(m => new
        {
            m.Id, m.Name, m.Quantity, m.UnitOfMeasure, m.MinimalStock, IsLowStock = m.Quantity <= m.MinimalStock
        }).ToListAsync();

        return Ok(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Название материала обязательно");
        if (dto.Quantity < 0) return BadRequest("Количество не может быть отрицательным");
        if (dto.MinStock < 0) return BadRequest("Минимальный запас не может быть отрицательным");

        var material = new Material
        {
            Name = dto.Name.Trim(),
            Quantity = dto.Quantity,
            UnitOfMeasure = string.IsNullOrWhiteSpace(dto.Unit) ? "шт" : dto.Unit,
            MinimalStock = dto.MinStock
        };

        _db.Materials.Add(material);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = material.Id }, material);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
    {
        if (dto.Amount < 0) return BadRequest("Количество не может быть отрицательным");

        var material = await _db.Materials.FindAsync(id);
        if (material is null) return NotFound();

        material.Quantity = dto.Amount;
        await _db.SaveChangesAsync();
        return Ok(new { material.Id, material.Quantity });
    }
}

public record CreateMaterialDto(string Name, decimal Quantity, string Unit, decimal MinStock);
public record UpdateStockDto(decimal Amount);
