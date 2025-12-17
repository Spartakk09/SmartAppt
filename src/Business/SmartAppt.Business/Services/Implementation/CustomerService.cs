using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;
using Business.SmartAppt.Models.Customer;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using Microsoft.Data.SqlClient;
using SmartAppt.Common.Logging;

namespace Business.SmartAppt.Services.Implementation;

public class CustomerService : ICustomerService
{
    protected readonly ICustomerRepository _customerRepository;
    protected readonly IBusinessRepository _businessRepository;
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IBookingRepository _bookingRepository;
    protected readonly IOpeningHoursRepository _openingHoursRepository;
    protected readonly IHolidayRepository _holidayRepository;
    private readonly IAppLogger<CustomerService> _logger;

    public CustomerService(ICustomerRepository customerRepository,
        IBusinessRepository businessRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository,
        IOpeningHoursRepository openingHoursRepository,
        IHolidayRepository holidayRepository,
        IAppLogger<CustomerService> logger)

    {
        _customerRepository = customerRepository;
        _businessRepository = businessRepository;
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
        _openingHoursRepository = openingHoursRepository;
        _holidayRepository = holidayRepository;
        _logger = logger;
    }

    public virtual async Task<BaseResponse> CreateCustomerAsync(
    CreateCustomerRequestModel model,
    CancellationToken ct)
    {
        _logger.Info($"CreateCustomer started. BusinessId={model.BusinessId}, FullName={model.FullName}");

        try
        {
            // Input validation
            if (model.BusinessId <= 0 || string.IsNullOrWhiteSpace(model.FullName))
            {
                _logger.Warn($"CreateCustomer failed: Invalid input. BusinessId={model.BusinessId}, FullName={model.FullName}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email.Length > 320)
            {
                _logger.Warn($"CreateCustomer failed: Email too long. Email={model.Email}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            if (!string.IsNullOrWhiteSpace(model.Phone) && model.Phone.Length > 50)
            {
                _logger.Warn($"CreateCustomer failed: Phone too long. Phone={model.Phone}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // Check if business exists
            BusinessEntity? businessExists = await _businessRepository.GetByIdAsync(model.BusinessId, ct);
            if (businessExists == null)
            {
                _logger.Warn($"CreateCustomer failed: BusinessId={model.BusinessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            // Create customer entity
            CustomerEntity newCustomer = new CustomerEntity
            {
                BusinessId = model.BusinessId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone
            };

            int? newId = await _customerRepository.CreateAsync(newCustomer, ct);
            if (newId == null || newId <= 0)
            {
                _logger.Warn($"CreateCustomer failed: Could not create customer for BusinessId={model.BusinessId}");
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };
            }

            _logger.Info($"CreateCustomer succeeded. CustomerId={newId.Value}, BusinessId={model.BusinessId}");
            return BaseResponse<int?>.Create(BaseResponseStatus.Success, newId);
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("UQ__Customer__BB77B33CF09977B6", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Warn($"CreateCustomer failed: Customer already exists. BusinessId={model.BusinessId}, FullName={model.FullName}");
                return new BaseResponse { Status = BaseResponseStatus.AlreadyExists };
            }

            _logger.Error($"CreateCustomer database error. BusinessId={model.BusinessId}, FullName={model.FullName}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"CreateCustomer unexpected error. BusinessId={model.BusinessId}, FullName={model.FullName}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> DeleteCustomerByIdAsync(
     int customerId,
     CancellationToken ct)
    {
        _logger.Info($"DeleteCustomer started. CustomerId={customerId}");

        try
        {
            if (customerId <= 0)
            {
                _logger.Warn($"DeleteCustomer failed: Invalid CustomerId={customerId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            CustomerEntity? existingCustomer = await _customerRepository.GetByIdAsync(customerId, ct);
            if (existingCustomer == null)
            {
                _logger.Warn($"DeleteCustomer failed: CustomerId={customerId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };
            }

            await _customerRepository.DeleteAsync(customerId, ct);

            _logger.Info($"DeleteCustomer succeeded. CustomerId={customerId}");
            return new BaseResponse { Status = BaseResponseStatus.Success };
        }

        catch (SqlException ex)
        {
            _logger.Error($"DeleteCustomer database error. CustomerId={customerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }

        catch (Exception ex)
        {
            _logger.Error($"DeleteCustomer unexpected error. CustomerId={customerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }


    public virtual async Task<BaseResponse> GetCustomerByIdAsync(
     int customerId,
     CancellationToken ct)
    {
        _logger.Info($"GetCustomerById started. CustomerId={customerId}");

        try
        {
            CustomerEntity? entity = await _customerRepository.GetByIdAsync(customerId, ct);
            if (entity == null)
            {
                _logger.Warn($"GetCustomerById failed: CustomerId={customerId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };
            }

            CustomerModel responseModel = new CustomerModel
            {
                CustomerId = entity.CustomerId,
                BusinessId = entity.BusinessId,
                FullName = entity.FullName,
                Email = entity.Email,
                Phone = entity.Phone,
                CreatedAtUtc = entity.CreatedAtUtc
            };

            _logger.Info($"GetCustomerById succeeded. CustomerId={customerId}");
            return BaseResponse<CustomerModel>.Create(BaseResponseStatus.IsValid, responseModel);
        }

        catch (SqlException ex)
        {
            _logger.Error($"GetCustomerById database error. CustomerId={customerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }

        catch (Exception ex)
        {
            _logger.Error($"GetCustomerById unexpected error. CustomerId={customerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> UpdateCustomerAsync(
    UpdateCustomerRequestModel model,
    CancellationToken ct)
    {
        _logger.Info($"UpdateCustomer started. CustomerId={model.CustomerId}, BusinessId={model.BusinessId}");

        try
        {
            if (model.CustomerId <= 0 || string.IsNullOrWhiteSpace(model.FullName))
            {
                _logger.Warn($"UpdateCustomer failed: Invalid input. CustomerId={model.CustomerId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BaseResponse existsStatus = await GetCustomerByIdAsync(model.CustomerId, ct);
            if (existsStatus.Status != BaseResponseStatus.IsValid)
            {
                _logger.Warn($"UpdateCustomer failed: CustomerId={model.CustomerId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };
            }

            BusinessEntity? businessExists = await _businessRepository.GetByIdAsync(model.BusinessId, ct);

            if (businessExists == null)
            {
                _logger.Warn($"UpdateCustomer failed: BusinessId={model.BusinessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            _logger.Debug(
                $"UpdateCustomer request data: CustomerId={model.CustomerId}, " +
                $"FullName={model.FullName}, Email={model.Email}, Phone={model.Phone}");

            CustomerEntity entity = new CustomerEntity
            {
                CustomerId = model.CustomerId,
                BusinessId = model.BusinessId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone
            };

            await _customerRepository.UpdateAsync(entity, ct);

            _logger.Info($"UpdateCustomer succeeded. CustomerId={model.CustomerId}");

            return new BaseResponse { Status = BaseResponseStatus.Success };
        }

        catch (SqlException ex)
        {
            _logger.Error($"UpdateCustomer database error. CustomerId={model.CustomerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }

        catch (Exception ex)
        {
            _logger.Error($"UpdateCustomer unexpected error. CustomerId={model.CustomerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }


    public virtual async Task<BaseResponse> GetMyBookingsAsync(
        int customerId,
        CancellationToken ct)
    {
        _logger.Info($"GetMyBookings started. CustomerId={customerId}");

        try
        {
            if (customerId <= 0)
            {
                _logger.Warn($"GetMyBookings failed: Invalid CustomerId={customerId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            CustomerEntity? customerExists =
                await _customerRepository.GetByIdAsync(customerId, ct);

            if (customerExists == null)
            {
                _logger.Warn($"GetMyBookings failed: CustomerId={customerId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };
            }

            List<BookingEntity?> bookings =
                await _bookingRepository.GetMyBookingsAsync(customerId, ct);

            if (bookings == null || !bookings.Any())
            {
                _logger.Info($"GetMyBookings: No bookings found for CustomerId={customerId}");
                return new BaseResponse { Status = BaseResponseStatus.NotFound };
            }

            ListOfBookings listOfBookings = new ListOfBookings
            {
                Bookings = bookings.Select(b => new BookingModel
                {
                    BookingId = b.BookingId,
                    BusinessId = b.BusinessId,
                    ServiceId = b.ServiceId,
                    CustomerId = b.CustomerId,
                    StartAtUtc = b.StartAtUtc,
                    EndAtUtc = b.EndAtUtc,
                    Status = b.Status,
                    Notes = b.Notes,
                    CreatedAtUtc = b.CreatedAtUtc
                }).ToList()
            };

            _logger.Info(
                $"GetMyBookings succeeded. CustomerId={customerId}, BookingsCount={listOfBookings.Bookings.Count}");

            return BaseResponse<ListOfBookings>.Create(
                BaseResponseStatus.Success,
                listOfBookings);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetMyBookings database error. CustomerId={customerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetMyBookings unexpected error. CustomerId={customerId}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> MakeBookingAsync(
    MakeBookingRequestModel booking,
    CancellationToken ct)
    {
        _logger.Info(
            $"MakeBooking started. BusinessId={booking.BusinessId}, ServiceId={booking.ServiceId}, CustomerId={booking.CustomerId}, StartAtUtc={booking.StartAtUtc:O}");

        try
        {
            // --- Basic validation ---
            if (booking.BusinessId <= 0 || booking.ServiceId <= 0 || booking.CustomerId <= 0)
            {
                _logger.Warn("MakeBooking failed: Invalid input IDs");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            if (booking.StartAtUtc <= DateTime.UtcNow)
            {
                _logger.Warn($"MakeBooking failed: StartAtUtc={booking.StartAtUtc:O} is in the past");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(booking.BusinessId, ct);
            if (business == null)
            {
                _logger.Warn($"MakeBooking failed: BusinessId={booking.BusinessId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(booking.ServiceId, ct);

            if (service == null || service.BusinessId != booking.BusinessId)
            {
                _logger.Warn($"MakeBooking failed: Invalid service. ServiceId={booking.ServiceId}, BusinessId={booking.BusinessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };
            }

            // --- Validate customer ---
            CustomerEntity? customer = await _customerRepository.GetByIdAsync(booking.CustomerId, ct);
            if (customer == null || customer.BusinessId != booking.BusinessId)
            {
                _logger.Warn($"MakeBooking failed: Invalid customer. CustomerId={booking.CustomerId}, BusinessId={booking.BusinessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };
            }

            // --- Check holiday ---
            bool isHoliday = await _holidayRepository.ExistsAsync(booking.BusinessId, booking.StartAtUtc, ct);
            if (isHoliday)
            {
                _logger.Warn($"MakeBooking failed: Holiday. BusinessId={booking.BusinessId}, Date={booking.StartAtUtc:yyyy-MM-dd}");
                return new BaseResponse { Status = BaseResponseStatus.Holiday };
            }

            // --- Check opening hours ---
            OpeningHoursEntity? openingHours =
                await _openingHoursRepository.GetByDateAsync(
                    booking.BusinessId,
                    booking.StartAtUtc,
                    ct);

            if (openingHours == null)
            {
                _logger.Warn($"MakeBooking failed: No working day. BusinessId={booking.BusinessId}, Date={booking.StartAtUtc:yyyy-MM-dd}");
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingDay };
            }

            TimeSpan startTime = booking.StartAtUtc.TimeOfDay;
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMin));

            if (startTime < openingHours.OpenTimeUtc || endTime > openingHours.CloseTimeUtc)
            {
                _logger.Warn($"MakeBooking failed: Outside working hours. Start={startTime}, End={endTime}, Open={openingHours.OpenTimeUtc}, Close={openingHours.CloseTimeUtc}");
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingHours };
            }

            BookingEntity newBooking = new BookingEntity
            {
                BusinessId = booking.BusinessId,
                ServiceId = booking.ServiceId,
                CustomerId = booking.CustomerId,
                StartAtUtc = booking.StartAtUtc,
                Notes = booking.Notes
            };

            int? bookingId = await _bookingRepository.MakeBookingAsync(newBooking, service.DurationMin, ct);
            if (bookingId == null)
            {
                _logger.Warn($"MakeBooking failed: Time slot busy. BusinessId={booking.BusinessId}, StartAtUtc={booking.StartAtUtc:O}");
                return new BaseResponse { Status = BaseResponseStatus.Busy };
            }

            _logger.Info($"MakeBooking succeeded. BookingId={bookingId.Value}, BusinessId={booking.BusinessId}");

            return BaseResponse<int?>.Create(BaseResponseStatus.Success, bookingId);
        }
        catch (SqlException ex)
        {
            _logger.Error($"MakeBooking database error. BusinessId={booking.BusinessId}, StartAtUtc={booking.StartAtUtc:O}", ex);

            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"MakeBooking unexpected error. BusinessId={booking.BusinessId}",ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        _logger.Info($"CancelBooking started. BookingId={bookingId}");

        try
        {
            if (bookingId <= 0)
            {
                _logger.Warn($"CancelBooking failed: Invalid BookingId={bookingId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            BookingEntity? existingBooking = await _bookingRepository.GetByIdAsync(bookingId, ct);
            if (existingBooking == null)
            {
                _logger.Warn($"CancelBooking failed: BookingId={bookingId} not found");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };
            }

            bool success = await _bookingRepository.CancelBookingAsync(bookingId, ct);

            if (!success)
            {
                _logger.Warn($"CancelBooking failed: BookingId={bookingId} already canceled");
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

    public virtual async Task<BaseResponse> UpdateBookingAsync(int oldBookingId, MakeBookingRequestModel booking, CancellationToken ct)
    {
        _logger.Info($"UpdateBooking started. OldBookingId={oldBookingId}, CustomerId={booking.CustomerId}, BusinessId={booking.BusinessId}");

        try
        {
            // --- Basic validation ---
            if (oldBookingId <= 0 || booking.BusinessId <= 0 || booking.ServiceId <= 0 || booking.CustomerId <= 0)
            {
                _logger.Warn("UpdateBooking failed: Invalid input");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // --- Validate old booking ---
            BookingEntity? oldBooking = await _bookingRepository.GetByIdAsync(oldBookingId, ct);
            if (oldBooking == null)
            {
                _logger.Warn($"UpdateBooking failed: Old booking not found. OldBookingId={oldBookingId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };
            }

            if (booking.StartAtUtc <= DateTime.UtcNow)
            {
                _logger.Warn("UpdateBooking failed: StartAtUtc is in the past");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(booking.BusinessId, ct);
            if (business == null)
            {
                _logger.Warn($"UpdateBooking failed: Business not found. BusinessId={booking.BusinessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(booking.ServiceId, ct);
            if (service == null || service.BusinessId != booking.BusinessId)
            {
                _logger.Warn($"UpdateBooking failed: Service invalid. ServiceId={booking.ServiceId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };
            }

            // --- Validate customer ---
            CustomerEntity? customer = await _customerRepository.GetByIdAsync(booking.CustomerId, ct);
            if (customer == null || customer.BusinessId != booking.BusinessId)
            {
                _logger.Warn($"UpdateBooking failed: Customer invalid. CustomerId={booking.CustomerId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };
            }

            // --- Check holiday ---
            bool isHoliday = await _holidayRepository.ExistsAsync(booking.BusinessId, booking.StartAtUtc, ct);
            if (isHoliday)
            {
                _logger.Warn($"UpdateBooking failed: Date is a holiday. StartAtUtc={booking.StartAtUtc}");
                return new BaseResponse { Status = BaseResponseStatus.Holiday };
            }

            // --- Check opening hours and weekdays ---
            OpeningHoursEntity? openingHours = await _openingHoursRepository.GetByDateAsync(booking.BusinessId, booking.StartAtUtc, ct);
            if (openingHours == null)
            {
                _logger.Warn($"UpdateBooking failed: No working day. StartAtUtc={booking.StartAtUtc}");
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingDay };
            }

            TimeSpan startTime = booking.StartAtUtc.TimeOfDay;
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMin));

            if (startTime < openingHours.OpenTimeUtc || endTime > openingHours.CloseTimeUtc)
            {
                _logger.Warn($"UpdateBooking failed: Outside working hours. Start={startTime}, End={endTime}");
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingHours };
            }

            _logger.Debug($"UpdateBooking request data: {System.Text.Json.JsonSerializer.Serialize(booking)}");

            BookingEntity newBooking = new BookingEntity
            {
                BusinessId = booking.BusinessId,
                ServiceId = booking.ServiceId,
                CustomerId = booking.CustomerId,
                StartAtUtc = booking.StartAtUtc,
                Notes = booking.Notes
            };

            int? newBookingId = await _bookingRepository.UpdateBookingAsync(oldBookingId, newBooking, service.DurationMin, ct);

            if (newBookingId == null)
            {
                _logger.Warn("UpdateBooking failed: Time slot busy");
                return new BaseResponse { Status = BaseResponseStatus.Busy };
            }

            _logger.Info($"UpdateBooking succeeded. NewBookingId={newBookingId}");
            return BaseResponse<int?>.Create(BaseResponseStatus.Success, newBookingId);
        }
        catch (SqlException ex)
        {
            _logger.Error("UpdateBooking database error", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateBooking unexpected error", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> GetFreeSlotsForDayAsync(int businessId, int serviceId, DateTime dateUtc, CancellationToken ct)
    {
        _logger.Info($"GetFreeSlotsForDay started. BusinessId={businessId}, ServiceId={serviceId}, Date={dateUtc:yyyy-MM-dd}");

        try
        {
            if (businessId <= 0 || serviceId <= 0 || dateUtc < DateTime.UtcNow.Date)
            {
                _logger.Warn("GetFreeSlotsForDay failed: Invalid input");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
            {
                _logger.Warn($"GetFreeSlotsForDay failed: Business not found. BusinessId={businessId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };
            }

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (service == null || service.BusinessId != businessId)
            {
                _logger.Warn($"GetFreeSlotsForDay failed: Service invalid. ServiceId={serviceId}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };
            }

            if (service.DurationMin <= 0)
            {
                _logger.Warn($"GetFreeSlotsForDay failed: Service has invalid duration. ServiceId={serviceId}, DurationMin={service.DurationMin}");
                return new BaseResponse { Status = BaseResponseStatus.InvalidInput };
            }

            // --- Check holiday ---
            if (await _holidayRepository.ExistsAsync(businessId, dateUtc, ct))
            {
                _logger.Warn($"GetFreeSlotsForDay: Date is a holiday. Date={dateUtc:yyyy-MM-dd}");
                return new BaseResponse { Status = BaseResponseStatus.Holiday };
            }

            // --- Get opening hours ---
            OpeningHoursEntity? openingHours = await _openingHoursRepository.GetByDateAsync(businessId, dateUtc, ct);
            if (openingHours == null)
            {
                _logger.Warn($"GetFreeSlotsForDay: No working day. Date={dateUtc:yyyy-MM-dd}");
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingDay };
            }

            // --- Get booked slots ---
            List<GetFreeSlotModel> bookedSlots = await _bookingRepository.GetBookingsForDayAsync(businessId, serviceId, dateUtc, ct);

            var freeSlots = new List<FreeSlotResponseModel>();
            TimeSpan slotStart = openingHours.OpenTimeUtc;
            TimeSpan slotEndBoundary = openingHours.CloseTimeUtc;
            TimeSpan duration = TimeSpan.FromMinutes(service.DurationMin);

            // --- Generate free slots ---
            while (slotStart + duration <= slotEndBoundary)
            {
                TimeSpan slotEnd = slotStart + duration;

                bool overlaps = bookedSlots.Any(b =>
                    slotStart < b.End.TimeOfDay &&
                    slotEnd > b.Start.TimeOfDay);

                if (!overlaps)
                {
                    freeSlots.Add(new FreeSlotResponseModel
                    {
                        Start = slotStart,
                        End = slotEnd
                    });
                }

                slotStart += duration;
            }

            if (freeSlots.Count == 0)
            {
                _logger.Info($"GetFreeSlotsForDay: No free slots available. BusinessId={businessId}, Date={dateUtc:yyyy-MM-dd}");
                return new BaseResponse { Status = BaseResponseStatus.Busy };
            }

            _logger.Info($"GetFreeSlotsForDay succeeded. BusinessId={businessId}, Date={dateUtc:yyyy-MM-dd}, FreeSlotsCount={freeSlots.Count}");
            return BaseResponse<List<FreeSlotResponseModel>>.Create(BaseResponseStatus.Success, freeSlots);
        }
        catch (SqlException ex)
        {
            _logger.Error($"GetFreeSlotsForDay database error. BusinessId={businessId}, ServiceId={serviceId}, Date={dateUtc:yyyy-MM-dd}", ex);
            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception ex)
        {
            _logger.Error($"GetFreeSlotsForDay unexpected error. BusinessId={businessId}, ServiceId={serviceId}, Date={dateUtc:yyyy-MM-dd}", ex);
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public async Task<BaseResponse> HasFreeSlotsForMonthAsync(int businessId, int serviceId, int month, int? year, CancellationToken ct)
    {
        _logger.Info($"HasFreeSlotsForMonth started. BusinessId={businessId}, ServiceId={serviceId}, Month={month}, Year={year ?? DateTime.UtcNow.Year}");

        try
        {
            if (month < 1 || month > 12 || (year.HasValue && year < DateTime.UtcNow.Year))
            {
                _logger.Warn("HasFreeSlotsForMonth failed: Invalid input");
                return BaseResponse<bool>.Create(BaseResponseStatus.InvalidInput, false);
            }

            int checkYear = year ?? DateTime.UtcNow.Year;

            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
            {
                _logger.Warn($"HasFreeSlotsForMonth failed: Business not found. BusinessId={businessId}");
                return BaseResponse<bool>.Create(BaseResponseStatus.InvalidBusiness, false);
            }

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (service == null || service.BusinessId != businessId)
            {
                _logger.Warn($"HasFreeSlotsForMonth failed: Service invalid. ServiceId={serviceId}");
                return BaseResponse<bool>.Create(BaseResponseStatus.InvalidService, false);
            }

            if (service.DurationMin <= 0)
            {
                _logger.Warn($"HasFreeSlotsForMonth failed: Service has invalid duration. ServiceId={serviceId}, DurationMin={service.DurationMin}");
                return BaseResponse<bool>.Create(BaseResponseStatus.InvalidInput, false);
            }

            TimeSpan duration = TimeSpan.FromMinutes(service.DurationMin);
            DateTime monthStart = new DateTime(checkYear, month, 1);
            DateTime monthEnd = monthStart.AddMonths(1);
            int daysInMonth = DateTime.DaysInMonth(checkYear, month);

            List<HolidayEntity> holidays = await _holidayRepository.GetInRangeAsync(businessId, monthStart, monthEnd, ct);
            List<OpeningHoursEntity> openingHours = await _openingHoursRepository.GetOpeningHoursAsync(businessId, ct);

            HashSet<DateTime> holidaySet = holidays.Select(h => h.HolidayDate.Date).ToHashSet();
            Dictionary<DayOfWeek, OpeningHoursEntity> openingDict = openingHours.ToDictionary(o => o.DayOfWeek, o => o);

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(checkYear, month, day);

                if (holidaySet.Contains(date))
                    continue;

                if (!openingDict.TryGetValue(date.DayOfWeek, out var oh))
                    continue;

                TimeSpan open = oh.OpenTimeUtc;
                TimeSpan close = oh.CloseTimeUtc;

                // Query bookings for the day
                List<GetFreeSlotModel> dayBookings = await _bookingRepository.GetBookingsForDayAsync(businessId, serviceId, date, ct);

                TimeSpan slotStart = open;
                TimeSpan slotEndLimit = close;

                while (slotStart + duration <= slotEndLimit)
                {
                    TimeSpan slotEnd = slotStart + duration;

                    bool overlaps = dayBookings.Any(b =>
                    {
                        TimeSpan bStart = b.Start.TimeOfDay;
                        TimeSpan bEnd = b.End.TimeOfDay;
                        return slotStart < bEnd && slotEnd > bStart;
                    });

                    if (!overlaps)
                    {
                        _logger.Info($"HasFreeSlotsForMonth: Found free slot on {date:yyyy-MM-dd}");
                        return BaseResponse<bool>.Create(BaseResponseStatus.Success, true);
                    }

                    slotStart += duration;
                }
            }

            _logger.Info($"HasFreeSlotsForMonth: No free slots for BusinessId={businessId}, ServiceId={serviceId}, Month={month}, Year={checkYear}");
            return BaseResponse<bool>.Create(BaseResponseStatus.Busy, false);
        }
        catch (SqlException ex)
        {
            _logger.Error($"HasFreeSlotsForMonth database error. BusinessId={businessId}, ServiceId={serviceId}, Month={month}, Year={year}", ex);
            return BaseResponse<bool>.Create(BaseResponseStatus.DatabaseError, false);
        }
        catch (Exception ex)
        {
            _logger.Error($"HasFreeSlotsForMonth unexpected error. BusinessId={businessId}, ServiceId={serviceId}, Month={month}, Year={year}", ex);
            return BaseResponse<bool>.Create(BaseResponseStatus.UnknownError, false);
        }
    }
}
