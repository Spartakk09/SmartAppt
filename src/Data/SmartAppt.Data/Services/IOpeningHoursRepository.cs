using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services;

public interface IOpeningHoursRepository
{
    Task<OpeningHoursEntity?> GetByDateAsync(int businessId, DateTime dateUtc, CancellationToken ct);
    Task<List<OpeningHoursEntity>> GetOpeningHoursAsync(int businessId, CancellationToken ct);
}
