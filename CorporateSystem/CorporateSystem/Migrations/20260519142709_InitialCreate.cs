using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CorporateSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Name", "Price", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Электроника", new DateTime(2025, 1, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Ноутбук с процессором Intel Core i5, 8 ГБ ОЗУ, SSD 512 ГБ, экран 15.6\" Full HD", "Ноутбук ASUS VivoBook 15", 54990.00m, 25 },
                    { 2, "Электроника", new DateTime(2025, 2, 10, 12, 0, 0, 0, DateTimeKind.Utc), "6.4\" Super AMOLED, 128 ГБ, камера 50 МП, аккумулятор 5000 мАч", "Смартфон Samsung Galaxy A54", 32990.00m, 50 },
                    { 3, "Мебель", new DateTime(2025, 3, 5, 9, 0, 0, 0, DateTimeKind.Utc), "Эргономичное кресло с поддержкой поясницы, регулируемые подлокотники", "Кресло офисное Ergonomic Pro", 18500.00m, 15 },
                    { 4, "Аксессуары", new DateTime(2025, 4, 20, 14, 0, 0, 0, DateTimeKind.Utc), "Беспроводные наушники с активным шумоподавлением, до 30 часов работы", "Наушники Sony WH-1000XM5", 29990.00m, 30 },
                    { 5, "Электроника", new DateTime(2025, 5, 1, 11, 0, 0, 0, DateTimeKind.Utc), "4K UHD, IPS, USB-C, 60 Гц, 99% sRGB, поворотная подставка", "Монитор Dell UltraSharp 27\"", 42500.00m, 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
