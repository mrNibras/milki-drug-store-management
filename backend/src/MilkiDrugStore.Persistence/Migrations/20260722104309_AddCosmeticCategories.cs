using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCosmeticCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_Categories_CategoryId",
                table: "Cosmetics");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Cosmetics",
                newName: "CosmeticCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Cosmetics_CategoryId",
                table: "Cosmetics",
                newName: "IX_Cosmetics_CosmeticCategoryId");

            migrationBuilder.CreateTable(
                name: "CosmeticCategories",
                columns: table => new
                {
                    CosmeticCategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CosmeticCategories", x => x.CosmeticCategoryId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_CosmeticCategories_CosmeticCategoryId",
                table: "Cosmetics",
                column: "CosmeticCategoryId",
                principalTable: "CosmeticCategories",
                principalColumn: "CosmeticCategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cosmetics_CosmeticCategories_CosmeticCategoryId",
                table: "Cosmetics");

            migrationBuilder.DropTable(
                name: "CosmeticCategories");

            migrationBuilder.RenameColumn(
                name: "CosmeticCategoryId",
                table: "Cosmetics",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Cosmetics_CosmeticCategoryId",
                table: "Cosmetics",
                newName: "IX_Cosmetics_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cosmetics_Categories_CategoryId",
                table: "Cosmetics",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
