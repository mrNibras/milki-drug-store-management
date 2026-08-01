using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Persistence.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.HasKey(si => si.SaleItemId);
        builder.Property(si => si.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(si => si.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(si => si.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(si => si.Profit).HasColumnType("decimal(18,2)");
        builder.Property(si => si.SubTotal).HasColumnType("decimal(18,2)");
        builder.HasOne(si => si.Sale).WithMany(s => s.Items).HasForeignKey(si => si.SaleId);
        builder.HasOne(si => si.Medicine).WithMany(m => m.SaleItems).HasForeignKey(si => si.ProductId);
    }
}
