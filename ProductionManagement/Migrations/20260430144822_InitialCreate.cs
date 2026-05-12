using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductionManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MinimalStock = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    EfficiencyFactor = table.Column<float>(type: "REAL", nullable: false),
                    CurrentWorkOrderId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Specifications = table.Column<string>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MinimalStock = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionTimePerUnit = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaterials",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityNeeded = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaterials", x => new { x.ProductId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMaterials_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductionLineId = table.Column<int>(type: "INTEGER", nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstimatedEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Progress = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_ProductionLines_ProductionLineId",
                        column: x => x.ProductionLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Materials",
                columns: new[] { "Id", "MinimalStock", "Name", "Quantity", "UnitOfMeasure" },
                values: new object[,]
                {
                    { 1, 100m, "Сталь листовая", 500m, "кг" },
                    { 2, 150m, "Алюминий", 80m, "кг" },
                    { 3, 50m, "Пластик АБС", 300m, "кг" },
                    { 4, 500m, "Болт М8", 2000m, "шт" },
                    { 5, 50m, "Масло машинное", 20m, "литр" }
                });

            migrationBuilder.InsertData(
                table: "ProductionLines",
                columns: new[] { "Id", "CurrentWorkOrderId", "EfficiencyFactor", "Name", "Status" },
                values: new object[,]
                {
                    { 1, null, 1.2f, "Линия А - Металлообработка", "Active" },
                    { 2, null, 1f, "Линия Б - Сборка", "Stopped" },
                    { 3, null, 0.8f, "Линия В - Пластик", "Active" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "Description", "MinimalStock", "Name", "ProductionTimePerUnit", "Specifications" },
                values: new object[,]
                {
                    { 1, "Металлические изделия", "Корпус центробежного насоса", 10, "Корпус насоса", 120, null },
                    { 2, "Металлические изделия", "Стальной кронштейн", 50, "Кронштейн крепёжный", 30, null },
                    { 3, "Электроника", "Пластиковая панель", 20, "Панель управления", 60, null }
                });

            migrationBuilder.InsertData(
                table: "ProductMaterials",
                columns: new[] { "MaterialId", "ProductId", "QuantityNeeded" },
                values: new object[,]
                {
                    { 1, 1, 5.5m },
                    { 4, 1, 8m },
                    { 1, 2, 1.2m },
                    { 4, 2, 4m },
                    { 3, 3, 0.8m }
                });

            migrationBuilder.InsertData(
                table: "WorkOrders",
                columns: new[] { "Id", "EstimatedEndDate", "ProductId", "ProductionLineId", "Progress", "Quantity", "StartDate", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 4, 28, 19, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 40, 5, new DateTime(2026, 4, 28, 9, 0, 0, 0, DateTimeKind.Unspecified), "InProgress" },
                    { 2, new DateTime(2026, 4, 29, 19, 0, 0, 0, DateTimeKind.Unspecified), 2, null, 0, 20, new DateTime(2026, 4, 29, 9, 0, 0, 0, DateTimeKind.Unspecified), "Pending" },
                    { 3, new DateTime(2026, 4, 27, 9, 0, 0, 0, DateTimeKind.Unspecified), 3, 3, 100, 10, new DateTime(2026, 4, 26, 9, 0, 0, 0, DateTimeKind.Unspecified), "Completed" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductMaterials_MaterialId",
                table: "ProductMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductId",
                table: "WorkOrders",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_ProductionLineId",
                table: "WorkOrders",
                column: "ProductionLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductMaterials");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "ProductionLines");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
