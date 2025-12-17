using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using Microsoft.Data.SqlClient;
using SmartAppt.Common.Logging;

namespace Business.SmartAppt.Services.Implementation;

public class BusinessService : IBusinessService
{
    protected readonly IBusinessRepository _businessRepository;
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IBookingRepository _bookingRepository;
    private readonly IAppLogger<BusinessService> _logger;

    public BusinessService(IBusinessRepository businessRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository,
        IAppLogger<BusinessService> logger)
    {
        _businessRepository = businessRepository;
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    //Create Business
    public virtual async Task<BaseResponse> CreateBusinessAsync(
    BusinessCreateRequestModel model,
    CancellationToken ct)
    {
        try
        {
            _logger.Info("CreateBusiness started");

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                _logger.Warn("CreateBusiness failed: Name is empty");
                return new BaseResponse
                {
                    Status = BaseResponseStatus.InvalidInput
                };
            }

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
            {
                _logger.Warn("CreateBusiness failed: Repository returned invalid ID");
                return new BaseResponse
                {
                    Status = BaseResponseStatus.CreationFailed
                };
            }

            _logger.Info($"CreateBusiness succeeded. BusinessId={newId.Value}");

            return BaseResponse<int>.Create(BaseResponseStatus.Success, newId.Value);
        }
        catch (SqlException ex)
        {
            _logger.Error("CreateBusiness database error", ex);

            return new BaseResponse
            {
                Status = BaseResponseStatus.DatabaseError
            };
        }
        catch (Exception ex)
        {
            _logger.Error("CreateBusiness unexpected error", ex);

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
            _logger.Info($"UpdateBusiness started. BusinessId={businessId}");

            // Retrieve existing entity
            BusinessEntity? entity = await _businessRepository.GetByIdAsync(businessId, ct);
            if (entity == null)
            {
                _logger.Warn($"UpdateBusiness failed: BusinessId={businessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                _logger.Warn($"UpdateBusiness failed: Name is empty for BusinessId={businessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            _logger.Debug($"UpdateBusiness request data: {System.Text.Json.JsonSerializer.Serialize(model)}");

            await _businessRepository.UpdateAsync(new BusinessEntity
            {
                BusinessId = businessId,
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                TimeZoneIana = model.TimeZoneIana,
                SettingsJson = model.SettingsJson
            }, ct);

            _logger.Info($"UpdateBusiness succeeded. BusinessId={businessId}");

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException ex)
        {
            _logger.Error($"UpdateBusiness database error. BusinessId={businessId}", ex);

            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"UpdateBusiness unexpected error. BusinessId={businessId}", ex);

            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Delete Business
    public virtual async Task<BaseResponse> DeleteBusinessAsync(int businessId, CancellationToken ct)
    {
        try
        {
            _logger.Info($"DeleteBusiness started. BusinessId={businessId}");

            BusinessEntity? entity = await _businessRepository.GetByIdAsync(businessId, ct);
            if (entity == null)
            {
                _logger.Warn($"DeleteBusiness failed: BusinessId={businessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            await _businessRepository.DeleteAsync(businessId, ct);

            _logger.Info($"DeleteBusiness succeeded. BusinessId={businessId}");

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException ex)
        {
            _logger.Error($"DeleteBusiness database error. BusinessId={businessId}", ex);

            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"DeleteBusiness unexpected error. BusinessId={businessId}", ex);

            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Add Services
    public virtual async Task<BaseResponse> AddServicesAsync(
    ServiceCreateRequestModel model,
    CancellationToken ct)
    {
        try
        {
            _logger.Info($"AddServices started for BusinessId={model.BusinessId}");

            // Validation
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                _logger.Warn("AddServices failed: Name is empty");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            if (model.DurationMin < 5 || model.DurationMin > 480)
            {
                _logger.Warn($"AddServices failed: DurationMin={model.DurationMin} is out of range");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            if (model.Price < 0)
            {
                _logger.Warn($"AddServices failed: Price={model.Price} is negative");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BusinessEntity? businessCheck = await _businessRepository.GetByIdAsync(model.BusinessId);
            if (businessCheck == null)
            {
                _logger.Warn($"AddServices failed: BusinessId={model.BusinessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

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
            {
                _logger.Warn($"AddServices failed: Repository returned invalid ID for BusinessId={model.BusinessId}");
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };
            }

            _logger.Info($"AddServices succeeded. ServiceId={newId.Value} for BusinessId={model.BusinessId}");

            return BaseResponse<int>.Create(BaseResponseStatus.Success, newId.Value);
        }

        catch (SqlException ex)
        {
            if (ex.Message.Contains("UQ_Service_BusinessId_Name", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn($"AddServices failed: Service already exists for BusinessId={model.BusinessId}, Name={model.Name}");
                return new BaseResponse { Status = BaseResponseStatus.AlreadyExists };
            }

            _logger.Error($"AddServices database error for BusinessId={model.BusinessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }

        catch (Exception ex)
        {
            _logger.Error($"AddServices unexpected error for BusinessId={model.BusinessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Delete Service
    public virtual async Task<BaseResponse> DeleteServiceAsync(int serviceId, CancellationToken ct)
    {
        try
        {
            _logger.Info($"DeleteService started. ServiceId={serviceId}");

            ServiceEntity? entity = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (entity == null)
            {
                _logger.Warn($"DeleteService failed: ServiceId={serviceId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };
            }

            await _serviceRepository.DeleteAsync(serviceId, ct);

            _logger.Info($"DeleteService succeeded. ServiceId={serviceId}");

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException ex)
        {
            _logger.Error($"DeleteService database error. ServiceId={serviceId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"DeleteService unexpected error. ServiceId={serviceId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Disable Servcie
    public virtual async Task<BaseResponse> DisableServiceAsync(int serviceId, CancellationToken ct)
    {
        try
        {
            _logger.Info($"DisableService started. ServiceId={serviceId}");

            ServiceEntity? entity = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (entity == null)
            {
                _logger.Warn($"DisableService failed: ServiceId={serviceId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };
            }

            await _serviceRepository.DisableServiceAsync(serviceId, ct);

            _logger.Info($"DisableService succeeded. ServiceId={serviceId}");

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException ex)
        {
            _logger.Error($"DisableService database error. ServiceId={serviceId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"DisableService unexpected error. ServiceId={serviceId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Cancel Booking
    public virtual async Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        try
        {
            _logger.Info($"CancelBooking started. BookingId={bookingId}");

            if (bookingId <= 0)
            {
                _logger.Warn($"CancelBooking failed: Invalid BookingId={bookingId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BookingEntity? bookingExists = await _bookingRepository.GetByIdAsync(bookingId, ct);
            if (bookingExists == null)
            {
                _logger.Warn($"CancelBooking failed: BookingId={bookingId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };
            }

            bool success = await _bookingRepository.CancelBookingAsync(bookingId, ct);

            if (!success)
            {
                _logger.Warn($"CancelBooking failed: BookingId={bookingId} is already canceled");
                return new BaseResponse { Status = BaseResponseStatus.AlreadyCanceled };
            }

            _logger.Info($"CancelBooking succeeded. BookingId={bookingId}");
            return new BaseResponse { Status = BaseResponseStatus.Success };
        }
        catch (SqlException ex)
        {
            _logger.Error($"CancelBooking database error. BookingId={bookingId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"CancelBooking unexpected error. BookingId={bookingId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Get Daily Bookings with Customer Details 
    public virtual async Task<BaseResponse> GetDailyBookingsWithCustomersAsync(
    int businessId,
    DateTime dateUtc,
    CancellationToken ct)
    {
        try
        {
            _logger.Info($"GetDailyBookingsWithCustomers started. BusinessId={businessId}, Date={dateUtc:yyyy-MM-dd}");

            if (businessId <= 0)
            {
                _logger.Warn($"GetDailyBookingsWithCustomers failed: Invalid BusinessId={businessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
            {
                _logger.Warn($"GetDailyBookingsWithCustomers failed: BusinessId={businessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            List<BookingWithCustomerDetailsModel> dbResult = await _bookingRepository.GetDailyBookingsWithCustomersAsync(businessId, dateUtc, ct);

            if (dbResult.Count == 0)
            {
                _logger.Info($"GetDailyBookingsWithCustomers: No bookings found for BusinessId={businessId} on {dateUtc:yyyy-MM-dd}");
                return new BaseResponse { Status = BaseResponseStatus.Empty };
            }

            List<DailyBookingResponseModel> result = dbResult.Select(x => new DailyBookingResponseModel
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

            _logger.Info($"GetDailyBookingsWithCustomers succeeded. BusinessId={businessId}, BookingsCount={result.Count}");

            return BaseResponse<List<DailyBookingResponseModel>>.Create(BaseResponseStatus.Success, result);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetDailyBookingsWithCustomers database error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetDailyBookingsWithCustomers unexpected error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
    }

    //Get Bookings By Business Id
    public virtual async Task<BaseResponse> GetBookingsAsync(
    int businessId,
    string? status = null,
    DateOnly? date = null,
    int skip = 0,
    int take = 50,
    CancellationToken ct = default)
    {
        _logger.Info($"GetBookings started. BusinessId={businessId}, Status={status}, Date={date}, Skip={skip}, Take={take}");

        if (businessId <= 0 || skip < 0 || take <= 0)
        {
            _logger.Warn($"GetBookings failed: Invalid input. BusinessId={businessId}, Skip={skip}, Take={take}");
            return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
        }

        if (status != null && status != "Pending" && status != "Confirmed" && status != "Canceled")
        {
            _logger.Warn($"GetBookings failed: Invalid status={status}");
            return new BaseResponse { Status = BaseResponseStatus.InvalidStatus };
        }

        BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
        if (business == null)
        {
            _logger.Warn($"GetBookings failed: BusinessId={businessId} not found");
            return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
        }

        try
        {
            List<BookingEntity> bookings = await _bookingRepository.GetBookingsAsync(businessId, status, date, skip, take, ct);

            if (!bookings.Any())
            {
                _logger.Info($"GetBookings: No bookings found for BusinessId={businessId}, Status={status}, Date={date}");
                return new BaseResponse { Status = BaseResponseStatus.Empty };
            }

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

            _logger.Info($"GetBookings succeeded. BusinessId={businessId}, BookingsCount={result.Count}");
            return BaseResponse<List<BookingModel>>.Create(BaseResponseStatus.Success, result);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetBookings database error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetBookings unexpected error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Decide Bookings
    public virtual async Task<BaseResponse> DecideBookingsAsync(
    int bookingId,
    string status,
    CancellationToken ct)
    {
        _logger.Info($"DecideBookings started. BookingId={bookingId}, Status={status}");

        try
        {
            if (bookingId <= 0)
            {
                _logger.Warn($"DecideBookings failed: Invalid BookingId={bookingId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BookingEntity? booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
            if (booking == null)
            {
                _logger.Warn($"DecideBookings failed: BookingId={bookingId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };
            }

            if (status != "Confirmed" && status != "Canceled")
            {
                _logger.Warn($"DecideBookings failed: Invalid status={status} for BookingId={bookingId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidStatus };
            }

            bool updated = await _bookingRepository.DecideBookingStatusAsync(bookingId, status, ct);
            if (!updated)
            {
                _logger.Warn($"DecideBookings failed: Could not update BookingId={bookingId} to Status={status}");
                return new BaseResponse { Status = BaseResponseStatus.FailToUpdate };
            }

            _logger.Info($"DecideBookings succeeded. BookingId={bookingId}, Status={status}");
            return new BaseResponse { Status = BaseResponseStatus.Success };
        }

        catch (SqlException ex)
        {
            _logger.Error($"DecideBookings database error. BookingId={bookingId}, Status={status}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }

        catch (Exception ex)
        {
            _logger.Error($"DecideBookings unexpected error. BookingId={bookingId}, Status={status}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Get Calendar (Month)
    public virtual async Task<BaseResponse> GetMonthlyCalendarAsync(
    int businessId,
    int month,
    int? year,
    CancellationToken ct)
    {
        _logger.Info($"GetMonthlyCalendar started. BusinessId={businessId}, Month={month}, Year={year ?? DateTime.UtcNow.Year}");

        try
        {
            if (businessId <= 0 || month < 1 || month > 12)
            {
                _logger.Warn($"GetMonthlyCalendar failed: Invalid input. BusinessId={businessId}, Month={month}, Year={year}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            int checkYear = year ?? DateTime.UtcNow.Year;
            int daysInMonth = DateTime.DaysInMonth(checkYear, month);

            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
            {
                _logger.Warn($"GetMonthlyCalendar failed: BusinessId={businessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            DateTime fromUtc = new DateTime(checkYear, month, 1);
            DateTime toUtc = fromUtc.AddMonths(1).AddTicks(-1);

            List<BookingCountByDayModel> dbResult = await _bookingRepository.GetMonthlyCalendarAsync(businessId, fromUtc, toUtc, ct);
            if (dbResult.Count == 0)
            {
                _logger.Info($"GetMonthlyCalendar: No bookings found for BusinessId={businessId}, Month={month}, Year={checkYear}");
                return new BaseResponse { Status = BaseResponseStatus.Empty };
            }

            // Map results
            Dictionary<DateTime, int> lookup = dbResult.ToDictionary(b => b.BookingDate.Date, b => b.BookingCount);
            List<MonthlyCalendarModel> result = new List<MonthlyCalendarModel>(daysInMonth);

            for (int i = 0; i < daysInMonth; i++)
            {
                DateTime current = fromUtc.AddDays(i);
                lookup.TryGetValue(current.Date, out int count);

                result.Add(new MonthlyCalendarModel
                {
                    DateUtc = current,
                    BookingCount = count
                });
            }

            _logger.Info($"GetMonthlyCalendar succeeded. BusinessId={businessId}, Month={month}, Year={checkYear}, DaysWithBookings={result.Count(b => b.BookingCount > 0)}");
            return BaseResponse<List<MonthlyCalendarModel>>.Create(BaseResponseStatus.Success, result);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetMonthlyCalendar database error. BusinessId={businessId}, Month={month}, Year={year}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetMonthlyCalendar unexpected error. BusinessId={businessId}, Month={month}, Year={year}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Get Business By Id
    public virtual async Task<BaseResponse> GetBusinessByIdAsync(int businessId, CancellationToken ct)
    {
        _logger.Info($"GetBusinessById started. BusinessId={businessId}");

        try
        {
            if(businessId <= 0)
            {
                _logger.Warn($"GetBusinessById failed: Invalid BusinessId={businessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BusinessEntity? entity = await _businessRepository.GetByIdAsync(businessId, ct);
            if (entity == null)
            {
                _logger.Warn($"GetBusinessById failed: BusinessId={businessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            BusinessModel business = new BusinessModel
            {
                BusinessId = entity.BusinessId,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                TimeZoneIana = entity.TimeZoneIana,
                SettingsJson = entity.SettingsJson,
                CreatedAtUtc = entity.CreatedAtUtc
            };

            _logger.Info($"GetBusinessById succeeded. BusinessId={businessId}");
            return BaseResponse<BusinessModel>.Create(BaseResponseStatus.Success, business);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetBusinessById database error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetBusinessById unexpected error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    //Get Services By Business Id
    public virtual async Task<BaseResponse> GetServicesByBusinessIdAsync(
    int businessId,
    int skip = 0,
    int take = 10,
    CancellationToken ct = default)
    {
        _logger.Info($"GetServicesByBusinessId started. BusinessId={businessId}, Skip={skip}, Take={take}");

        try
        {
            if (businessId <= 0)
            {
                _logger.Warn($"GetServicesByBusinessId failed: Invalid input. BusinessId={businessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // Validate business exists
            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
            {
                _logger.Warn($"GetServicesByBusinessId failed: BusinessId={businessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            List<ServiceEntity> serviceEntities = await _serviceRepository.GetSeviceByBusinessIdAsync(businessId, skip, take, ct);
            if (serviceEntities.Count == 0)
            {
                _logger.Info($"GetServicesByBusinessId: No services found for BusinessId={businessId}");
                return new BaseResponse { Status = BaseResponseStatus.Empty };
            }

            List<ServiceModel> services = serviceEntities.Select(s => new ServiceModel
            {
                ServiceId = s.ServiceId,
                BusinessId = s.BusinessId,
                Name = s.Name,
                DurationMin = s.DurationMin,
                Price = s.Price,
                IsActive = s.IsActive
            }).ToList();

            _logger.Info($"GetServicesByBusinessId succeeded. BusinessId={businessId}, ServicesCount={services.Count}");
            return BaseResponse<List<ServiceModel>>.Create(BaseResponseStatus.Success, services);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetServicesByBusinessId database error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetServicesByBusinessId unexpected error. BusinessId={businessId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }
}