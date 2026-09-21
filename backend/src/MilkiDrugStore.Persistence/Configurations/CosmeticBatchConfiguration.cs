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
        builder.Property(b => b.ExpiryDate).IsRequired();
        builder.Property(b => b.DateReceived).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(b => b.Remarks).HasMaxLength(300);

        builder.HasOne(b => b.Cosmetic)
            .WithMany(c => c.Batches)
            .HasForeignKey(b => b.CosmeticId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.CosmeticId);
        builder.HasIndex(b => b.BatchNumber);
    }
}
