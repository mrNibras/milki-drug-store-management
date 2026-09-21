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
        builder.HasOne(al => al.Branch).WithMany(b => b.AuditLogs).HasForeignKey(al => al.BranchId);
        builder.HasIndex(al => new { al.UserId, al.CreatedAt });
        builder.HasIndex(al => al.CreatedAt);
        builder.HasIndex(al => new { al.TableName, al.RecordId });
    }
}
