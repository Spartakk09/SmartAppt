using Business.SmartAppt.Models;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using Microsoft.Data.SqlClient;

namespace Business.SmartAppt.Services.Implementation;

public class Business_BO_Service : IBusiness_BO_Service
{
    protected readonly IBusinessRepository _businessRepository;

    public Business_BO_Service(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    // Create a new business
    public virtual async Task<BaseResponse> CreateAsync(BusinessCreateRequestModel model, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BusinessEntity entity = new BusinessEntity
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                TimeZoneIana = model.TimeZoneIana,
                SettingsJson = model.SettingsJson
            };

            int? newId = await _businessRepository.CreateAsync(entity, ct);

            if (!newId.HasValue || newId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };

            return BaseResponse<int>.Create(BaseResponseStatus.Success, newId.Value);
        }
        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    // Update an existing business
    public virtual async Task<BaseResponse> UpdateAsync(BusinessModel model, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BusinessEntity? entity = await _businessRepository.GetByIdAsync(model.BusinessId, ct);
            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            await _businessRepository.UpdateAsync(new BusinessEntity
            {
                BusinessId = model.BusinessId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                TimeZoneIana = model.TimeZoneIana,
                SettingsJson = model.SettingsJson,
                CreatedAtUtc = model.CreatedAtUtc
            }, ct);

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    // Get business by ID
    public virtual async Task<BaseResponse> GetByIdAsync(int businessId, CancellationToken ct)
    {
        try
        {
            BusinessEntity? entity = await _businessRepository.GetByIdAsync(businessId, ct);
            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            BusinessModel response = new BusinessModel
            {
                BusinessId = entity.BusinessId,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                TimeZoneIana = entity.TimeZoneIana,
                SettingsJson = entity.SettingsJson,
                CreatedAtUtc = entity.CreatedAtUtc
            };

            return BaseResponse<BusinessModel>.Create(BaseResponseStatus.IsValid, response);
        }
        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    // Delete a business
    public virtual async Task<BaseResponse> DeleteAsync(int businessId, CancellationToken ct)
    {
        try
        {
            BusinessEntity? enitty = await _businessRepository.GetByIdAsync(businessId, ct);
            if (enitty == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            await _businessRepository.DeleteAsync(businessId, ct);

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException)
        {
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }
}
