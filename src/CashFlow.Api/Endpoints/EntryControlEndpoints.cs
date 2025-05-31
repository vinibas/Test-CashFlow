using CashFlow.Api.Data;
using CashFlow.Api.Models;
using ViniBas.ResultPattern.AspNet.ResultMinimal;

namespace CashFlow.Api.Endpoints;

public static class EntryControlEndpoints
{
    public static void MapEntryControlEndpoints(this IEndpointRouteBuilder routeBuilder)
        => routeBuilder.MapPost("EntryControl", EntryControlPostHandlerAsync);

    internal static async Task<IResult> EntryControlPostHandlerAsync(IEntryDao entryDao, IUnitOfWork uow, Entry entry)
    {
        var result = entry.Validate();

        if (result.IsSuccess)
        {
            await entryDao.InsertAsync(entry);
            await uow.CommitAsync();
        }

        return result.Match(r => TypedResults.Created());
    }
}
