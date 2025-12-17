using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services;

public interface IHolidayRepository
{
    Task<bool> ExistsAsync(int businessId, DateTime dateUtc, CancellationToken ct);
    Task<List<HolidayEntity>> GetInRangeAsync(int businessId, DateTime start, DateTime end, CancellationToken ct);
}
