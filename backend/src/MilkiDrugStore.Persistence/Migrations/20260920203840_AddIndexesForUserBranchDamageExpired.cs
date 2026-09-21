using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForUserBranchDamageExpired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_BranchId_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.CreateTable(
                name: "AuditLogArchives",
                columns: table => new
                {
                    ArchiveId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalAuditId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogArchives", x => x.ArchiveId);
                    table.ForeignKey(
                        name: "FK_AuditLogArchives_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_AuditLogArchives_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationArchives",
                columns: table => new
                {
                    ArchiveId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginalNotificationId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationArchives", x => x.ArchiveId);
                    table.ForeignKey(
                        name: "FK_NotificationArchives_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsApproved",
                table: "Users",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_IsActive",
                table: "UnitTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Email",
                table: "Suppliers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Phone",
                table: "Suppliers",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierName",
                table: "Suppliers",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_PaymentStatus",
                table: "Sales",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PaymentStatus",
                table: "Purchases",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PurchaseDate",
                table: "Purchases",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationType",
                table: "Notifications",
                column: "NotificationType");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_CategoryId",
                table: "Medicines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_IsActive",
                table: "Medicines",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_UnitTypeId",
                table: "Medicines",
                column: "UnitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_BatchNumber",
                table: "MedicineBatches",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ReferenceId",
                table: "InventoryTransactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_RecordedBy",
                table: "ExpiredRecords",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_RecordedDate",
                table: "ExpiredRecords",
                column: "RecordedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_RecordedBy",
                table: "DamageRecords",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_RecordedDate",
                table: "DamageRecords",
                column: "RecordedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive",
                table: "Categories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_IsActive",
                table: "Branches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_BranchId",
                table: "AuditLogs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName_RecordId",
                table: "AuditLogs",
                columns: new[] { "TableName", "RecordId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogArchives_ArchivedAt",
                table: "AuditLogArchives",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogArchives_BranchId",
                table: "AuditLogArchives",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogArchives_CreatedAt",
                table: "AuditLogArchives",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogArchives_OriginalAuditId",
                table: "AuditLogArchives",
                column: "OriginalAuditId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogArchives_UserId",
                table: "AuditLogArchives",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationArchives_ArchivedAt",
                table: "NotificationArchives",
                column: "ArchivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationArchives_BranchId_IsRead_ArchivedAt",
                table: "NotificationArchives",
                columns: new[] { "BranchId", "IsRead", "ArchivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationArchives_CreatedAt",
                table: "NotificationArchives",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationArchives_OriginalNotificationId",
                table: "NotificationArchives",
                column: "OriginalNotificationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogArchives");

            migrationBuilder.DropTable(
                name: "NotificationArchives");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsApproved",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UnitTypes_IsActive",
                table: "UnitTypes");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Email",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Phone",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_SupplierName",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Sales_PaymentStatus",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_PaymentStatus",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_PurchaseDate",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_NotificationType",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_CategoryId",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_IsActive",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_UnitTypeId",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_MedicineBatches_BatchNumber",
                table: "MedicineBatches");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_ReferenceId",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ExpiredRecords_RecordedBy",
                table: "ExpiredRecords");

            migrationBuilder.DropIndex(
                name: "IX_ExpiredRecords_RecordedDate",
                table: "ExpiredRecords");

            migrationBuilder.DropIndex(
                name: "IX_DamageRecords_RecordedBy",
                table: "DamageRecords");

            migrationBuilder.DropIndex(
                name: "IX_DamageRecords_RecordedDate",
                table: "DamageRecords");

            migrationBuilder.DropIndex(
                name: "IX_Categories_IsActive",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Branches_IsActive",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_BranchId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TableName_RecordId",
                table: "AuditLogs");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_BranchId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "BranchId", "CreatedAt" });
        }
    }
}
