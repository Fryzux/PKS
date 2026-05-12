using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers;

public class ProductionLinesController : Controller
{
    private readonly AppDbContext _db;
    public ProductionLinesController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var lines = await _db.ProductionLines
            .Include(l => l.WorkOrders.Where(o => o.Status != OrderStatus.Cancelled))
            .ThenInclude(o => o.Product)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return View(lines);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, float efficiencyFactor = 1.0f)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Название линии не может быть пустым";
            return RedirectToAction(nameof(Index));
        }

        if (efficiencyFactor < 0.5f || efficiencyFactor > 2.0f)
        {
            TempData["Error"] = "Коэффициент эффективности должен быть от 0.5 до 2.0";
            return RedirectToAction(nameof(Index));
        }

        var line = new ProductionLine
        {
            Name = name.Trim(),
            Status = LineStatus.Stopped,
            EfficiencyFactor = efficiencyFactor
        };

        _db.ProductionLines.Add(line);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Линия '{line.Name}' создана";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line is null) return RedirectToAction(nameof(Index));

        if (line.Status == LineStatus.Active)
        {
            var hasActive = await _db.WorkOrders
                .AnyAsync(o => o.ProductionLineId == id && o.Status == OrderStatus.InProgress);
            if (hasActive)
            {
                TempData["Error"] = $"Нельзя остановить линию '{line.Name}': есть активный заказ в работе. Сначала завершите или отмените заказ.";
                return RedirectToAction(nameof(Index));
            }
        }

        line.Status = line.Status == LineStatus.Active ? LineStatus.Stopped : LineStatus.Active;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Статус линии '{line.Name}' изменён";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateEfficiency(int id, float factor)
    {
        var line = await _db.ProductionLines.FindAsync(id);
        if (line is not null && factor >= 0.5f && factor <= 2.0f)
        {
            line.EfficiencyFactor = factor;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Коэффициент эффективности обновлён";
        }
        return RedirectToAction(nameof(Index));
    }
}
