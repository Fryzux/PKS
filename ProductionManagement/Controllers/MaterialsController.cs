using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Controllers;

public class MaterialsController : Controller
{
    private readonly AppDbContext _db;
    public MaterialsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
        => View(await _db.Materials.OrderBy(m => m.Name).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Material model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Ошибка при добавлении материала: проверьте введённые данные";
            return RedirectToAction(nameof(Index));
        }
        _db.Materials.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Материал добавлен";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Material model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Ошибка при редактировании материала: проверьте введённые данные";
            return RedirectToAction(nameof(Index));
        }
        _db.Materials.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Материал обновлён";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Replenish(int id, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["Error"] = "Количество для пополнения должно быть больше 0";
            return RedirectToAction(nameof(Index));
        }

        var material = await _db.Materials.FindAsync(id);
        if (material is not null)
        {
            material.Quantity += amount;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Запас '{material.Name}' пополнен на {amount}";
        }
        else
        {
            TempData["Error"] = "Материал не найден";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var material = await _db.Materials
            .Include(m => m.ProductMaterials)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (material is null)
        {
            TempData["Error"] = "Материал не найден";
            return RedirectToAction(nameof(Index));
        }

        if (material.ProductMaterials.Any())
        {
            TempData["Error"] = $"Нельзя удалить материал '{material.Name}': он используется в {material.ProductMaterials.Count} продукт(ах). Сначала удалите его из состава продуктов.";
            return RedirectToAction(nameof(Index));
        }

        _db.Materials.Remove(material);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Материал '{material.Name}' удалён";
        return RedirectToAction(nameof(Index));
    }
}
