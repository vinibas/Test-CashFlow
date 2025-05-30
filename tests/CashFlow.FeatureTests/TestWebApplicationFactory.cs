using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CashFlow.FeatureTests;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config => 
        {
            config.AddInMemoryCollection(new Dictionary<string, string?> { { "Key", "Value" } });
        });

        builder.ConfigureServices(services =>
        {
        });

        return base.CreateHost(builder);
    }
}
