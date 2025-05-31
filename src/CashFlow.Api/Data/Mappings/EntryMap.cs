using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Api.Data.Mappings;

public class EntryMap : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .HasMaxLength(250)
            .IsUnicode(false);

        builder.Property(e => e.Type)
            .HasConversion(t => (char)t, t => (EntryType)t);


        builder.HasIndex(e => e.CreatedAtUtc)
            .HasDatabaseName("IX_Entry_CreatedAtUtc");
    }
}
