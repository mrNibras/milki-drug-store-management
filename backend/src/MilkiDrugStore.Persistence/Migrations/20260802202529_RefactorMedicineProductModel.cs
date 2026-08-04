using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMedicineProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_UnitTypes_UnitTypeId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_CosmeticCategories_CosmeticCategoryId",
                table: "Cosmetics");

            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_UnitTypes_UnitTypeId",
                table: "Cosmetics");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_MedicineBatches_MedicineBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Medicines_MedicineId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineBatches_Medicines_MedicineId",
                table: "MedicineBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicines_Categories_CategoryId",
                table: "Medicines");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicines_UnitTypes_UnitTypeId",
                table: "Medicines");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Medicines_MedicineId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Medicines_MedicineId",
                table: "SaleItems");

            migrationBuilder.DropTable(
                name: "CosmeticCategories");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_CategoryId",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_UnitTypeId",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_MedicineBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Cosmetics_CosmeticCategoryId",
                table: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_Cosmetics_UnitTypeId",
                table: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UnitTypeId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "MedicineBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "UnitTypeId",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "MedicineId",
                table: "SaleItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_MedicineId",
                table: "SaleItems",
                newName: "IX_SaleItems_ProductId");

            migrationBuilder.RenameColumn(
                name: "MedicineId",
                table: "PurchaseItems",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseItems_MedicineId",
                table: "PurchaseItems",
                newName: "IX_PurchaseItems_ProductId");

            migrationBuilder.RenameColumn(
                name: "LowStockThreshold",
                table: "Medicines",
                newName: "ReorderLevel");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Medicines",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "MedicineId",
                table: "Medicines",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "MedicineId",
                table: "MedicineBatches",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicineBatches_MedicineId",
                table: "MedicineBatches",
                newName: "IX_MedicineBatches_ProductId");

            migrationBuilder.RenameColumn(
                name: "MedicineId",
                table: "InventoryTransactions",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_MedicineId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_ProductId");

            migrationBuilder.RenameColumn(
                name: "CosmeticCategoryId",
                table: "Cosmetics",
                newName: "CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "BatchSelectionMode",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Medicines",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Medicines",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DosageForm",
                table: "Medicines",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Medicines",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "Medicines",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "Medicines",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "Medicines",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Strength",
                table: "Medicines",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Medicines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManufacturingDate",
                table: "MedicineBatches",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "MedicineBatches",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Barcode",
                table: "Medicines",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_DosageForm",
                table: "Medicines",
                column: "DosageForm");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_GenericName",
                table: "Medicines",
                column: "GenericName");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_ProductCode",
                table: "Medicines",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Strength",
                table: "Medicines",
                column: "Strength");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_SupplierId",
                table: "MedicineBatches",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Medicines_ProductId",
                table: "InventoryTransactions",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineBatches_Medicines_ProductId",
                table: "MedicineBatches",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineBatches_Suppliers_SupplierId",
                table: "MedicineBatches",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_Medicines_ProductId",
                table: "PurchaseItems",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Medicines_ProductId",
                table: "SaleItems",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Medicines_ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineBatches_Medicines_ProductId",
                table: "MedicineBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicineBatches_Suppliers_SupplierId",
                table: "MedicineBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Medicines_ProductId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Medicines_ProductId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_Barcode",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_DosageForm",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_GenericName",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_ProductCode",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_Strength",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_MedicineBatches_SupplierId",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "BatchSelectionMode",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "DosageForm",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "Strength",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "ManufacturingDate",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "MedicineBatches");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "SaleItems",
                newName: "MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                newName: "IX_SaleItems_MedicineId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "PurchaseItems",
                newName: "MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseItems_ProductId",
                table: "PurchaseItems",
                newName: "IX_PurchaseItems_MedicineId");

            migrationBuilder.RenameColumn(
                name: "ReorderLevel",
                table: "Medicines",
                newName: "LowStockThreshold");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Medicines",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Medicines",
                newName: "MedicineId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "MedicineBatches",
                newName: "MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicineBatches_ProductId",
                table: "MedicineBatches",
                newName: "IX_MedicineBatches_MedicineId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "InventoryTransactions",
                newName: "MedicineId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_MedicineId");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Cosmetics",
                newName: "CosmeticCategoryId");

            migrationBuilder.AddColumn<int>(
                name: "MedicineBatchBatchId",
                table: "InventoryTransactions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitTypeId",
                table: "Categories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CosmeticCategories",
                columns: table => new
                {
                    CosmeticCategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CosmeticCategories", x => x.CosmeticCategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_CategoryId",
                table: "Medicines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_UnitTypeId",
                table: "Medicines",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_MedicineBatchBatchId",
                table: "InventoryTransactions",
                column: "MedicineBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_CosmeticCategoryId",
                table: "Cosmetics",
                column: "CosmeticCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_UnitTypeId",
                table: "Cosmetics",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UnitTypeId",
                table: "Categories",
                column: "UnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_UnitTypes_UnitTypeId",
                table: "Categories",
                column: "UnitTypeId",
                principalTable: "UnitTypes",
                principalColumn: "UnitTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_CosmeticCategories_CosmeticCategoryId",
                table: "Cosmetics",
                column: "CosmeticCategoryId",
                principalTable: "CosmeticCategories",
                principalColumn: "CosmeticCategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_UnitTypes_UnitTypeId",
                table: "Cosmetics",
                column: "UnitTypeId",
                principalTable: "UnitTypes",
                principalColumn: "UnitTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_MedicineBatches_MedicineBatchBatchId",
                table: "InventoryTransactions",
                column: "MedicineBatchBatchId",
                principalTable: "MedicineBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Medicines_MedicineId",
                table: "InventoryTransactions",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicineBatches_Medicines_MedicineId",
                table: "MedicineBatches",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicines_Categories_CategoryId",
                table: "Medicines",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicines_UnitTypes_UnitTypeId",
                table: "Medicines",
                column: "UnitTypeId",
                principalTable: "UnitTypes",
                principalColumn: "UnitTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_Medicines_MedicineId",
                table: "PurchaseItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Medicines_MedicineId",
                table: "SaleItems",
                column: "MedicineId",
                principalTable: "Medicines",
                principalColumn: "MedicineId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
