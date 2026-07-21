using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class MedicineBatchConfiguration : IEntityTypeConfiguration<MedicineBatch>
{
    public void Configure(EntityTypeBuilder<MedicineBatch> builder)
    {
        builder.HasKey(b => b.BatchId);
        builder.Property(b => b.BatchNumber).HasMaxLength(100);
        builder.Property(b => b.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.SellingPrice).HasColumnType("decimal(18,2)");
        builder.Property(b => b.Remarks).HasMaxLength(300);
        builder.HasOne(b => b.Medicine).WithMany(m => m.Batches).HasForeignKey(b => b.MedicineId);
        builder.HasOne(b => b.Branch).WithMany(br => br.MedicineBatches).HasForeignKey(b => b.BranchId);
        builder.HasIndex(b => b.ExpiryDate);
    }
}
