using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;

namespace ProductionManagement.Services;

public interface IMaterialValidationService
{
    Task<(bool IsValid, List<string> Errors)> CheckMaterialsAvailabilityAsync(int productId, int quantity);
}

public class MaterialValidationService : IMaterialValidationService
{
    private readonly AppDbContext _db;

    public MaterialValidationService(AppDbContext db) => _db = db;

    public async Task<(bool IsValid, List<string> Errors)> CheckMaterialsAvailabilityAsync(int productId, int quantity)
    {
        var errors = new List<string>();

        var productMaterials = await _db.ProductMaterials
            .Include(pm => pm.Material)
            .Where(pm => pm.ProductId == productId)
            .ToListAsync();

        foreach (var pm in productMaterials)
        {
            var needed = pm.QuantityNeeded * quantity;
            if (pm.Material.Quantity < needed)
            {
                errors.Add($"Недостаточно '{pm.Material.Name}': нужно {needed} {pm.Material.UnitOfMeasure}, есть {pm.Material.Quantity}");
            }
        }

        return (!errors.Any(), errors);
    }
}
