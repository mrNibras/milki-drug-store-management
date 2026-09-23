using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCosmeticBatchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_CosmeticBatches_CosmeticBatchBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_CosmeticBatches_CosmeticBatchBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_CosmeticBatches_CosmeticBatchBatchId",
                table: "SaleItems");

            migrationBuilder.RenameColumn(
                name: "CosmeticBatchBatchId",
                table: "SaleItems",
                newName: "CosmeticId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_CosmeticBatchBatchId",
                table: "SaleItems",
                newName: "IX_SaleItems_CosmeticId");

            migrationBuilder.RenameColumn(
                name: "CosmeticBatchBatchId",
                table: "PurchaseItems",
                newName: "CosmeticId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseItems_CosmeticBatchBatchId",
                table: "PurchaseItems",
                newName: "IX_PurchaseItems_CosmeticId");

            migrationBuilder.RenameColumn(
                name: "CosmeticBatchBatchId",
                table: "InventoryTransactions",
                newName: "CosmeticId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_CosmeticBatchBatchId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_CosmeticId");

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchId",
                table: "SaleItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchId",
                table: "PurchaseItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchId",
                table: "InventoryTransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Cosmetics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Cosmetics",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Cosmetics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CosmeticBatches",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "CosmeticBatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "BuyingPrice",
                table: "CosmeticBatches",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "CosmeticBatches",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "CosmeticBatches",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "CosmeticBatches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_CosmeticBatchId",
                table: "SaleItems",
                column: "CosmeticBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_CosmeticBatchId",
                table: "PurchaseItems",
                column: "CosmeticBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CosmeticBatchId",
                table: "InventoryTransactions",
                column: "CosmeticBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_BranchId",
                table: "Cosmetics",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_CategoryId",
                table: "Cosmetics",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_ProductName",
                table: "Cosmetics",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_Cosmetics_SupplierId",
                table: "Cosmetics",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_BranchId",
                table: "CosmeticBatches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_LowStockThreshold",
                table: "CosmeticBatches",
                column: "LowStockThreshold");

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_SupplierId",
                table: "CosmeticBatches",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_CosmeticBatches_Branches_BranchId",
                table: "CosmeticBatches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CosmeticBatches_Suppliers_SupplierId",
                table: "CosmeticBatches",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_Branches_BranchId",
                table: "Cosmetics",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_Suppliers_SupplierId",
                table: "Cosmetics",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_CosmeticBatches_CosmeticBatchId",
                table: "InventoryTransactions",
                column: "CosmeticBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Cosmetics_CosmeticId",
                table: "InventoryTransactions",
                column: "CosmeticId",
                principalTable: "Cosmetics",
                principalColumn: "CosmeticId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_CosmeticBatches_CosmeticBatchId",
                table: "PurchaseItems",
                column: "CosmeticBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_Cosmetics_CosmeticId",
                table: "PurchaseItems",
                column: "CosmeticId",
                principalTable: "Cosmetics",
                principalColumn: "CosmeticId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_CosmeticBatches_CosmeticBatchId",
                table: "SaleItems",
                column: "CosmeticBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Cosmetics_CosmeticId",
                table: "SaleItems",
                column: "CosmeticId",
                principalTable: "Cosmetics",
                principalColumn: "CosmeticId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CosmeticBatches_Branches_BranchId",
                table: "CosmeticBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_CosmeticBatches_Suppliers_SupplierId",
                table: "CosmeticBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_Branches_BranchId",
                table: "Cosmetics");

            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_Suppliers_SupplierId",
                table: "Cosmetics");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_CosmeticBatches_CosmeticBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Cosmetics_CosmeticId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_CosmeticBatches_CosmeticBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Cosmetics_CosmeticId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_CosmeticBatches_CosmeticBatchId",
                table: "SaleItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Cosmetics_CosmeticId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_SaleItems_CosmeticBatchId",
                table: "SaleItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseItems_CosmeticBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_CosmeticBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Cosmetics_BranchId",
                table: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_Cosmetics_CategoryId",
                table: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_Cosmetics_ProductName",
                table: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_Cosmetics_SupplierId",
                table: "Cosmetics");

            migrationBuilder.DropIndex(
                name: "IX_CosmeticBatches_BranchId",
                table: "CosmeticBatches");

            migrationBuilder.DropIndex(
                name: "IX_CosmeticBatches_LowStockThreshold",
                table: "CosmeticBatches");

            migrationBuilder.DropIndex(
                name: "IX_CosmeticBatches_SupplierId",
                table: "CosmeticBatches");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchId",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchId",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Cosmetics");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Cosmetics");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Cosmetics");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "CosmeticBatches");

            migrationBuilder.DropColumn(
                name: "BuyingPrice",
                table: "CosmeticBatches");

            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "CosmeticBatches");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "CosmeticBatches");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "CosmeticBatches");

            migrationBuilder.RenameColumn(
                name: "CosmeticId",
                table: "SaleItems",
                newName: "CosmeticBatchBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_CosmeticId",
                table: "SaleItems",
                newName: "IX_SaleItems_CosmeticBatchBatchId");

            migrationBuilder.RenameColumn(
                name: "CosmeticId",
                table: "PurchaseItems",
                newName: "CosmeticBatchBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseItems_CosmeticId",
                table: "PurchaseItems",
                newName: "IX_PurchaseItems_CosmeticBatchBatchId");

            migrationBuilder.RenameColumn(
                name: "CosmeticId",
                table: "InventoryTransactions",
                newName: "CosmeticBatchBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryTransactions_CosmeticId",
                table: "InventoryTransactions",
                newName: "IX_InventoryTransactions_CosmeticBatchBatchId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "CosmeticBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

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
    }
}
