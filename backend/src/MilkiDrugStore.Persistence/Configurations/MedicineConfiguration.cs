 using Microsoft.EntityFrameworkCore;
 using Microsoft.EntityFrameworkCore.Metadata.Builders;
 using MilkiDrugStore.Domain.Entities;

 namespace MilkiDrugStore.Persistence.Configurations;

 public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
 {
     public void Configure(EntityTypeBuilder<Medicine> builder)
     {
         builder.HasKey(m => m.MedicineId);
          builder.Property(m => m.BrandName).HasMaxLength(150).IsRequired();
         builder.Property(m => m.GenericName).HasMaxLength(150);
         builder.HasOne(m => m.Category).WithMany(c => c.Medicines).HasForeignKey(m => m.CategoryId);
         builder.HasOne(m => m.UnitType).WithMany(u => u.Medicines).HasForeignKey(m => m.UnitTypeId);
          builder.HasIndex(m => m.BrandName);
     }
 }
