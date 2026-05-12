using System.ComponentModel.DataAnnotations;

namespace ProductionManagement.Models;

public class ProductMaterial
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int MaterialId { get; set; }
    public Material Material { get; set; } = null!;

    [Range(0.001, double.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    public decimal QuantityNeeded { get; set; }
}
