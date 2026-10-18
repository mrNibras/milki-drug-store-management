using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTokenFromPasswordResets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Invalidate any outstanding unexpired unused reset tokens before dropping plaintext column
            migrationBuilder.Sql(@"
                UPDATE ""PasswordResets""
                SET ""IsUsed"" = true
                WHERE ""IsUsed"" = false AND ""ExpiryDate"" > NOW();
            ");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "PasswordResets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "PasswordResets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
