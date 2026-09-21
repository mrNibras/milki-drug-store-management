using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.Title).HasMaxLength(150).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(500);
        builder.Property(n => n.NotificationType).HasMaxLength(50);
        builder.HasOne(n => n.Branch).WithMany(b => b.Notifications).HasForeignKey(n => n.BranchId);
        builder.HasIndex(n => new { n.BranchId, n.IsRead, n.CreatedAt });
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n => n.NotificationType);
    }
}
