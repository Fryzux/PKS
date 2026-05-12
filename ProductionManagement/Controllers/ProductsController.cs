using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers;

public class ProductsController : Controller
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index([FromQuery] string? category = null, [FromQuery] string? search = null)
    {
        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);

        var categories = await _db.Products.Select(p => p.Category).Distinct().ToListAsync();
        ViewBag.Categories = categories;
        ViewBag.SelectedCategory = category;
        ViewBag.Search = search;

        var list = await query.OrderBy(p => p.Name).ToListAsync();
        if (!string.IsNullOrEmpty(search))
            list = list.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.ProductMaterials).ThenInclude(pm => pm.Material)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound();

        ViewBag.AllMaterials = await _db.Materials.ToListAsync();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        _db.Products.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Продукт создан";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        _db.Products.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Продукт обновлён";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    public async Task<IActionResult> AddMaterial(int productId, int materialId, decimal quantityNeeded)
    {
        if (quantityNeeded <= 0)
        {
            TempData["Error"] = "Количество материала должно быть больше 0";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        var exists = await _db.ProductMaterials.AnyAsync(pm => pm.ProductId == productId && pm.MaterialId == materialId);
        if (exists)
        {
            TempData["Error"] = "Этот материал уже добавлен к продукту";
        }
        else
        {
            _db.ProductMaterials.Add(new ProductMaterial { ProductId = productId, MaterialId = materialId, QuantityNeeded = quantityNeeded });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Материал добавлен к продукту";
        }
        return RedirectToAction(nameof(Details), new { id = productId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveMaterial(int productId, int materialId)
    {
        var pm = await _db.ProductMaterials.FindAsync(productId, materialId);
        if (pm is not null)
        {
            _db.ProductMaterials.Remove(pm);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Details), new { id = productId });
    }
}
