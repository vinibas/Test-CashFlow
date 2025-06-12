using CashFlow.Api.Models;

namespace CashFlow.Api.Endpoints.ViewModels;

public record DailyConsolidatedReportVM
    (DateOnly Date, decimal TotalCredits, decimal TotalDebits, decimal NetBalance, bool IsClosed);

public record DailyConsolidatedReportExtendedVM
    (DateOnly Date, IEnumerable<EntryReportItemVM> Entries, decimal TotalCredits, decimal TotalDebits, decimal NetBalance, bool IsClosed);

public record EntryReportItemVM(decimal Value, char Type, string? Description, DateTime TransactionAtUtc);
