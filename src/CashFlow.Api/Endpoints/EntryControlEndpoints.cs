using Microsoft.AspNetCore.Http.HttpResults;

namespace CashFlow.Api.Endpoints;

public static class EntryControlEndpoints
{
    public static void MapEntryControlEndpoints(this IEndpointRouteBuilder routeBuilder)
        => routeBuilder.MapPost("EntryControl", EntryControlPostHandler);

    internal static IResult EntryControlPostHandler() => TypedResults.Created();
}
