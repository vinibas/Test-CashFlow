using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Api.Data.Mappings;

public class DailyConsolidatedMap : IEntityTypeConfiguration<DailyConsolidated>
{
    public void Configure(EntityTypeBuilder<DailyConsolidated> builder)
    {
        builder.HasKey(dc => dc.Id);

        builder.HasOne<Entry>()
            .WithOne()
            .HasForeignKey<DailyConsolidated>(dc => dc.LastLineNumberCalculated)
            .HasPrincipalKey<Entry>(e => e.LineNumber);

        builder.HasIndex(dc => dc.Date)
            .HasDatabaseName("IX_DailyConsolidated_Date")
            .IsUnique();
    }
}
