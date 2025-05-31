using CashFlow.Api.Data;
using CashFlow.Api.Models;

namespace CashFlow.Api;

public interface IEntryDao
{
    Task InsertAsync(Entry entry);
}

public class EntryDao : IEntryDao
{
    private readonly CashFlowContext _context;

    public EntryDao(CashFlowContext context)
        => _context = context;

    public async Task InsertAsync(Entry entry)
        => await _context.AddAsync(entry);
}
