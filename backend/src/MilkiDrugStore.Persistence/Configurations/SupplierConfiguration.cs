using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.SupplierId);
        builder.Property(s => s.SupplierName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(100);
        builder.Property(s => s.Address).HasMaxLength(255);
        builder.Property(s => s.PaymentStatus).HasMaxLength(50);
    }
}
