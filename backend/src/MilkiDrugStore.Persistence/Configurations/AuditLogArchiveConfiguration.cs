using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class AuditLogArchiveConfiguration : IEntityTypeConfiguration<AuditLogArchive>
{
    public void Configure(EntityTypeBuilder<AuditLogArchive> builder)
    {
        builder.HasKey(a => a.ArchiveId);
        builder.Property(a => a.OriginalAuditId).IsRequired();
        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.Action).IsRequired().HasMaxLength(200);
        builder.Property(a => a.TableName).HasMaxLength(100);
        builder.Property(a => a.ArchivedAt).IsRequired();
        builder.Property(a => a.ArchivedBy).IsRequired();
        builder.HasIndex(a => a.OriginalAuditId).IsUnique();
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.ArchivedAt);
        builder.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId);
        builder.HasOne(a => a.Branch).WithMany().HasForeignKey(a => a.BranchId);
    }
}
