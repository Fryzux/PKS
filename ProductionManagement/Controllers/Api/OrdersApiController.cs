using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;
using ProductionManagement.Services;

namespace ProductionManagement.Controllers.Api;

[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWorkOrderService _orderService;
    private readonly IMaterialValidationService _materialValidation;
    private readonly IProductionCalculationService _calc;

    public OrdersApiController(AppDbContext db, IWorkOrderService orderService,
        IMaterialValidationService materialValidation, IProductionCalculationService calc)
    {
        _db = db;
        _orderService = orderService;
        _materialValidation = materialValidation;
        _calc = calc;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? date = null)
    {
        var query = _db.WorkOrders.Include(o => o.Product).Include(o => o.ProductionLine).AsQueryable();

        if (!string.IsNullOrEmpty(status) && status != "active")
            query = query.Where(o => o.Status == status);
        else if (status == "active")
            query = query.Where(o => o.Status == OrderStatus.InProgress || o.Status == OrderStatus.Pending);

        if (date == "today")
            query = query.Where(o => o.StartDate.Date == DateTime.Today);

        var orders = await query.OrderByDescending(o => o.StartDate).Select(o => new
        {
            o.Id,
            ProductName = o.Product.Name,
            LineName = o.ProductionLine == null ? "Не назначена" : o.ProductionLine.Name,
            o.Quantity, o.StartDate, o.EstimatedEndDate, o.Status, o.Progress
        }).ToListAsync();

        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        if (dto.Quantity <= 0) return BadRequest("Количество должно быть больше 0");

        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product is null) return BadRequest("Продукт не найден");

        var (isValid, errors) = await _materialValidation.CheckMaterialsAvailabilityAsync(dto.ProductId, dto.Quantity);
        if (!isValid) return BadRequest(new { errors });

        float efficiency = 1.0f;
        if (dto.LineId.HasValue)
        {
            var line = await _db.ProductionLines.FindAsync(dto.LineId.Value);
            if (line is not null) efficiency = line.EfficiencyFactor;
        }

        var minutes = _calc.CalculateMinutes(dto.Quantity, product.ProductionTimePerUnit, efficiency);
        var start = DateTime.Now;

        var order = new WorkOrder
        {
            ProductId = dto.ProductId,
            ProductionLineId = dto.LineId,
            Quantity = dto.Quantity,
            StartDate = start,
            EstimatedEndDate = _calc.CalculateEndDate(start, minutes),
            Status = OrderStatus.Pending
        };

        _db.WorkOrders.Add(order);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = order.Id }, new { order.Id, order.Status, order.EstimatedEndDate, TotalMinutes = minutes });
    }

    [HttpPut("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressDto dto)
    {
        var (success, error) = await _orderService.UpdateProgressAsync(id, dto.Percent);
        if (!success) return BadRequest(error);
        return Ok();
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetDetails(int id)
    {
        var order = await _db.WorkOrders
            .Include(o => o.Product).ThenInclude(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .Include(o => o.ProductionLine)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        return Ok(new
        {
            order.Id, order.Status, order.Quantity, order.Progress,
            order.StartDate, order.EstimatedEndDate,
            Product = new { order.Product.Id, order.Product.Name, order.Product.Category, order.Product.ProductionTimePerUnit },
            Line = order.ProductionLine is null ? null : new { order.ProductionLine.Id, order.ProductionLine.Name, order.ProductionLine.EfficiencyFactor },
            Materials = order.Product.ProductMaterials.Select(pm => new
            {
                pm.Material.Name, pm.Material.UnitOfMeasure, pm.QuantityNeeded,
                TotalNeeded = pm.QuantityNeeded * order.Quantity
            })
        });
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(int id)
    {
        var (success, error) = await _orderService.StartOrderAsync(id);
        if (!success) return BadRequest(error);
        return Ok();
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _orderService.CancelOrderAsync(id);
        if (!success) return BadRequest(error);
        return Ok();
    }
}

public record CreateOrderDto(int ProductId, int Quantity, int? LineId);
public record UpdateProgressDto(int Percent);
