using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.HasKey(it => it.TransactionId);
        builder.Property(it => it.TransactionType).HasMaxLength(20).IsRequired();
        builder.Property(it => it.ReferenceType).HasMaxLength(50);
        builder.Property(it => it.UnitPrice).HasColumnType("decimal(18,2)");
        builder.HasOne(it => it.Medicine).WithMany(m => m.InventoryTransactions).HasForeignKey(it => it.MedicineId);
        builder.HasOne(it => it.Batch).WithMany().HasForeignKey(it => it.BatchId);
        builder.HasIndex(it => it.MedicineId);
        builder.HasIndex(it => it.TransactionType);
    }
}
