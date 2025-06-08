using ViniBas.ResultPattern.ResultObjects;

namespace CashFlow.Api.Models;

public class Entry
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public decimal Value { get; private set; }
    public EntryType Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime TransactionAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;

    // For EF
    private Entry() { }

    public Entry(decimal value, EntryType type, string? description, DateTime? transactionAtUtc)
    {
        // If Kind doesn't specify a Kind, then Postgresql throws a exception
        if (transactionAtUtc.HasValue && transactionAtUtc.Value.Kind == DateTimeKind.Unspecified)
            transactionAtUtc = DateTime.SpecifyKind(transactionAtUtc.Value, DateTimeKind.Utc);
        
        Value = value;
        Type = type;
        Description = description;
        TransactionAtUtc = transactionAtUtc ?? CreatedAtUtc;
    }

    public Result Validate()
    {
        var errors = new List<Error>();

        if (Value <= 0)
            errors.Add(Error.Validation("ValueMustBeGreaterThanZero", "The entry value must be greater than zero."));
        if (Value != Math.Round(Value, 2))
            errors.Add(Error.Validation("ValueIsInAIncorrectFormat", "The monetary value entered is in an incorrect format."));
        if (Description?.Length > 250)
            errors.Add(Error.Validation("DescriptionIsGreatherThenMaxLength", "The entry description cannot be longer than 250 characters."));

        return errors.Any() ?
            errors : Result.Success();
    }
}
