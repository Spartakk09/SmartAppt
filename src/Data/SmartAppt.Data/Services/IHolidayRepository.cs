namespace Data.SmartAppt.SQL.Services;

public interface IHolidayRepository
{
    Task<bool> ExistsAsync(int businessId, DateTime dateUtc, CancellationToken ct);
}
