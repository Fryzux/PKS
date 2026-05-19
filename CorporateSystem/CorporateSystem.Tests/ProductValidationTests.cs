using System.ComponentModel.DataAnnotations;
using CorporateSystem.Shared.Models;

namespace CorporateSystem.Tests;

public class ProductValidationTests
{
    private List<ValidationResult> ValidateModel(Product product)
    {
        var context = new ValidationContext(product);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(product, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidProduct_PassesValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = "Тестовый товар",
            Price = 99.99m,
            StockQuantity = 10,
            Category = "Электроника"
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void EmptyName_FailsValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = "",
            Price = 99.99m,
            StockQuantity = 10
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void NegativePrice_FailsValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = "Товар",
            Price = -10m,
            StockQuantity = 5
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void ZeroPrice_FailsValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = "Товар",
            Price = 0m,
            StockQuantity = 5
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void NegativeStock_FailsValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = "Товар",
            Price = 100m,
            StockQuantity = -1
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains("StockQuantity"));
    }

    [Fact]
    public void NameTooLong_FailsValidation()
    {
        // Arrange
        var product = new Product
        {
            Name = new string('А', 201),
            Price = 100m,
            StockQuantity = 5
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Contains(results, r => r.MemberNames.Contains("Name"));
    }

    [Fact]
    public void MinimalValidProduct_PassesValidation()
    {
        // Arrange — минимальный набор полей
        var product = new Product
        {
            Name = "A",
            Price = 0.01m,
            StockQuantity = 0
        };

        // Act
        var results = ValidateModel(product);

        // Assert
        Assert.Empty(results);
    }
}
