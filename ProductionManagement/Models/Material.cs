using System.ComponentModel.DataAnnotations;

namespace ProductionManagement.Models;

public class Material
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название материала обязательно")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [MaxLength(20)]
    public string UnitOfMeasure { get; set; } = "шт";

    [Range(0, double.MaxValue)]
    public decimal MinimalStock { get; set; }

    public bool IsLowStock => Quantity <= MinimalStock;

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
}
