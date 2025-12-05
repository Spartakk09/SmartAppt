using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services;

public interface IBusinessRepository
{
    public Task<int?> CreateAsync(BusinessEntity business, CancellationToken ct);
    public Task UpdateAsync(BusinessEntity business, CancellationToken ct);
    public Task<BusinessEntity?> GetByIdAsync(int businessId, CancellationToken ct = default);
    public Task DeleteAsync(int businessId, CancellationToken ct = default);
}
