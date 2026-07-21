using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class DamageRecordConfiguration : IEntityTypeConfiguration<DamageRecord>
{
    public void Configure(EntityTypeBuilder<DamageRecord> builder)
    {
        builder.HasKey(d => d.DamageId);
        builder.Property(d => d.Reason).HasMaxLength(300);
        builder.HasOne(d => d.Batch).WithMany(b => b.DamageRecords).HasForeignKey(d => d.BatchId);
        builder.HasOne(d => d.Branch).WithMany(b => b.DamageRecords).HasForeignKey(d => d.BranchId);
    }
}
