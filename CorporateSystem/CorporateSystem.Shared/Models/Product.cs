using System.ComponentModel.DataAnnotations;

namespace CorporateSystem.Shared.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Название товара обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Цена обязательна")]
    [Range(0.01, 999999.99, ErrorMessage = "Цена должна быть от 0.01 до 999 999.99")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Количество обязательно")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
    public int StockQuantity { get; set; }

    [StringLength(100, ErrorMessage = "Категория не должна превышать 100 символов")]
    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
