using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Services;

public interface IWorkOrderService
{
    Task<(bool Success, string Error)> StartOrderAsync(int orderId);
    Task<(bool Success, string Error)> CancelOrderAsync(int orderId);
    Task<(bool Success, string Error)> UpdateProgressAsync(int orderId, int percent);
}

public class WorkOrderService : IWorkOrderService
{
    private readonly AppDbContext _db;

    public WorkOrderService(AppDbContext db) => _db = db;

    public async Task<(bool Success, string Error)> StartOrderAsync(int orderId)
    {
        var order = await _db.WorkOrders
            .Include(o => o.ProductionLine)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null) return (false, "Заказ не найден");
        if (order.Status != OrderStatus.Pending) return (false, "Только заказы со статусом Pending можно запустить");
        if (order.ProductionLineId is null) return (false, "Не назначена производственная линия");

        // Списываем материалы
        var deductError = await DeductMaterialsAsync(order.ProductId, order.Quantity);
        if (deductError is not null) return (false, deductError);

        order.Status = OrderStatus.InProgress;
        order.StartDate = DateTime.Now;

        if (order.ProductionLine is not null)
            order.ProductionLine.CurrentWorkOrderId = orderId;

        await _db.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> CancelOrderAsync(int orderId)
    {
        var order = await _db.WorkOrders.FindAsync(orderId);
        if (order is null) return (false, "Заказ не найден");
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
            return (false, "Нельзя отменить завершённый или уже отменённый заказ");

        // Если заказ был в работе — возвращаем материалы
        if (order.Status == OrderStatus.InProgress)
            await RestoreMaterialsAsync(order.ProductId, order.Quantity);

        order.Status = OrderStatus.Cancelled;

        var line = await _db.ProductionLines
            .FirstOrDefaultAsync(l => l.CurrentWorkOrderId == orderId);
        if (line is not null) line.CurrentWorkOrderId = null;

        await _db.SaveChangesAsync();
        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateProgressAsync(int orderId, int percent)
    {
        if (percent is < 0 or > 100) return (false, "Прогресс должен быть от 0 до 100");

        var order = await _db.WorkOrders.FindAsync(orderId);
        if (order is null) return (false, "Заказ не найден");
        if (order.Status != OrderStatus.InProgress)
            return (false, "Обновить прогресс можно только у заказа в статусе 'В работе'");

        order.Progress = percent;

        if (percent == 100)
        {
            order.Status = OrderStatus.Completed;

            // Освобождаем линию от завершённого заказа
            var line = await _db.ProductionLines
                .FirstOrDefaultAsync(l => l.CurrentWorkOrderId == orderId);
            if (line is not null)
                line.CurrentWorkOrderId = null;
        }

        await _db.SaveChangesAsync();
        return (true, string.Empty);
    }

    // Списывает материалы со склада. Возвращает null при успехе или сообщение об ошибке.
    private async Task<string?> DeductMaterialsAsync(int productId, int quantity)
    {
        var productMaterials = await _db.ProductMaterials
            .Include(pm => pm.Material)
            .Where(pm => pm.ProductId == productId)
            .ToListAsync();

        foreach (var pm in productMaterials)
        {
            var needed = pm.QuantityNeeded * quantity;
            if (pm.Material.Quantity < needed)
                return $"Недостаточно материала '{pm.Material.Name}': нужно {needed}, есть {pm.Material.Quantity}";
        }

        foreach (var pm in productMaterials)
            pm.Material.Quantity -= pm.QuantityNeeded * quantity;

        return null;
    }

    // Возвращает материалы на склад при отмене заказа.
    private async Task RestoreMaterialsAsync(int productId, int quantity)
    {
        var productMaterials = await _db.ProductMaterials
            .Include(pm => pm.Material)
            .Where(pm => pm.ProductId == productId)
            .ToListAsync();

        foreach (var pm in productMaterials)
            pm.Material.Quantity += pm.QuantityNeeded * quantity;
    }
}
