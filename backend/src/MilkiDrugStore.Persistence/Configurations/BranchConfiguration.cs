using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.BranchId);
        builder.Property(b => b.BranchName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Location).HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(50);
        builder.Property(b => b.Email).HasMaxLength(150);
        builder.Property(b => b.Address).HasMaxLength(300);
        builder.Property(b => b.IsActive).HasDefaultValue(true);
        builder.Property(b => b.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}
