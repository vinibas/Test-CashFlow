using System.Data.Common;
using CashFlow.Api.Data;
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
            List<Type> typesToRemove = [
                typeof(DbContextOptions<CashFlowContext>),
                typeof(IDbContextOptionsConfiguration<CashFlowContext>),
                typeof(DbConnection),
            ];
            typesToRemove.ForEach(type => RemoveServiceDescriptor(services, type));

            services.AddDbContext<CashFlowContext>(options =>
                options.UseInMemoryDatabase("CashFlowDatabase"));
        });

        builder.UseEnvironment("Development");

        return base.CreateHost(builder);
    }

    private static void RemoveServiceDescriptor(IServiceCollection services, Type typeToRemove)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeToRemove);
        if (descriptor != null)
            services.Remove(descriptor);
    }
}
