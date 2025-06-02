using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Api.Data.Mappings;

public class DailyConsolidatedMap : IEntityTypeConfiguration<DailyConsolidated>
{
    public void Configure(EntityTypeBuilder<DailyConsolidated> builder)
    {
        builder.HasKey(dc => dc.Id);

        builder.Property<long>("LastLineNumberCalculated")
            .IsRequired();

        builder.HasOne<Entry>()
            .WithOne()
            .HasForeignKey<DailyConsolidated>("LastLineNumberCalculated")
            .HasPrincipalKey<Entry>("LineNumber");

        builder.HasIndex(dc => dc.Date)
            .HasDatabaseName("IX_DailyConsolidated_Date")
            .IsUnique();
    }
}
