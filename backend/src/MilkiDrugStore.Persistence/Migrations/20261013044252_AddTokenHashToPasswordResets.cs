using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Cryptography;
using System.Text;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenHashToPasswordResets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add TokenHash column as nullable
            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "PasswordResets",
                type: "text",
                nullable: true);

            // Step 2: Populate TokenHash from existing Token values using SHA-256
            migrationBuilder.Sql(@"
                UPDATE ""PasswordResets""
                SET ""TokenHash"" = encode(sha256(""Token""::bytea), 'hex')
                WHERE ""Token"" IS NOT NULL AND ""Token"" <> '';
            ");

            // Step 3: Make TokenHash non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "TokenHash",
                table: "PasswordResets",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "PasswordResets");
        }
    }
}
