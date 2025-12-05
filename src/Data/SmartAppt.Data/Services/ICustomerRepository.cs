using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services;

public interface ICustomerRepository
{
    public Task<int?> CreateAsync(CustomerEntity customer, CancellationToken ct);
    public Task UpdateAsync(CustomerEntity customer, CancellationToken ct);
    public Task<CustomerEntity?> GetByIdAsync(int customerId, CancellationToken ct = default);
    public Task DeleteAsync(int customerId, CancellationToken ct = default);
}
