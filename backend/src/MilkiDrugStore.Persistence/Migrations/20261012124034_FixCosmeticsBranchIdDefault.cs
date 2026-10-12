using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCosmeticsBranchIdDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the FK constraints first
            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_Branches_BranchId",
                table: "Cosmetics");

            migrationBuilder.DropForeignKey(
                name: "FK_CosmeticBatches_Branches_BranchId",
                table: "CosmeticBatches");

            // Ensure a Main Branch exists (ID will be 1 if table is empty)
            // This handles the case where migrations run before the seeder
            migrationBuilder.Sql(@"
                INSERT INTO ""Branches"" (""BranchName"", ""Location"", ""IsActive"", ""CreatedAt"")
                SELECT 'Main Branch', 'Main Store', true, CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM ""Branches"" WHERE ""BranchName"" = 'Main Branch');
            ");

            // Update Cosmetics with BranchId = 0 to use the Main Branch
            migrationBuilder.Sql(@"
                UPDATE ""Cosmetics"" 
                SET ""BranchId"" = (SELECT ""BranchId"" FROM ""Branches"" WHERE ""BranchName"" = 'Main Branch' LIMIT 1)
                WHERE ""BranchId"" = 0;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""CosmeticBatches"" 
                SET ""BranchId"" = (SELECT ""BranchId"" FROM ""Branches"" WHERE ""BranchName"" = 'Main Branch' LIMIT 1)
                WHERE ""BranchId"" = 0;
            ");

            // Re-add the FK constraints
            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_Branches_BranchId",
                table: "Cosmetics",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CosmeticBatches_Branches_BranchId",
                table: "CosmeticBatches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the FK constraints
            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_Branches_BranchId",
                table: "Cosmetics");

            migrationBuilder.DropForeignKey(
                name: "FK_CosmeticBatches_Branches_BranchId",
                table: "CosmeticBatches");

            // Revert BranchId to 0 for records that were updated (best effort)
            migrationBuilder.Sql(@"
                UPDATE ""Cosmetics"" 
                SET ""BranchId"" = 0 
                WHERE ""BranchId"" = (SELECT ""BranchId"" FROM ""Branches"" WHERE ""BranchName"" = 'Main Branch' LIMIT 1);
            ");

            migrationBuilder.Sql(@"
                UPDATE ""CosmeticBatches"" 
                SET ""BranchId"" = 0 
                WHERE ""BranchId"" = (SELECT ""BranchId"" FROM ""Branches"" WHERE ""BranchName"" = 'Main Branch' LIMIT 1);
            ");

            // Re-add the FK constraints
            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_Branches_BranchId",
                table: "Cosmetics",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CosmeticBatches_Branches_BranchId",
                table: "CosmeticBatches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
