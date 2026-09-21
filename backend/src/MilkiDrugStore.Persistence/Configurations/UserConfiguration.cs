using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.FullName).HasMaxLength(100);
        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);
        builder.HasOne(u => u.Branch).WithMany(b => b.Users).HasForeignKey(u => u.BranchId);
        builder.HasIndex(u => u.RoleId);
        builder.HasIndex(u => u.BranchId);
        builder.HasIndex(u => u.IsApproved);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.CreatedAt);
    }
}
