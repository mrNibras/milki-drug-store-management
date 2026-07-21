using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.SaleId);
        builder.Property(s => s.SaleNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.SaleNumber).IsUnique();
        builder.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalProfit).HasColumnType("decimal(18,2)");
        builder.Property(s => s.TotalDiscount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.DiscountReason).HasMaxLength(500);
        builder.Property(s => s.PaymentMethod).HasMaxLength(20).IsRequired();
        builder.Property(s => s.PaymentStatus).HasMaxLength(20).IsRequired();
        builder.Property(s => s.AmountPaid).HasColumnType("decimal(18,2)");
        builder.Property(s => s.AmountDue).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ReferenceNumber).HasMaxLength(100);
        builder.HasOne(s => s.User).WithMany(u => u.Sales).HasForeignKey(s => s.UserId);
        builder.HasOne(s => s.Branch).WithMany(b => b.Sales).HasForeignKey(s => s.BranchId);
        builder.HasIndex(s => s.SaleDate);
    }
}
