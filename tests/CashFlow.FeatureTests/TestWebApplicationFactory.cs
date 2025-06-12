using System.Data.Common;
using CashFlow.Api.Data;
using CashFlow.Api.Data.Daos;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CashFlow.FeatureTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?> { { "InIntegrationTestContext", "true" } }));

        builder.ConfigureServices((_, services) =>
        {
            OverrideContext(services);
            OverrideDailyConsolidatedDao(services);
        });

        builder.UseEnvironment("Development");

        return base.CreateHost(builder);
    }

    private static void OverrideContext(IServiceCollection services)
    {
        List<Type> typesToRemove = [
            typeof(DbContextOptions<CashFlowContext>),
            typeof(IDbContextOptionsConfiguration<CashFlowContext>),
            typeof(DbConnection),
        ];
        typesToRemove.ForEach(type => RemoveServiceDescriptor(services, type));

        services.AddDbContext<CashFlowContext>(options =>
            options.UseInMemoryDatabase("CashFlowDatabase"));
    }

    private static void OverrideDailyConsolidatedDao(IServiceCollection services)
    {
        RemoveServiceDescriptor(services, typeof(IDailyConsolidatedDao));
        services.AddScoped<IDailyConsolidatedDao, DailyConsolidatedDaoForTest>();
    }

    private static void RemoveServiceDescriptor(IServiceCollection services, Type typeToRemove)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeToRemove);
        if (descriptor != null)
            services.Remove(descriptor);
    }

    class DailyConsolidatedDaoForTest : DailyConsolidatedDao
    {
        private CashFlowContext _context;
        public DailyConsolidatedDaoForTest(CashFlowContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<DailyConsolidated?> GetConsolidatedUpdatedAsync(DateOnly date)
            => await _context.DailyConsolidated.SingleOrDefaultAsync(dc => dc.Date == date);
    }

    public void CleanDatabase()
    {
        using var scope = Services.CreateScope();
        var cx = scope.ServiceProvider.GetRequiredService<CashFlowContext>();
        cx.Entries.RemoveRange(cx.Entries);
        cx.DailyConsolidated.RemoveRange(cx.DailyConsolidated);
        cx.SaveChangesAsync();
    }

}
