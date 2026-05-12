using Microsoft.EntityFrameworkCore;
using ProductionManagement.Models;

namespace ProductionManagement.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductionLine> ProductionLines { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<ProductMaterial> ProductMaterials { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductMaterial>()
            .HasKey(pm => new { pm.ProductId, pm.MaterialId });

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(pm => pm.Product)
            .WithMany(p => p.ProductMaterials)
            .HasForeignKey(pm => pm.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProductMaterial>()
            .HasOne(pm => pm.Material)
            .WithMany(m => m.ProductMaterials)
            .HasForeignKey(pm => pm.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(w => w.Product)
            .WithMany(p => p.WorkOrders)
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkOrder>()
            .HasOne(w => w.ProductionLine)
            .WithMany(l => l.WorkOrders)
            .HasForeignKey(w => w.ProductionLineId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Material>()
            .Property(m => m.Quantity)
            .HasColumnType("decimal(18,3)");

        modelBuilder.Entity<Material>()
            .Property(m => m.MinimalStock)
            .HasColumnType("decimal(18,3)");

        modelBuilder.Entity<ProductMaterial>()
            .Property(pm => pm.QuantityNeeded)
            .HasColumnType("decimal(18,3)");

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Material>().HasData(
            new Material { Id = 1, Name = "Сталь листовая", Quantity = 500, UnitOfMeasure = "кг", MinimalStock = 100 },
            new Material { Id = 2, Name = "Алюминий", Quantity = 80, UnitOfMeasure = "кг", MinimalStock = 150 },
            new Material { Id = 3, Name = "Пластик АБС", Quantity = 300, UnitOfMeasure = "кг", MinimalStock = 50 },
            new Material { Id = 4, Name = "Болт М8", Quantity = 2000, UnitOfMeasure = "шт", MinimalStock = 500 },
            new Material { Id = 5, Name = "Масло машинное", Quantity = 20, UnitOfMeasure = "литр", MinimalStock = 50 }
        );

        modelBuilder.Entity<ProductionLine>().HasData(
            new ProductionLine { Id = 1, Name = "Линия А - Металлообработка", Status = LineStatus.Active, EfficiencyFactor = 1.2f },
            new ProductionLine { Id = 2, Name = "Линия Б - Сборка", Status = LineStatus.Stopped, EfficiencyFactor = 1.0f },
            new ProductionLine { Id = 3, Name = "Линия В - Пластик", Status = LineStatus.Active, EfficiencyFactor = 0.8f }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Корпус насоса", Category = "Металлические изделия", Description = "Корпус центробежного насоса", MinimalStock = 10, ProductionTimePerUnit = 120 },
            new Product { Id = 2, Name = "Кронштейн крепёжный", Category = "Металлические изделия", Description = "Стальной кронштейн", MinimalStock = 50, ProductionTimePerUnit = 30 },
            new Product { Id = 3, Name = "Панель управления", Category = "Электроника", Description = "Пластиковая панель", MinimalStock = 20, ProductionTimePerUnit = 60 }
        );

        modelBuilder.Entity<ProductMaterial>().HasData(
            new ProductMaterial { ProductId = 1, MaterialId = 1, QuantityNeeded = 5.5m },
            new ProductMaterial { ProductId = 1, MaterialId = 4, QuantityNeeded = 8 },
            new ProductMaterial { ProductId = 2, MaterialId = 1, QuantityNeeded = 1.2m },
            new ProductMaterial { ProductId = 2, MaterialId = 4, QuantityNeeded = 4 },
            new ProductMaterial { ProductId = 3, MaterialId = 3, QuantityNeeded = 0.8m }
        );

        var start = new DateTime(2026, 4, 28, 9, 0, 0);
        modelBuilder.Entity<WorkOrder>().HasData(
            new WorkOrder { Id = 1, ProductId = 1, ProductionLineId = 1, Quantity = 5, StartDate = start, EstimatedEndDate = start.AddHours(10), Status = OrderStatus.InProgress, Progress = 40 },
            new WorkOrder { Id = 2, ProductId = 2, ProductionLineId = null, Quantity = 20, StartDate = start.AddDays(1), EstimatedEndDate = start.AddDays(1).AddHours(10), Status = OrderStatus.Pending, Progress = 0 },
            new WorkOrder { Id = 3, ProductId = 3, ProductionLineId = 3, Quantity = 10, StartDate = start.AddDays(-2), EstimatedEndDate = start.AddDays(-1), Status = OrderStatus.Completed, Progress = 100 }
        );
    }
}
