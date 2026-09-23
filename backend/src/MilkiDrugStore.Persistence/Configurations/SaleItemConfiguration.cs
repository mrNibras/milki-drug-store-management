using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.HasKey(si => si.SaleItemId);
        builder.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(si => si.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(si => si.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(si => si.Profit).HasColumnType("decimal(18,2)");
        builder.Property(si => si.SubTotal).HasColumnType("decimal(18,2)");
        builder.HasOne(si => si.Sale).WithMany(s => s.Items).HasForeignKey(si => si.SaleId);
        builder.HasOne(si => si.Medicine).WithMany(m => m.SaleItems).HasForeignKey(si => si.ProductId).IsRequired(false);
        builder.HasOne(si => si.Cosmetic).WithMany(c => c.SaleItems).HasForeignKey(si => si.CosmeticId);

        builder.HasIndex(si => si.SaleId);
        builder.HasIndex(si => si.ProductId);
        builder.HasIndex(si => si.BatchId);
        builder.HasIndex(si => si.CosmeticId);
        builder.HasIndex(si => si.CosmeticBatchId);
        builder.HasIndex(si => new { si.SaleId, si.ProductId });
    }
}
