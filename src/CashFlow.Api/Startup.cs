using CashFlow.Api.Endpoints;
using HealthChecks.UI.Client;
using Scalar.AspNetCore;

namespace CashFlow.Api;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{

    public IConfiguration Configuration { get; } = configuration;
    public IWebHostEnvironment Environment { get; } = environment;
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks();
        services.AddOpenApi();
        // services.RegisterServices(Configuration, Environment);
    }
    
    public void Configure(WebApplication app)
    {
        app.MapHealthChecks("/_health", new()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        if (Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/");
        }

        app.UseHttpsRedirection();

        app.MapGroup("api/v1")
            .MapEntryControlEndpoints();
    }
}
