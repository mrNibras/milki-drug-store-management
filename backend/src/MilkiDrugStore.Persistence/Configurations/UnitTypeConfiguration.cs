using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder.HasKey(u => u.UnitTypeId);
        builder.Property(u => u.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Name).IsUnique();
        builder.HasIndex(u => u.IsActive);
    }
}
