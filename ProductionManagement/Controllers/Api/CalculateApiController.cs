using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Services;

namespace ProductionManagement.Controllers.Api;

[ApiController]
[Route("api/calculate")]
public class CalculateApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IProductionCalculationService _calc;

    public CalculateApiController(AppDbContext db, IProductionCalculationService calc)
    {
        _db = db;
        _calc = calc;
    }

    [HttpPost("production")]
    public async Task<IActionResult> CalculateProduction([FromBody] CalculateDto dto)
    {
        if (dto.Quantity <= 0) return BadRequest("Количество должно быть больше 0");

        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product is null) return NotFound("Продукт не найден");

        float efficiency = 1.0f;
        string? lineName = null;
        if (dto.LineId.HasValue)
        {
            var line = await _db.ProductionLines.FindAsync(dto.LineId.Value);
            if (line is not null)
            {
                efficiency = line.EfficiencyFactor;
                lineName = line.Name;
            }
        }

        var minutes = _calc.CalculateMinutes(dto.Quantity, product.ProductionTimePerUnit, efficiency);
        var endDate = _calc.CalculateEndDate(DateTime.Now, minutes);

        return Ok(new
        {
            ProductId = dto.ProductId,
            ProductName = product.Name,
            dto.Quantity,
            LineId = dto.LineId,
            LineName = lineName,
            EfficiencyFactor = efficiency,
            TotalMinutes = minutes,
            TotalHours = Math.Round(minutes / 60.0, 2),
            EstimatedEndDate = endDate
        });
    }
}

public record CalculateDto(int ProductId, int Quantity, int? LineId = null);
