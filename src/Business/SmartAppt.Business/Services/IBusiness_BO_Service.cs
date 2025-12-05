using Business.SmartAppt.Models;

namespace Business.SmartAppt.Services;

public interface IBusiness_BO_Service
{
    Task<BaseResponse> CreateAsync(BusinessCreateRequestModel model, CancellationToken ct);
    Task<BaseResponse> UpdateAsync(BusinessModel model, CancellationToken ct);
    Task<BaseResponse> GetByIdAsync(int businessId, CancellationToken ct);
    Task<BaseResponse> DeleteAsync(int businessId, CancellationToken ct);
}
