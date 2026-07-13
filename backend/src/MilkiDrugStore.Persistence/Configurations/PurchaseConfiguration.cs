using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasKey(p => p.PurchaseId);
        builder.Property(p => p.PurchaseNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.PurchaseNumber).IsUnique();
        builder.Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(p => p.AmountDue).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PaymentStatus).HasMaxLength(20).IsRequired();
        builder.Property(p => p.PaymentMethod).HasMaxLength(20);
        builder.HasOne(p => p.Supplier).WithMany(s => s.Purchases).HasForeignKey(p => p.SupplierId);
    }
}
