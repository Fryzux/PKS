using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveOrders = await _db.WorkOrders
            .CountAsync(o => o.Status == OrderStatus.InProgress || o.Status == OrderStatus.Pending);
        ViewBag.ActiveLines = await _db.ProductionLines.CountAsync(l => l.Status == LineStatus.Active);
        ViewBag.LowStockCount = await _db.Materials.CountAsync(m => m.Quantity <= m.MinimalStock);
        ViewBag.TotalProducts = await _db.Products.CountAsync();

        ViewBag.RecentOrders = await _db.WorkOrders
            .Include(o => o.Product)
            .Include(o => o.ProductionLine)
            .Where(o => o.Status == OrderStatus.InProgress || o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.StartDate)
            .Take(5)
            .ToListAsync();

        ViewBag.LowStockMaterials = await _db.Materials
            .Where(m => m.Quantity <= m.MinimalStock)
            .OrderBy(m => m.Quantity)
            .Take(6)
            .ToListAsync();

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
