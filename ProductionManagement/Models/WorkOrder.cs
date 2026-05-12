using System.ComponentModel.DataAnnotations;

namespace ProductionManagement.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? ProductionLineId { get; set; }
    public ProductionLine? ProductionLine { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    public int Quantity { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;

    public DateTime EstimatedEndDate { get; set; }

    [Required]
    public string Status { get; set; } = OrderStatus.Pending;

    public int Progress { get; set; } = 0;

    public bool IsOverdue => Status == OrderStatus.InProgress && DateTime.Now > EstimatedEndDate;
}

public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static string ToRussian(string status) => status switch
    {
        Pending    => "Ожидание",
        InProgress => "В работе",
        Completed  => "Завершён",
        Cancelled  => "Отменён",
        _          => status
    };
}
