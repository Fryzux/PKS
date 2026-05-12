using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers.Api;

[ApiController]
[Route("api/lines")]
public class LinesApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public LinesApiController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool available = false)
    {
        var query = _db.ProductionLines.AsQueryable();
        if (available)
            query = query.Where(l => l.Status == LineStatus.Active && l.CurrentWorkOrderId == null);

        var lines = await query.Select(l => new
        {
            l.Id, l.Name, l.Status, l.EfficiencyFactor, l.CurrentWorkOrderId
        }).ToListAsync();

        return Ok(lines);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateLineStatusDto dto)
    {
        if (dto.Status != LineStatus.Active && dto.Status != LineStatus.Stopped)
            return BadRequest("Статус должен быть 'Active' или 'Stopped'");

        var line = await _db.ProductionLines.FindAsync(id);
        if (line is null) return NotFound();

        // Нельзя остановить линию с активным заказом
        if (dto.Status == LineStatus.Stopped && line.Status == LineStatus.Active)
        {
            var hasActive = await _db.WorkOrders
                .AnyAsync(o => o.ProductionLineId == id && o.Status == OrderStatus.InProgress);
            if (hasActive)
                return BadRequest($"Нельзя остановить линию: есть активный заказ в работе. Сначала завершите или отмените заказ.");
        }

        line.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(new { line.Id, line.Status });
    }

    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var exists = await _db.ProductionLines.AnyAsync(l => l.Id == id);
        if (!exists) return NotFound();

        var orders = await _db.WorkOrders
            .Where(o => o.ProductionLineId == id && o.Status != OrderStatus.Cancelled)
            .Include(o => o.Product)
            .OrderBy(o => o.StartDate)
            .Select(o => new
            {
                o.Id, ProductName = o.Product.Name, o.Quantity,
                o.StartDate, o.EstimatedEndDate, o.Status, o.Progress
            }).ToListAsync();

        return Ok(orders);
    }

    [HttpPut("{id}/efficiency")]
    public async Task<IActionResult> UpdateEfficiency(int id, [FromBody] UpdateEfficiencyDto dto)
    {
        if (dto.Factor < 0.5f || dto.Factor > 2.0f)
            return BadRequest("Коэффициент должен быть от 0.5 до 2.0");

        var line = await _db.ProductionLines.FindAsync(id);
        if (line is null) return NotFound();

        line.EfficiencyFactor = dto.Factor;
        await _db.SaveChangesAsync();
        return Ok(new { line.Id, line.EfficiencyFactor });
    }
}

public record UpdateLineStatusDto(string Status);
public record UpdateEfficiencyDto(float Factor);
