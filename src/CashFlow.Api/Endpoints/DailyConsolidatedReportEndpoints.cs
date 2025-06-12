using CashFlow.Api.Data.Daos;
using CashFlow.Api.Endpoints.ViewModels;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using ViniBas.ResultPattern.AspNet.ResultMinimal;
using ViniBas.ResultPattern.ResultObjects;

namespace CashFlow.Api.Endpoints;

using ResultReport = Results<Ok<DailyConsolidatedReportVM>, ProblemHttpResult>;
using ResultReportExtended = Results<Ok<DailyConsolidatedReportExtendedVM>, ProblemHttpResult>;

public static class DailyConsolidatedReportEndpoints
{
    public static IEndpointRouteBuilder MapDailyConsolidatedReportEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("DailyConsolidatedReport/{date}", DailyConsolidatedReportGetHandlerAsync);
        routeBuilder.MapGet("DailyConsolidatedReport/Extended/{date}", DailyConsolidatedReportExtendedGetHandlerAsync);

        return routeBuilder;
    }

    internal static async Task<ResultReport> DailyConsolidatedReportGetHandlerAsync
        (IDailyConsolidatedDao dailyConsolidatedDao, string date)
    {
        if (!DateOnly.TryParse(date, out var dateDO))
            return Error
                .Validation("InvalidFormatDateOnly", "The date format is invalid. Use 'yyyy-MM-dd'.")
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

    internal static async Task<ResultReportExtended> DailyConsolidatedReportExtendedGetHandlerAsync(
        IDailyConsolidatedDao dailyConsolidatedDao,
        IEntryDao entryDao,
        string date)
    {
        if (!DateOnly.TryParse(date, out var dateDO))
            return Error
                .Validation("InvalidFormatDateOnly", "The date format is invalid. Use 'yyyy-MM-dd'.")
                .ToProblemDetailsResult();

        var consolidated = await dailyConsolidatedDao.GetConsolidatedUpdatedAsync(dateDO);

        IEnumerable<Entry> entries;

        if (consolidated is null)
        {
            consolidated = new DailyConsolidated(dateDO, 0, 0);
            entries = [];
        }
        else
            entries = await entryDao.ListEntriesByDateAsync(dateDO, consolidated.LastLineNumberCalculated);

        var resultVM = new DailyConsolidatedReportExtendedVM(
            consolidated.Date,
            entries.Select(e => new EntryReportItemVM(e.Value, (char)e.Type, e.Description, e.TransactionAtUtc)),
            consolidated.TotalCredits,
            consolidated.TotalDebits,
            consolidated.NetBalance,
            consolidated.IsClosed);

        return TypedResults.Ok(resultVM);
    }
}
