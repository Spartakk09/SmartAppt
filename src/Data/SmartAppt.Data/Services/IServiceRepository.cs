using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services;

public interface IServiceRepository
{
    public Task<int?> CreateAsync(ServiceEntity service, CancellationToken ct);
    public Task UpdateAsync(ServiceEntity service, CancellationToken ct);
    public Task<ServiceEntity?> GetByIdAsync(int businessId, CancellationToken ct = default);
    public Task DeleteAsync(int serviceId, CancellationToken ct = default);
    public Task DisableServiceAsync(int serviceId, CancellationToken ct);
    public Task<List<ServiceEntity>> GetSeviceByBusinessIdAsync(int businessId, int skip = 0, int take = 10, CancellationToken ct = default);
}
