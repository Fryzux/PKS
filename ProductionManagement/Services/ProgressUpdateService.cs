using Microsoft.EntityFrameworkCore;
using ProductionManagement.Data;
using ProductionManagement.Models;

namespace ProductionManagement.Services;

public class ProgressUpdateService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProgressUpdateService> _logger;

    // Интервал автообновления прогресса
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    public ProgressUpdateService(IServiceScopeFactory scopeFactory, ILogger<ProgressUpdateService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProgressUpdateService запущен");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                await UpdateProgressAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при автообновлении прогресса");
            }
        }
    }

    private async Task UpdateProgressAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activeOrders = await db.WorkOrders
            .Where(o => o.Status == OrderStatus.InProgress)
            .ToListAsync(ct);

        if (!activeOrders.Any()) return;

        var now = DateTime.Now;
        var completedIds = new List<int>();

        foreach (var order in activeOrders)
        {
            var totalSeconds = (order.EstimatedEndDate - order.StartDate).TotalSeconds;
            if (totalSeconds <= 0) continue;

            var elapsed = (now - order.StartDate).TotalSeconds;
            var computed = (int)Math.Clamp(elapsed / totalSeconds * 100, 0, 100);

            // Прогресс не откатывается назад (учитываем ручные правки)
            if (computed > order.Progress)
            {
                order.Progress = computed;

                if (computed >= 100)
                {
                    order.Status = OrderStatus.Completed;
                    completedIds.Add(order.Id);
                    _logger.LogInformation("Заказ #{Id} автоматически завершён", order.Id);
                }
            }
        }

        if (activeOrders.Any(o => o.Progress > 0))
        {
            // Освобождаем линии завершённых заказов
            if (completedIds.Any())
            {
                var lines = await db.ProductionLines
                    .Where(l => l.CurrentWorkOrderId != null && completedIds.Contains(l.CurrentWorkOrderId.Value))
                    .ToListAsync(ct);

                foreach (var line in lines)
                    line.CurrentWorkOrderId = null;
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Прогресс обновлён для {Count} заказов", activeOrders.Count);
        }
    }
}
