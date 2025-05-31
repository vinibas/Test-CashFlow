using ViniBas.ResultPattern.ResultObjects;

namespace CashFlow.Api.Models;

public class Entry
{
    public decimal Value { get; private set; }
    public char Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

    public Entry(decimal value, char type, string? description)
    {
        Value = value;
        Type = type;
        Description = description;
    }

    public Result Validate()
    {
        var errors = new List<Error>();

        if (Value <= 0)
            errors.Add(Error.Validation("ValueMustBeGreaterThanZero", "The entry value must be greater than zero."));
        if (Type is not 'C' and not 'D')
            errors.Add(Error.Validation("TypesAreDifferentFromCAndD", "The entry type must be only 'C' for credit or 'D' for debit."));

        return errors.Any() ?
            errors : Result.Success();
    }
}
