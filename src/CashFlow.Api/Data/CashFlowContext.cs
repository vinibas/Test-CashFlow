using System.Reflection;
using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Data;

public class CashFlowContext : DbContext, IUnitOfWork
{
    public CashFlowContext(DbContextOptions<CashFlowContext> dbContextOptions)
        : base(dbContextOptions)
    { }

    public DbSet<Entry> Entries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
    }
    
    public async Task<bool> CommitAsync()
        => await SaveChangesAsync() > 0;
}
