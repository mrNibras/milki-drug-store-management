using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class ExpiredRecordConfiguration : IEntityTypeConfiguration<ExpiredRecord>
{
    public void Configure(EntityTypeBuilder<ExpiredRecord> builder)
    {
        builder.HasKey(e => e.ExpiredId);
        builder.HasOne(e => e.Batch).WithMany(b => b.ExpiredRecords).HasForeignKey(e => e.BatchId);
        builder.HasOne(e => e.Branch).WithMany(b => b.ExpiredRecords).HasForeignKey(e => e.BranchId);
        builder.HasIndex(e => e.BatchId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordedDate);
        builder.HasIndex(e => e.RecordedBy);
    }
}
