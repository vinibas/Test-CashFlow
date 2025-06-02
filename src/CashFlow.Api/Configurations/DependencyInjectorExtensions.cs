using CashFlow.Api.Data;
using CashFlow.Api.Data.Daos;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Configurations;

internal static class DependencyInjectorExtensions
{
    internal static void RegisterServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var conString = configuration.GetConnectionString("CashFlowDatabase") ??
            throw new InvalidOperationException("Connection string 'CashFlowDatabase' not found.");

        var inIntegrationTestContext = configuration.GetValue<string>("InIntegrationTestContext");

        if (inIntegrationTestContext != "true")
            services.AddDbContext<CashFlowContext>(options =>
                options
                    .UseNpgsql(conString)
                    .EnableSensitiveDataLogging(isDevelopment));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CashFlowContext>());

        services.AddScoped<IEntryDao, EntryDao>();
        services.AddScoped<IDailyConsolidatedDao, DailyConsolidatedDao>();
    }
}
