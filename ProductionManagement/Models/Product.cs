using System.ComponentModel.DataAnnotations;

namespace ProductionManagement.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Specifications { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimalStock { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Время производства должно быть больше 0")]
    public int ProductionTimePerUnit { get; set; }

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
