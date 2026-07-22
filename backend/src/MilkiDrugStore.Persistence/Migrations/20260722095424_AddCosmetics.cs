using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCosmetics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchBatchId",
                table: "SaleItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchBatchId",
                table: "PurchaseItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchBatchId",
                table: "InventoryTransactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchBatchId",
                table: "ExpiredRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchBatchId",
                table: "DamageRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cosmetics",
                columns: table => new
                {
                    CosmeticId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cosmetics", x => x.CosmeticId);
                    table.ForeignKey(
                        name: "FK_Cosmetics_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cosmetics_UnitTypes_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "UnitTypes",
                        principalColumn: "UnitTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CosmeticBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CosmeticId = table.Column<int>(type: "INTEGER", nullable: false),
                    BatchNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    QuantityReceived = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityIssued = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityDamaged = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityExpired = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CosmeticBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_CosmeticBatches_Cosmetics_CosmeticId",
                        column: x => x.CosmeticId,
                        principalTable: "Cosmetics",
                        principalColumn: "CosmeticId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_CosmeticBatchBatchId",
                table: "SaleItems",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_CosmeticBatchBatchId",
                table: "PurchaseItems",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CosmeticBatchBatchId",
                table: "InventoryTransactions",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_CosmeticBatchBatchId",
                table: "ExpiredRecords",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_CosmeticBatchBatchId",
                table: "DamageRecords",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_BatchNumber",
                table: "CosmeticBatches",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_CosmeticId",
                table: "CosmeticBatches",
                column: "CosmeticId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_CategoryId",
                table: "Cosmetics",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_UnitTypeId",
                table: "Cosmetics",
                column: "UnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "DamageRecords",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "ExpiredRecords",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_CosmeticBatches_CosmeticBatchBatchId",
                table: "InventoryTransactions",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_CosmeticBatches_CosmeticBatchBatchId",
                table: "PurchaseItems",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_CosmeticBatches_CosmeticBatchBatchId",
                table: "SaleItems",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "DamageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_CosmeticBatches_CosmeticBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_CosmeticBatches_CosmeticBatchBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_CosmeticBatches_CosmeticBatchBatchId",
                table: "SaleItems");

            migrationBuilder.DropTable(
                name: "CosmeticBatches");

            migrationBuilder.DropTable(
                name: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_CosmeticBatchBatchId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseItems_CosmeticBatchBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_CosmeticBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ExpiredRecords_CosmeticBatchBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropIndex(
                name: "IX_DamageRecords_CosmeticBatchBatchId",
                table: "DamageRecords");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchBatchId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchBatchId",
                table: "DamageRecords");
        }
    }
}
