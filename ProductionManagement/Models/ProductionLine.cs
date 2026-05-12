using System.ComponentModel.DataAnnotations;

namespace ProductionManagement.Models;

public class ProductionLine
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название линии обязательно")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = LineStatus.Stopped;

    [Range(0.5, 2.0, ErrorMessage = "Коэффициент должен быть от 0.5 до 2.0")]
    public float EfficiencyFactor { get; set; } = 1.0f;

    public int? CurrentWorkOrderId { get; set; }

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public static class LineStatus
{
    public const string Active = "Active";
    public const string Stopped = "Stopped";
}
