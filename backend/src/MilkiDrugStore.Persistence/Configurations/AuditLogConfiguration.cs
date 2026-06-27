using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(al => al.AuditId);
        builder.Property(al => al.Action).HasMaxLength(200).IsRequired();
        builder.Property(al => al.TableName).HasMaxLength(100);
        builder.HasOne(al => al.User).WithMany(u => u.AuditLogs).HasForeignKey(al => al.UserId);
    }
}
