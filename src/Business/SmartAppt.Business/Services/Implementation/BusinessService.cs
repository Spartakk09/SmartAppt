using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Booking;
using Business.SmartAppt.Models.Service;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using Data.SmartAppt.SQL.Services.Implementation;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata;

namespace Business.SmartAppt.Services.Implementation;

public class BusinessService : IBusinessService
{
    protected readonly IBusinessRepository _businessRepository;
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IBookingRepository _bookingRepository;

    public BusinessService(IBusinessRepository businessRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository)
    {
        _businessRepository = businessRepository;
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
    }

    //Create Business
    public virtual async Task<BaseResponse> CreateBusinessAsync(BusinessCreateRequestModel model, CancellationToken ct)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            // Prepare entity
            BusinessEntity entity = new BusinessEntity
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                TimeZoneIana = model.TimeZoneIana,
                SettingsJson = model.SettingsJson
            };

            // Create in repository
            int? newId = await _businessRepository.CreateAsync(entity, ct);

            if (!newId.HasValue || newId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };

            return BaseResponse<int>.Create(BaseResponseStatus.Success, newId.Value);
        }

        catch (SqlException)
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }

        catch
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.UnknownError
            };
        }
    }

    //Update Business By Id
    public virtual async Task<BaseResponse> UpdateBusinessByIdAsync(int businessId, BusinessCreateRequestModel model, CancellationToken ct)
    {
        try
        {
            BusinessEntity? entity = await _businessRepository.GetByIdAsync(businessId, ct);
            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            await _businessRepository.UpdateAsync(new BusinessEntity
            {
                BusinessId = businessId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                TimeZoneIana = model.TimeZoneIana,
                SettingsJson = model.SettingsJson
            }, ct);

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException)
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }
        catch
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.UnknownError
            };
        }
    }

    //Delete Business
    public virtual async Task<BaseResponse> DeleteBusinessAsync(int businessId, CancellationToken ct)
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
            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }

        catch
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.UnknownError
            };
        }
    }

    //Add Services
    public virtual async Task<BaseResponse> AddServicesAsync(ServiceCreateRequestModel model, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (model.DurationMin < 5 || model.DurationMin > 480)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (model.Price < 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            var businessCheck = await _businessRepository.GetByIdAsync(model.BusinessId);
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

            return BaseResponse<int>.Create(BaseResponseStatus.Success, newId.Value);
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

    //elete Service
    public virtual async Task<BaseResponse> DeleteServiceAsync(int serviceId, CancellationToken ct)
    {
        try
        {
            ServiceEntity? entity = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            await _serviceRepository.DeleteAsync(serviceId, ct);

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

    //Disable Servcie
    public virtual async Task<BaseResponse> DisableServiceAsync(int serviceId, CancellationToken ct)
    {
        try
        {
            ServiceEntity? entity = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            await _serviceRepository.DisableServiceAsync(serviceId, ct);

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

    //Cancel Booking
    public virtual async Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        try
        {
            if (bookingId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BookingEntity? bookingExists = await _bookingRepository.GetByIdAsync(bookingId, ct);
            if (bookingExists == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };

            bool success = await _bookingRepository.CancelBookingAsync(bookingId, ct);

            if (!success)
                return new BaseResponse { Status = BaseResponseStatus.AlreadyCanceled };

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

    //Get Daily Bookings with Customer Details 
    public virtual async Task<BaseResponse> GetDailyBookingsWithCustomersAsync(int businessId, DateTime dateUtc, CancellationToken ct)
    {
        try
        {
            if (businessId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BusinessEntity? business =
                await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            List<BookingWithCustomerDetailsModel> dbResult =
                await _bookingRepository.GetDailyBookingsWithCustomersAsync(businessId, dateUtc, ct);

            if (dbResult.Count == 0)
                return new BaseResponse { Status = BaseResponseStatus.Empty };

            List<DailyBookingResponseModel> result =
                dbResult.Select(x => new DailyBookingResponseModel
                {
                    BookingId = x.BookingId,
                    StartAtUtc = x.StartAtUtc,
                    EndAtUtc = x.EndAtUtc,
                    Status = x.Status,
                    Notes = x.Notes,

                    Service = new ServiceResponseModel
                    {
                        ServiceId = x.ServiceId,
                        Name = x.ServiceName,
                        DurationMin = x.DurationMin,
                        Price = x.Price
                    },

                    Customer = new CustomerResponseModel
                    {
                        CustomerId = x.CustomerId,
                        FullName = x.FullName,
                        Email = x.Email,
                        Phone = x.Phone
                    }
                }).ToList();

            return BaseResponse<List<DailyBookingResponseModel>>.Create(BaseResponseStatus.Success, result);
        }

        catch (SqlException)
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }

        catch (Exception)
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }
    }

    //Get All Bookings By Business Id
    public virtual async Task<BaseResponse> GetAllBookingsByBusinessIdAsync(int businessId, CancellationToken ct)
    {
        try
        {
            if (businessId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            List<BookingEntity> bookings = await _bookingRepository.GetAllBookingsByBusinessIdAsync(businessId, ct);
            if (bookings.Count == 0)
                return new BaseResponse { Status = BaseResponseStatus.Empty };

            List<BookingModel> result = bookings.Select(b => new BookingModel
            {
                BookingId = b.BookingId,
                ServiceId = b.ServiceId,
                CustomerId = b.CustomerId,
                StartAtUtc = b.StartAtUtc,
                EndAtUtc = b.EndAtUtc,
                Status = b.Status,
                Notes = b.Notes
            }).ToList();

            if (result.Count == 0)
                return new BaseResponse { Status = BaseResponseStatus.Empty };

            return BaseResponse<List<BookingModel>>.Create(BaseResponseStatus.Success, result);
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

    //Decide Bookings
    public virtual async Task<BaseResponse> DecideBookingsAsync(int bookingId, string status, CancellationToken ct)
    {
        try
        {
            if (bookingId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BookingEntity? booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
            if (booking == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };

            if (status != "Confirmed" && status != "Canceled")
                return new BaseResponse { Status = BaseResponseStatus.InvalidStatus };

            bool updated = await _bookingRepository.DecideBookingStatusAsync(bookingId, status, ct);

            if (!updated)
                return new BaseResponse { Status = BaseResponseStatus.FailToUpdate };


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

    //Get Calendar (Month)
    public virtual async Task<BaseResponse> GetMonthlyCalendarAsync(int businessId, int month, int? year, CancellationToken ct)
    {
        try
        {
            if (businessId <= 0 || month < 1 || month > 12)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            int checkYear = year ?? DateTime.UtcNow.Year;
            int daysInMonth = DateTime.DaysInMonth(checkYear, month);

            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            DateTime fromUtc = new DateTime(checkYear, month, 1);
            DateTime toUtc = fromUtc.AddMonths(1).AddTicks(-1);

            List<BookingCountByDayModel> dbResult = await _bookingRepository
                .GetMonthlyCalendarAsync(businessId, fromUtc, toUtc, ct);

            List<MonthlyCalendarModel> result = new List<MonthlyCalendarModel>();
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(checkYear, month, day);
                int bookingCount = dbResult
                    .FirstOrDefault(b => b.BookingDate.Date == currentDate.Date)?.BookingCount ?? 0;

                result.Add(new MonthlyCalendarModel
                {
                    DateUtc = currentDate,
                    BookingCount = bookingCount
                });
            }

            if (result.All(d => d.BookingCount == 0))
                return new BaseResponse { Status = BaseResponseStatus.Empty };

            return BaseResponse<List<MonthlyCalendarModel>>.Create(BaseResponseStatus.Success, result);
        }

        catch (SqlException)
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }

        catch (Exception)
        {
            return new BaseResponse
            {
                Status = BaseResponseStatus.UnknownError
            };
        }
    }
}