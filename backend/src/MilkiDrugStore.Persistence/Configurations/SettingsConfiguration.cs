using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SettingsConfiguration : IEntityTypeConfiguration<Settings>
{
    public void Configure(EntityTypeBuilder<Settings> builder)
    {
        builder.HasKey(s => s.SettingId);
        builder.Property(s => s.PharmacyName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(300);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Language).HasMaxLength(20);
        builder.Property(s => s.Currency).HasMaxLength(10);
         builder.Property(s => s.BatchSelectionMode).IsRequired();
         builder.HasOne(s => s.Branch).WithOne(b => b.Settings).HasForeignKey<Settings>(s => s.BranchId);
     }
 }
