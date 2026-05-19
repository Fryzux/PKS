using CorporateSystem.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CorporateSystem.Data;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.StockQuantity)
            .IsRequired();

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("datetime('now')")
            .IsRequired();

        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Category);

        // Seed data — 5 демо-товаров
        builder.HasData(
            new Product
            {
                Id = 1,
                Name = "Ноутбук ASUS VivoBook 15",
                Description = "Ноутбук с процессором Intel Core i5, 8 ГБ ОЗУ, SSD 512 ГБ, экран 15.6\" Full HD",
                Price = 54990.00m,
                StockQuantity = 25,
                Category = "Электроника",
                CreatedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 2,
                Name = "Смартфон Samsung Galaxy A54",
                Description = "6.4\" Super AMOLED, 128 ГБ, камера 50 МП, аккумулятор 5000 мАч",
                Price = 32990.00m,
                StockQuantity = 50,
                Category = "Электроника",
                CreatedAt = new DateTime(2025, 2, 10, 12, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 3,
                Name = "Кресло офисное Ergonomic Pro",
                Description = "Эргономичное кресло с поддержкой поясницы, регулируемые подлокотники",
                Price = 18500.00m,
                StockQuantity = 15,
                Category = "Мебель",
                CreatedAt = new DateTime(2025, 3, 5, 9, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 4,
                Name = "Наушники Sony WH-1000XM5",
                Description = "Беспроводные наушники с активным шумоподавлением, до 30 часов работы",
                Price = 29990.00m,
                StockQuantity = 30,
                Category = "Аксессуары",
                CreatedAt = new DateTime(2025, 4, 20, 14, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = 5,
                Name = "Монитор Dell UltraSharp 27\"",
                Description = "4K UHD, IPS, USB-C, 60 Гц, 99% sRGB, поворотная подставка",
                Price = 42500.00m,
                StockQuantity = 10,
                Category = "Электроника",
                CreatedAt = new DateTime(2025, 5, 1, 11, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
