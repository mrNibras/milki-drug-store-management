using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SupplierPaymentItemConfiguration : IEntityTypeConfiguration<SupplierPaymentItem>
{
    public void Configure(EntityTypeBuilder<SupplierPaymentItem> builder)
    {
        builder.HasKey(ip => ip.SupplierPaymentItemId);

        builder.Property(ip => ip.AllocatedAmount).HasPrecision(18, 2);

        builder.HasIndex(ip => ip.PurchaseId);

        builder.HasOne(ip => ip.SupplierPayment)
            .WithMany(sp => sp.Items)
            .HasConstraintName("FK_SupplierPaymentItem_SupplierPayment")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ip => ip.Purchase)
            .WithMany(p => p.SupplierPaymentItems)
            .HasConstraintName("FK_SupplierPaymentItem_Purchase")
            .OnDelete(DeleteBehavior.NoAction);
    }
}