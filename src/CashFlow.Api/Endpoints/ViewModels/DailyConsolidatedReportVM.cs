namespace CashFlow.Api.Endpoints.ViewModels;

public record DailyConsolidatedReportVM
    (DateOnly Date, decimal TotalCredits, decimal TotalDebits, decimal NetBalance, bool IsClosed);