using System.Reflection;
using CashFlow.Api.Data.Mappings;
using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Data;

public class CashFlowContext : DbContext, IUnitOfWork
{
    private readonly IConfiguration _configuration;

    public CashFlowContext(DbContextOptions<CashFlowContext> dbContextOptions, IConfiguration configuration)
        : base(dbContextOptions)
    {
        _configuration = configuration;
    }

    public DbSet<Entry> Entries { get; set; } = null!;
    public DbSet<DailyConsolidated> DailyConsolidated { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EntryMap());
        modelBuilder.ApplyConfiguration(new DailyConsolidatedMap());

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
    }

    public override int SaveChanges()
    {
        BlockDirectDailyConsolidatedWrites();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        BlockDirectDailyConsolidatedWrites();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void BlockDirectDailyConsolidatedWrites()
    {
        var inIntegrationTestContext = _configuration.GetValue<string>("InIntegrationTestContext");
        if (inIntegrationTestContext == "true")
            return;
        
        var entries = ChangeTracker.Entries<DailyConsolidated>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);
        if (entries.Any())
            throw new InvalidOperationException("Direct write operations on DailyConsolidated are not allowed. " +
                "Write operations are in update_and_get_daily_consolidated postgresql function.");
    }
    
    public async Task<bool> CommitAsync()
        => await SaveChangesAsync() > 0;
}
