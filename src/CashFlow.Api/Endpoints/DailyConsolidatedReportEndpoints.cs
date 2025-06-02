using CashFlow.Api.Data.Daos;
using CashFlow.Api.Endpoints.ViewModels;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using ViniBas.ResultPattern.AspNet.ResultMinimal;
using ViniBas.ResultPattern.ResultObjects;

namespace CashFlow.Api.Endpoints;

public static class DailyConsolidatedReportEndpoints
{
    public static IEndpointRouteBuilder MapDailyConsolidatedReportEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("DailyConsolidatedReport/{date}", DailyConsolidatedReportGetHandlerAsync);
        return routeBuilder;
    }

    internal static async Task<Results<Ok<DailyConsolidatedReportVM>, ProblemHttpResult>> DailyConsolidatedReportGetHandlerAsync
        (IDailyConsolidatedDao dailyConsolidatedDao, string date)
    {
        if (!DateOnly.TryParse(date, out var dateDO))
            return Error
                .Validation("InvalidFormatDate", "The date format is invalid. Use 'yyyy-MM-dd'.")
                .ToProblemDetailsResult();

        var consolidated = await dailyConsolidatedDao.GetConsolidatedUpdatedAsync(dateDO);

        if (consolidated is null)
            consolidated = new DailyConsolidated(dateDO, 0, 0);
        
        var resultVM = new DailyConsolidatedReportVM(
                consolidated.Date,
                consolidated.TotalCredits,
                consolidated.TotalDebits,
                consolidated.NetBalance,
                consolidated.IsClosed);

        return TypedResults.Ok(resultVM);
    }
}
