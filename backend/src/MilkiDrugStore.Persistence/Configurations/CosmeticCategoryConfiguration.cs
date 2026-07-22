using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class CosmeticCategoryConfiguration : IEntityTypeConfiguration<CosmeticCategory>
{
    public void Configure(EntityTypeBuilder<CosmeticCategory> builder)
    {
        builder.ToTable("CosmeticCategories");
        builder.HasKey(c => c.CosmeticCategoryId);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Description).HasMaxLength(300);
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}
