using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.HasKey(m => m.MedicineId);
        builder.Property(m => m.MedicineName).HasMaxLength(150).IsRequired();
        builder.Property(m => m.GenericName).HasMaxLength(150);
        builder.Property(m => m.UnitType).HasMaxLength(50);
        builder.HasOne(m => m.Category).WithMany(c => c.Medicines).HasForeignKey(m => m.CategoryId);
        builder.HasIndex(m => m.MedicineName);
    }
}
