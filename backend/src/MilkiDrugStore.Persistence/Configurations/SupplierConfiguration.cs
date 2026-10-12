using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.SupplierId);
        builder.Property(s => s.SupplierName).HasPrecision(150).IsRequired();
        builder.Property(s => s.Phone).HasPrecision(50);
        builder.Property(s => s.Email).HasPrecision(100);
        builder.Property(s => s.Address).HasPrecision(255);
        builder.Property(s => s.ContactPerson).HasPrecision(100);
        builder.Property(s => s.PaymentStatus).HasPrecision(50);
        builder.HasIndex(s => s.SupplierName);
        builder.HasIndex(s => s.Phone);
        builder.HasIndex(s => s.Email);
    }
}
