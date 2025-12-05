using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Service;

namespace Business.SmartAppt.Services;

public interface IService_BO_Service
{
    Task<BaseResponse> CreateAsync(ServiceCreateRequestModel model, CancellationToken ct);
    Task<BaseResponse> UpdateAsync(ServiceModel model, CancellationToken ct);
    Task<BaseResponse> GetByIdAsync(int serviceId, CancellationToken ct);
    Task<BaseResponse> DeleteAsync(int serviceId, CancellationToken ct);
}
