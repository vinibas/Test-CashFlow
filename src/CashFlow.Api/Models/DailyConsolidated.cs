namespace CashFlow.Api.Models;

public class DailyConsolidated
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public DateOnly Date { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal NetBalance => TotalCredits - TotalDebits;
    public bool IsClosed => DateOnly.FromDateTime(DateTime.UtcNow) > Date;

    public long LastLineNumberCalculated { get; private set; }

    // For EF
    private DailyConsolidated() { }

    public DailyConsolidated(DateOnly date, decimal totalCredits, decimal totalDebits)
    {
        Date = date;
        TotalCredits = totalCredits;
        TotalDebits = totalDebits;
    }
}
