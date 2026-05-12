using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;
using ProductionManagement.Services;

namespace ProductionManagement.Controllers;

public class WorkOrdersController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWorkOrderService _orderService;
    private readonly IMaterialValidationService _materialValidation;
    private readonly IProductionCalculationService _calc;

    public WorkOrdersController(AppDbContext db, IWorkOrderService orderService,
        IMaterialValidationService materialValidation, IProductionCalculationService calc)
    {
        _db = db;
        _orderService = orderService;
        _materialValidation = materialValidation;
        _calc = calc;
    }

    public async Task<IActionResult> Index([FromQuery] string? status = null)
    {
        var query = _db.WorkOrders
            .Include(o => o.Product)
            .Include(o => o.ProductionLine)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        ViewBag.StatusFilter = status;
        return View(await query.OrderByDescending(o => o.StartDate).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await _db.Products.OrderBy(p => p.Name).ToListAsync();
        ViewBag.Lines = await _db.ProductionLines
            .Where(l => l.Status == LineStatus.Active)
            .OrderBy(l => l.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(int productId, int quantity, int? lineId)
    {
        if (quantity <= 0) { TempData["Error"] = "Количество должно быть больше 0"; return RedirectToAction(nameof(Create)); }

        var product = await _db.Products.FindAsync(productId);
        if (product is null) { TempData["Error"] = "Продукт не найден"; return RedirectToAction(nameof(Index)); }

        var (isValid, errors) = await _materialValidation.CheckMaterialsAvailabilityAsync(productId, quantity);
        if (!isValid)
        {
            TempData["Error"] = "Недостаточно материалов: " + string.Join("; ", errors);
            return RedirectToAction(nameof(Create));
        }

        float efficiency = 1.0f;
        if (lineId.HasValue)
        {
            var line = await _db.ProductionLines.FindAsync(lineId.Value);
            if (line is not null) efficiency = line.EfficiencyFactor;
        }

        var minutes = _calc.CalculateMinutes(quantity, product.ProductionTimePerUnit, efficiency);
        var start = DateTime.Now;

        var order = new WorkOrder
        {
            ProductId = productId,
            ProductionLineId = lineId,
            Quantity = quantity,
            StartDate = start,
            EstimatedEndDate = _calc.CalculateEndDate(start, minutes),
            Status = OrderStatus.Pending
        };

        _db.WorkOrders.Add(order);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Заказ создан. Расчётное время: {Math.Round(minutes / 60.0, 1)} ч.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        var (success, error) = await _orderService.StartOrderAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "Заказ запущен" : error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var (success, error) = await _orderService.CancelOrderAsync(id);
        TempData[success ? "Success" : "Error"] = success ? "Заказ отменён" : error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProgress(int id, int percent)
    {
        var (success, error) = await _orderService.UpdateProgressAsync(id, percent);
        TempData[success ? "Success" : "Error"] = success ? "Прогресс обновлён" : error;
        return RedirectToAction(nameof(Index));
    }
}
