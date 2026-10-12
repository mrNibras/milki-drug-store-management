using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SupplierPaymentConfiguration : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.HasKey(sp => sp.SupplierPaymentId);
        builder.Property(sp => sp.Amount).HasPrecision(18, 2);
        builder.Property(sp => sp.PaymentMethod).HasPrecision(20).IsRequired();
        builder.Property(sp => sp.ReferenceNumber).HasPrecision(100);
        builder.Property(sp => sp.Notes).HasPrecision(500);
        builder.HasIndex(sp => sp.SupplierId);
        builder.HasIndex(sp => sp.PaymentDate);

        builder.HasOne(sp => sp.Supplier)
            .WithMany(s => s.SupplierPayments)
            .HasConstraintName("FK_SupplierPayment_Supplier")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(sp => sp.Branch)
            .WithMany(b => b.SupplierPayments)
            .HasConstraintName("FK_SupplierPayment_Branch")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(sp => sp.Items)
            .WithOne(ip => ip.SupplierPayment)
            .HasConstraintName("FK_SupplierPaymentItem_SupplierPayment")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
