using CashFlow.Api.Data;
using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Data.Daos;

public interface IEntryDao
{
    Task InsertAsync(Entry entry);
    Task<IEnumerable<Entry>> ListEntriesByDateAsync(DateOnly date, long maxLineNumber);
}

public class EntryDao : IEntryDao
{
    private readonly CashFlowContext _context;

    public EntryDao(CashFlowContext context)
        => _context = context;

    public async Task InsertAsync(Entry entry)
        => await _context.AddAsync(entry);

    public async Task<IEnumerable<Entry>> ListEntriesByDateAsync(DateOnly date, long maxLineNumber)
        => await _context.Entries
        .Where(e => DateOnly.FromDateTime(e.TransactionAtUtc) == date)
        .Where(e => e.LineNumber <= maxLineNumber)
        .OrderBy(e => e.TransactionAtUtc)
        .ToListAsync();
}
