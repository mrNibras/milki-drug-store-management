using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class CosmeticConfiguration : IEntityTypeConfiguration<Cosmetic>
{
    public void Configure(EntityTypeBuilder<Cosmetic> builder)
    {
        builder.ToTable("Cosmetics");
        builder.HasKey(c => c.CosmeticId);
        builder.Property(c => c.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.CategoryId);
        builder.Property(c => c.UnitTypeId);
        builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.BranchId);
        builder.Property(c => c.SupplierId);

        builder.HasIndex(c => c.BranchId);
        builder.HasIndex(c => c.CategoryId);
        builder.HasIndex(c => c.SupplierId);
        builder.HasIndex(c => c.ProductName);
    }
}
