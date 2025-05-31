using CashFlow.Api.Models;
using ViniBas.ResultPattern.ResultObjects;

namespace CashFlow.Api.Endpoints;

public record EntryVM(decimal? Value, string? Type, string? Description)
{
    public Result Validate()
    {
        var errors = new List<Error>();

        if (Value is null)
            errors.Add(Error.Validation("ValueIsRequired", "The entry value is required."));
        if (Type?.Length != 1 || Type[0] is not 'C' and not 'D')
            errors.Add(Error.Validation("TypesAreDifferentFromCAndD", "The entry type must be only 'C' for credit or 'D' for debit."));

        return errors.Any() ?
            errors : Result.Success();
    }
}
