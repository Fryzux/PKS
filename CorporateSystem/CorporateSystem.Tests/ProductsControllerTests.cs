using CorporateSystem.Controllers;
using CorporateSystem.Data;
using CorporateSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CorporateSystem.Tests;

public class ProductsControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        var logger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_context, logger.Object);

        // Seed test data
        _context.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Тестовый товар 1",
                Description = "Описание 1",
                Price = 100.00m,
                StockQuantity = 10,
                Category = "Тест"
            },
            new Product
            {
                Id = 2,
                Name = "Тестовый товар 2",
                Description = "Описание 2",
                Price = 200.00m,
                StockQuantity = 5,
                Category = "Тест"
            }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsProduct()
    {
        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Тестовый товар 1", product.Name);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreated()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "Новый товар",
            Description = "Описание нового товара",
            Price = 150.00m,
            StockQuantity = 20,
            Category = "Новая категория"
        };

        // Act
        var result = await _controller.Create(newProduct);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createdResult.Value);
        Assert.Equal("Новый товар", product.Name);
        Assert.Equal(3, await _context.Products.CountAsync());
    }

    [Fact]
    public async Task Update_ExistingProduct_ReturnsNoContent()
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 1,
            Name = "Обновлённый товар",
            Price = 999.99m,
            StockQuantity = 100,
            Category = "Обновлённая"
        };

        // Act
        var result = await _controller.Update(1, updatedProduct);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var dbProduct = await _context.Products.FindAsync(1);
        Assert.Equal("Обновлённый товар", dbProduct!.Name);
        Assert.Equal(999.99m, dbProduct.Price);
    }

    [Fact]
    public async Task Update_MismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Тест", Price = 10m, StockQuantity = 1 };

        // Act
        var result = await _controller.Update(2, product);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingProduct_ReturnsNoContent()
    {
        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(1, await _context.Products.CountAsync());
    }

    [Fact]
    public async Task Delete_NonExistingProduct_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
