namespace CashFlow.Api.Data;

public interface IUnitOfWork : IDisposable
{
    Task<bool> CommitAsync();
}
