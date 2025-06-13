using CashFlow.Api.Data;
using CashFlow.Api.Endpoints;
using HealthChecks.UI.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using ViniBas.ResultPattern.AspNet;

namespace CashFlow.Api.Configurations;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{

    public IConfiguration Configuration { get; } = configuration;
    public IWebHostEnvironment Environment { get; } = environment;

    public void ConfigureLog(IHostBuilder host)
    {
        host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.WithExceptionDetails(CreateDestructuringOptionsBuilder())
                .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
        });

        static DestructuringOptionsBuilder CreateDestructuringOptionsBuilder()
            => new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithDestructurers(
                    [
                        new DbUpdateExceptionDestructurer(),
                    ]);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var conString = Configuration.GetConnectionString("CashFlowDatabase") ??
            throw new InvalidOperationException("Connection string 'CashFlowDatabase' not found.");
        var seqEndpoint = Configuration.GetValue<string>("Seq:HealthUrl") ??
            throw new InvalidOperationException("Seq endpoint not found in configuration.");
        var seqHealthUrl = Configuration.GetValue<string>("Seq:HealthUrl") 
            ?? throw new InvalidOperationException("Seq health url not found in configuration.");

        services.AddProblemDetails();
        services.AddHealthChecks()
            .AddNpgSql(conString)
            .AddUrlGroup(new Uri(seqHealthUrl), name: "seq", failureStatus: HealthStatus.Degraded)
            .AddSeqPublisher((options) => { options.Endpoint = seqEndpoint; });

        services.AddOpenApi();
        
        services.RegisterServices(Configuration, Environment.IsDevelopment());
    }

    public void Configure(WebApplication app)
    {
        GlobalConfiguration.UseProblemDetails = true;

        ApplyMigrationsIfNotInProduction(app);

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        app.UseSerilogRequestLogging();

        app.MapHealthChecks("/_health", new()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapOpenApi();
        app.MapScalarApiReference("/");

        app.UseHttpsRedirection();

        app.MapGroup("api/v1")
            .MapEntryControlEndpoints()
            .MapDailyConsolidatedReportEndpoints();
    }

    private void ApplyMigrationsIfNotInProduction(WebApplication app)
    {
        var inIntegrationTestContext = Configuration.GetValue<string>("InIntegrationTestContext");

        if (!Environment.IsProduction() && inIntegrationTestContext != "true")
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CashFlowContext>();
            db.Database.Migrate();
        }
    }
}
