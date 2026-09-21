using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MilkiDrugStore.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Cosmetics",
                columns: table => new
                {
                    CosmeticId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    UnitTypeId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cosmetics", x => x.CosmeticId);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrandName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    GenericName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Strength = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DosageForm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    UnitTypeId = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    SupplierId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypes",
                columns: table => new
                {
                    UnitTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypes", x => x.UnitTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NotificationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    PharmacyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    ExpiryAlertMonths = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BatchSelectionMode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_Settings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CosmeticBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CosmeticId = table.Column<int>(type: "integer", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    QuantityIssued = table.Column<int>(type: "integer", nullable: false),
                    QuantityDamaged = table.Column<int>(type: "integer", nullable: false),
                    QuantityExpired = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Remarks = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicineBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    QuantityReceived = table.Column<int>(type: "integer", nullable: false),
                    QuantityIssued = table.Column<int>(type: "integer", nullable: false),
                    QuantityDamaged = table.Column<int>(type: "integer", nullable: false),
                    QuantityExpired = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_MedicineBatches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicineBatches_Medicines_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Medicines",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicineBatches_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId");
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseId);
                    table.ForeignKey(
                        name: "FK_Purchases_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Purchases_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResets",
                columns: table => new
                {
                    PasswordResetId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResets", x => x.PasswordResetId);
                    table.ForeignKey(
                        name: "FK_PasswordResets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    RefreshTokenId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    SaleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalProfit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.SaleId);
                    table.ForeignKey(
                        name: "FK_Sales_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sales_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DamageRecords",
                columns: table => new
                {
                    DamageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RecordedBy = table.Column<int>(type: "integer", nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CosmeticBatchBatchId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageRecords", x => x.DamageId);
                    table.ForeignKey(
                        name: "FK_DamageRecords_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DamageRecords_CosmeticBatches_CosmeticBatchBatchId",
                        column: x => x.CosmeticBatchBatchId,
                        principalTable: "CosmeticBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_DamageRecords_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpiredRecords",
                columns: table => new
                {
                    ExpiredId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedBy = table.Column<int>(type: "integer", nullable: false),
                    CosmeticBatchBatchId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpiredRecords", x => x.ExpiredId);
                    table.ForeignKey(
                        name: "FK_ExpiredRecords_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpiredRecords_CosmeticBatches_CosmeticBatchBatchId",
                        column: x => x.CosmeticBatchBatchId,
                        principalTable: "CosmeticBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_ExpiredRecords_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "BatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    TransactionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CosmeticBatchBatchId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_CosmeticBatches_CosmeticBatchBatchId",
                        column: x => x.CosmeticBatchBatchId,
                        principalTable: "CosmeticBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Medicines_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Medicines",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItems",
                columns: table => new
                {
                    PurchaseItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PurchaseId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CosmeticBatchBatchId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseItems", x => x.PurchaseItemId);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_CosmeticBatches_CosmeticBatchBatchId",
                        column: x => x.CosmeticBatchBatchId,
                        principalTable: "CosmeticBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_PurchaseItems_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Medicines_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Medicines",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_Purchases_PurchaseId",
                        column: x => x.PurchaseId,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    SaleItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CosmeticBatchBatchId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.SaleItemId);
                    table.ForeignKey(
                        name: "FK_SaleItems_CosmeticBatches_CosmeticBatchBatchId",
                        column: x => x.CosmeticBatchBatchId,
                        principalTable: "CosmeticBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_SaleItems_MedicineBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "BatchId");
                    table.ForeignKey(
                        name: "FK_SaleItems_Medicines_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Medicines",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "SaleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_BranchId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_BatchNumber",
                table: "CosmeticBatches",
                column: "BatchNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CosmeticBatches_CosmeticId",
                table: "CosmeticBatches",
                column: "CosmeticId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_BatchId",
                table: "DamageRecords",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_BranchId",
                table: "DamageRecords",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageRecords_CosmeticBatchBatchId",
                table: "DamageRecords",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_BatchId",
                table: "ExpiredRecords",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_BranchId",
                table: "ExpiredRecords",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiredRecords_CosmeticBatchBatchId",
                table: "ExpiredRecords",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_BatchId",
                table: "InventoryTransactions",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CosmeticBatchBatchId",
                table: "InventoryTransactions",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CreatedAt",
                table: "InventoryTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId",
                table: "InventoryTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId_CreatedAt",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionType",
                table: "InventoryTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_UserId",
                table: "InventoryTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_BranchId_ExpiryDate",
                table: "MedicineBatches",
                columns: new[] { "BranchId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_ExpiryDate",
                table: "MedicineBatches",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_ProductId_BatchNumber",
                table: "MedicineBatches",
                columns: new[] { "ProductId", "BatchNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_ProductId_ExpiryDate",
                table: "MedicineBatches",
                columns: new[] { "ProductId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_SupplierId",
                table: "MedicineBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Barcode",
                table: "Medicines",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_BrandName",
                table: "Medicines",
                column: "BrandName");

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
                name: "IX_Notifications_BranchId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "BranchId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResets_UserId",
                table: "PasswordResets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_BatchId",
                table: "PurchaseItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_CosmeticBatchBatchId",
                table: "PurchaseItems",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_ProductId",
                table: "PurchaseItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_PurchaseId",
                table: "PurchaseItems",
                column: "PurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_PurchaseId_ProductId",
                table: "PurchaseItems",
                columns: new[] { "PurchaseId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_BranchId_PurchaseDate",
                table: "Purchases",
                columns: new[] { "BranchId", "PurchaseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PurchaseNumber",
                table: "Purchases",
                column: "PurchaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SupplierId",
                table: "Purchases",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_BatchId",
                table: "SaleItems",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_CosmeticBatchBatchId",
                table: "SaleItems",
                column: "CosmeticBatchBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId_ProductId",
                table: "SaleItems",
                columns: new[] { "SaleId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_BranchId",
                table: "Sales",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleDate",
                table: "Sales",
                column: "SaleDate");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleDate_BranchId",
                table: "Sales",
                columns: new[] { "SaleDate", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleNumber",
                table: "Sales",
                column: "SaleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_UserId_SaleDate",
                table: "Sales",
                columns: new[] { "UserId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_BranchId",
                table: "Settings",
                column: "BranchId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_Name",
                table: "UnitTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "DamageRecords");

            migrationBuilder.DropTable(
                name: "ExpiredRecords");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PasswordResets");

            migrationBuilder.DropTable(
                name: "PurchaseItems");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UnitTypes");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "CosmeticBatches");

            migrationBuilder.DropTable(
                name: "MedicineBatches");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.DropTable(
                name: "Cosmetics");

            migrationBuilder.DropTable(
                name: "Medicines");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
