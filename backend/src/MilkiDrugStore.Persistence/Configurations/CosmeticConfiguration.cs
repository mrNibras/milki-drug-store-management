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
        builder.Property(c => c.Price).HasColumnType("decimal(18,2)");
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(c => c.CosmeticCategory)
            .WithMany(cc => cc.Cosmetics)
            .HasForeignKey(c => c.CosmeticCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.UnitType)
            .WithMany()
            .HasForeignKey(c => c.UnitTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
