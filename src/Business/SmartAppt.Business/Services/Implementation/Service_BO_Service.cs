using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Service;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using Microsoft.Data.SqlClient;

namespace Business.SmartAppt.Services.Implementation;

public class Service_BO_Service : IService_BO_Service
{
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IBusinessRepository _businessRepository;

    public Service_BO_Service(IServiceRepository serviceRepository, IBusinessRepository businessRepository)
    {
        _serviceRepository = serviceRepository;
        _businessRepository = businessRepository;
    }

    public virtual async Task<BaseResponse> CreateAsync(ServiceCreateRequestModel model, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse{ Status = BaseResponseStatus.ValidationError };

            if (model.DurationMin < 5 || model.DurationMin > 480)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (model.Price < 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BusinessEntity? businessCheck = await _businessRepository.GetByIdAsync(model.BusinessId);
            if (businessCheck == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            ServiceEntity entity = new ServiceEntity
            {
                BusinessId = model.BusinessId,
                Name = model.Name,
                DurationMin = model.DurationMin,
                Price = model.Price,
                IsActive = true
            };

            int? newId = await _serviceRepository.CreateAsync(entity, ct);

            if (!newId.HasValue || newId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };

            return BaseResponse<int?>.Create(BaseResponseStatus.Success, newId.Value);
        }

        catch (SqlException ex)
        {
            if (ex.Message.Contains("UQ_Service_BusinessId_Name", StringComparison.OrdinalIgnoreCase))
                return new BaseResponse { Status = BaseResponseStatus.AlreadyExists };

            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception)
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> GetByIdAsync(int serviceId, CancellationToken ct)
    {
        try
        {
            var entity = await _serviceRepository.GetByIdAsync(serviceId, ct);

            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.NotFound };

            ServiceModel responseModel = new ServiceModel
            {
                ServiceId = entity.ServiceId,
                BusinessId = entity.BusinessId,
                Name = entity.Name,
                DurationMin = entity.DurationMin,
                Price = entity.Price,
                IsActive = entity.IsActive
            };

            return BaseResponse<ServiceModel>.Create(BaseResponseStatus.IsValid, responseModel);
        }
        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception)
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> UpdateAsync(ServiceModel model, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (model.DurationMin < 5 || model.DurationMin > 480)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (model.Price < 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            var existsStatus = await GetByIdAsync(model.ServiceId, ct);
            if (existsStatus.Status != BaseResponseStatus.IsValid)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            BusinessEntity? businessCheck = await _businessRepository.GetByIdAsync(model.BusinessId);
            if (businessCheck == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            var entity = new ServiceEntity
            {
                ServiceId = model.ServiceId,
                BusinessId = model.BusinessId,
                Name = model.Name,
                DurationMin = model.DurationMin,
                Price = model.Price,
                IsActive = model.IsActive
            };

            await _serviceRepository.UpdateAsync(entity, ct);

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }

        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception)
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> DeleteAsync(int serviceId, CancellationToken ct)
    {
        try
        {
            ServiceEntity? id = await _serviceRepository.GetByIdAsync(serviceId, ct);

            if (id == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            await _serviceRepository.DeleteAsync(serviceId, ct);

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception)
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }
}
