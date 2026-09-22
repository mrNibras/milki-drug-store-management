using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.HasKey(b => b.BatchId);
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(b => b.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.SellingPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Remarks).HasMaxLength(300);
        builder.HasOne(b => b.Medicine).WithMany(m => m.Batches).HasForeignKey(b => b.ProductId);
        builder.HasOne(b => b.Branch).WithMany(br => br.MedicineBatches).HasForeignKey(b => b.BranchId);
        builder.HasOne(b => b.Supplier).WithMany(s => s.MedicineBatches).HasForeignKey(b => b.SupplierId);
        builder.HasIndex(b => b.ExpiryDate);
        builder.HasIndex(b => new { b.ProductId, b.BatchNumber });
        builder.HasIndex(b => new { b.ProductId, b.ExpiryDate });
        builder.HasIndex(b => new { b.BranchId, b.ExpiryDate });
        builder.HasIndex(b => b.SupplierId);
        builder.HasIndex(b => b.BatchNumber);
    }
}
