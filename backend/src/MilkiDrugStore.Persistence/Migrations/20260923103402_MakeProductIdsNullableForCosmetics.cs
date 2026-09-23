using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeProductIdsNullableForCosmetics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Medicines_ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Medicines_ProductId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Medicines_ProductId",
                table: "SaleItems");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InventoryTransactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "SaleItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "PurchaseItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Medicines_ProductId",
                table: "InventoryTransactions",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseItems_Medicines_ProductId",
                table: "PurchaseItems",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_Medicines_ProductId",
                table: "SaleItems",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Medicines_ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseItems_Medicines_ProductId",
                table: "PurchaseItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_Medicines_ProductId",
                table: "SaleItems");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InventoryTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "SaleItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "PurchaseItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Medicines_ProductId",
                table: "InventoryTransactions",
                column: "ProductId",
                principalTable: "Medicines",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

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
    }
}
