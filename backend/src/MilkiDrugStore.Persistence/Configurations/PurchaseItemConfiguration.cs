using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class PurchaseItemConfiguration : IEntityTypeConfiguration<PurchaseItem>
{
    public void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        builder.HasKey(pi => pi.PurchaseItemId);
        builder.Property(pi => pi.BatchNumber).HasMaxLength(100);
        builder.Property(pi => pi.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(pi => pi.SubTotal).HasColumnType("decimal(18,2)");
        builder.HasOne(pi => pi.Purchase).WithMany(p => p.Items).HasForeignKey(pi => pi.PurchaseId);
        builder.HasOne(pi => pi.Medicine).WithMany().HasForeignKey(pi => pi.MedicineId);
    }
}
