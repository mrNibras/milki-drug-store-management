using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class CosmeticBatchConfiguration : IEntityTypeConfiguration<CosmeticBatch>
{
    public void Configure(EntityTypeBuilder<CosmeticBatch> builder)
    {
        builder.ToTable("CosmeticBatches");
        builder.HasKey(b => b.BatchId);
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.QuantityReceived).IsRequired();
        builder.Property(b => b.QuantityIssued).IsRequired();
        builder.Property(b => b.QuantityDamaged).IsRequired();
        builder.Property(b => b.QuantityExpired).IsRequired();
        builder.Property(b => b.ExpiryDate);
        builder.Property(b => b.DateReceived).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(b => b.Remarks).HasMaxLength(300);

        builder.Property(b => b.BuyingPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.SellingPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.LowStockThreshold).HasDefaultValue(10);

        builder.Property(b => b.BranchId);
        builder.Property(b => b.SupplierId);

        builder.HasOne(b => b.Cosmetic)
            .WithMany(c => c.Batches)
            .HasForeignKey(b => b.CosmeticId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Branch)
            .WithMany()
            .HasForeignKey(b => b.BranchId);

        builder.HasOne(b => b.Supplier)
            .WithMany()
            .HasForeignKey(b => b.SupplierId);

        builder.HasIndex(b => b.CosmeticId);
        builder.HasIndex(b => b.BatchNumber);
        builder.HasIndex(b => b.BranchId);
        builder.HasIndex(b => b.SupplierId);
        builder.HasIndex(b => b.LowStockThreshold);
    }
}
