using System.Globalization;
using CashFlow.Api.Data;
using CashFlow.Api.Data.Daos;
using CashFlow.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using ViniBas.ResultPattern.AspNet.ResultMinimal;
using ViniBas.ResultPattern.ResultObjects;

namespace CashFlow.Api.Endpoints;

public static class EntryControlEndpoints
{
    public static IEndpointRouteBuilder MapEntryControlEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapPost("EntryControl/{date?}", EntryControlPostHandlerAsync);
        return routeBuilder;
    }

    internal static async Task<Results<Created, ProblemHttpResult>> EntryControlPostHandlerAsync
        (IEntryDao entryDao, IUnitOfWork uow, EntryVM entryVM, string? date)
    {
        DateTime? transactionAtUtc = null;

        if (date != null)
        {
            if (!DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateParsed))

                return Error
                    .Validation("InvalidFormatDate", "The date format is invalid.")
                    .ToProblemDetailsResult();

            transactionAtUtc = dateParsed;
        }

        var resultVm = entryVM.Validate();

        if (resultVm.IsFailure)
            return resultVm.ToResponse().ToProblemDetailsResult();

        var typeCasted = (EntryType)entryVM.Type!.Single();
        var entry = new Entry(entryVM.Value!.Value, typeCasted, entryVM.Description, transactionAtUtc);
        var result = entry.Validate();

        if (result.IsSuccess)
        {
            await entryDao.InsertAsync(entry);
            await uow.CommitAsync();
        }

        return result.Match<Results<Created, ProblemHttpResult>, Created>(r => TypedResults.Created());
    }
}
