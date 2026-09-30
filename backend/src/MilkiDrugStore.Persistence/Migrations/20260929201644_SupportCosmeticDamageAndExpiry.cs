using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SupportCosmeticDamageAndExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "DamageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_MedicineBatches_BatchId",
                table: "DamageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpiredRecords_MedicineBatches_BatchId",
                table: "ExpiredRecords");

            migrationBuilder.RenameColumn(
                name: "CosmeticBatchBatchId",
                table: "ExpiredRecords",
                newName: "CosmeticId");

            migrationBuilder.RenameIndex(
                name: "IX_ExpiredRecords_CosmeticBatchBatchId",
                table: "ExpiredRecords",
                newName: "IX_ExpiredRecords_CosmeticId");

            migrationBuilder.RenameColumn(
                name: "CosmeticBatchBatchId",
                table: "DamageRecords",
                newName: "CosmeticId");

            migrationBuilder.RenameIndex(
                name: "IX_DamageRecords_CosmeticBatchBatchId",
                table: "DamageRecords",
                newName: "IX_DamageRecords_CosmeticId");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "ExpiredRecords",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchId",
                table: "ExpiredRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "DamageRecords",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "CosmeticBatchId",
                table: "DamageRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_CosmeticBatchId",
                table: "ExpiredRecords",
                column: "CosmeticBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_CosmeticBatchId",
                table: "DamageRecords",
                column: "CosmeticBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchId",
                table: "DamageRecords",
                column: "CosmeticBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_Cosmetics_CosmeticId",
                table: "DamageRecords",
                column: "CosmeticId",
                principalTable: "Cosmetics",
                principalColumn: "CosmeticId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_MedicineBatches_BatchId",
                table: "DamageRecords",
                column: "BatchId",
                principalTable: "MedicineBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchId",
                table: "ExpiredRecords",
                column: "CosmeticBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpiredRecords_Cosmetics_CosmeticId",
                table: "ExpiredRecords",
                column: "CosmeticId",
                principalTable: "Cosmetics",
                principalColumn: "CosmeticId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpiredRecords_MedicineBatches_BatchId",
                table: "ExpiredRecords",
                column: "BatchId",
                principalTable: "MedicineBatches",
                principalColumn: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchId",
                table: "DamageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_Cosmetics_CosmeticId",
                table: "DamageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_DamageRecords_MedicineBatches_BatchId",
                table: "DamageRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpiredRecords_Cosmetics_CosmeticId",
                table: "ExpiredRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpiredRecords_MedicineBatches_BatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExpiredRecords_CosmeticBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropIndex(
                name: "IX_DamageRecords_CosmeticBatchId",
                table: "DamageRecords");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchId",
                table: "ExpiredRecords");

            migrationBuilder.DropColumn(
                name: "CosmeticBatchId",
                table: "DamageRecords");

            migrationBuilder.RenameColumn(
                name: "CosmeticId",
                table: "ExpiredRecords",
                newName: "CosmeticBatchBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_ExpiredRecords_CosmeticId",
                table: "ExpiredRecords",
                newName: "IX_ExpiredRecords_CosmeticBatchBatchId");

            migrationBuilder.RenameColumn(
                name: "CosmeticId",
                table: "DamageRecords",
                newName: "CosmeticBatchBatchId");

            migrationBuilder.RenameIndex(
                name: "IX_DamageRecords_CosmeticId",
                table: "DamageRecords",
                newName: "IX_DamageRecords_CosmeticBatchBatchId");

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "ExpiredRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BatchId",
                table: "DamageRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "DamageRecords",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_DamageRecords_MedicineBatches_BatchId",
                table: "DamageRecords",
                column: "BatchId",
                principalTable: "MedicineBatches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchBatchId",
                table: "ExpiredRecords",
                column: "CosmeticBatchBatchId",
                principalTable: "CosmeticBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpiredRecords_MedicineBatches_BatchId",
                table: "ExpiredRecords",
                column: "BatchId",
                principalTable: "MedicineBatches",
                principalColumn: "BatchId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
