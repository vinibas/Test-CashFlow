
using CashFlow.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CashFlow.Api.Data.Daos;

public interface IDailyConsolidatedDao
{
    Task<DailyConsolidated?> GetConsolidatedUpdatedAsync(DateOnly dateOnly);
}

public class DailyConsolidatedDao : IDailyConsolidatedDao
{
    private readonly CashFlowContext _context;

    public DailyConsolidatedDao(CashFlowContext context)
        => _context = context;

    public virtual async Task<DailyConsolidated?> GetConsolidatedUpdatedAsync(DateOnly date)
    {
        var dateParam = new NpgsqlParameter("p_date", NpgsqlTypes.NpgsqlDbType.Date)
            { Value = date.ToDateTime(TimeOnly.MinValue) };
        var result = await _context.DailyConsolidated
            .FromSqlRaw(@"SELECT * FROM update_and_get_daily_consolidated(@p_date)", dateParam)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return result;
    }
}
