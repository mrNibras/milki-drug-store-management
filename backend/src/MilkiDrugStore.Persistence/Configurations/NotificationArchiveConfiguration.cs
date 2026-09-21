using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class NotificationArchiveConfiguration : IEntityTypeConfiguration<NotificationArchive>
{
    public void Configure(EntityTypeBuilder<NotificationArchive> builder)
    {
        builder.HasKey(n => n.ArchiveId);
        builder.Property(n => n.OriginalNotificationId).IsRequired();
        builder.Property(n => n.BranchId).IsRequired();
        builder.Property(n => n.Title).IsRequired().HasMaxLength(150);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
        builder.Property(n => n.NotificationType).IsRequired().HasMaxLength(50);
        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.ArchivedAt).IsRequired();
        builder.Property(n => n.ArchivedBy).IsRequired();
        builder.HasIndex(n => n.OriginalNotificationId).IsUnique();
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.ArchivedAt);
        builder.HasIndex(n => new { n.BranchId, n.IsRead, n.ArchivedAt });
        builder.HasOne(n => n.Branch).WithMany().HasForeignKey(n => n.BranchId);
    }
}
