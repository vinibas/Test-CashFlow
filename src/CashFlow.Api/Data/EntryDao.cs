using CashFlow.Api.Models;

namespace CashFlow.Api;

public interface IEntryDao
{
    Task InsertAsync(Entry entry);
}

public class EntryDao : IEntryDao
{
    public Task InsertAsync(Entry entry)
    {
        throw new NotImplementedException();
    }
}
