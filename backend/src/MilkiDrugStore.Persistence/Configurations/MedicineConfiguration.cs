using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.HasKey(m => m.ProductId);
        builder.Property(m => m.ProductCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.ProductCode).IsUnique();
        builder.Property(m => m.BrandName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.GenericName).HasMaxLength(150);
        builder.Property(m => m.Strength).HasMaxLength(50);
        builder.Property(m => m.DosageForm).HasMaxLength(100);
        builder.Property(m => m.Barcode).HasMaxLength(100);
        builder.HasIndex(m => m.Barcode).IsUnique();
        builder.Property(m => m.Manufacturer).HasMaxLength(150);
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(m => m.SellingPrice).HasColumnType("decimal(18,2)");
        builder.Property(m => m.CategoryId);
        builder.Property(m => m.UnitTypeId);
        builder.HasIndex(m => m.BrandName);
        builder.HasIndex(m => m.GenericName);
        builder.HasIndex(m => m.Strength);
        builder.HasIndex(m => m.DosageForm);
    }
}
