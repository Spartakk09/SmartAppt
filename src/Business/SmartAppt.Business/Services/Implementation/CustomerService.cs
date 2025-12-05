using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Booking;
using Business.SmartAppt.Models.Customer;
using Data.SmartAppt.SQL.Models;
using Data.SmartAppt.SQL.Services;
using Microsoft.Data.SqlClient;

namespace Business.SmartAppt.Services.Implementation;

public class CustomerService : ICustomerService
{
    protected readonly ICustomerRepository _customerRepository;
    protected readonly IBusinessRepository _businessRepository;
    protected readonly IServiceRepository _serviceRepository;
    protected readonly IBookingRepository _bookingRepository;
    protected readonly IOpeningHoursRepository _openingHoursRepository;
    protected readonly IHolidayRepository _holidayRepository;

    public CustomerService(ICustomerRepository customerRepository,
        IBusinessRepository businessRepository,
        IServiceRepository serviceRepository,
        IBookingRepository bookingRepository,
        IOpeningHoursRepository openingHoursRepository,
        IHolidayRepository holidayRepository)
    {
        _customerRepository = customerRepository;
        _businessRepository = businessRepository;
        _serviceRepository = serviceRepository;
        _bookingRepository = bookingRepository;
        _openingHoursRepository = openingHoursRepository;
        _holidayRepository = holidayRepository;
    }

    public virtual async Task<BaseResponse> CreateCustomerAsync(CreateCustomerRequestModel model, CancellationToken ct)
    {
        try
        {

            if (model.BusinessId <= 0 || string.IsNullOrWhiteSpace(model.FullName))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email.Length > 320)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (!string.IsNullOrWhiteSpace(model.Phone) && model.Phone.Length > 50)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            var businessExists = await _businessRepository.GetByIdAsync(model.BusinessId, ct);
            if (businessExists == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            CustomerEntity newCustomer = new CustomerEntity
            {
                BusinessId = model.BusinessId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone
            };

            int? newId = await _customerRepository.CreateAsync(newCustomer, ct);
            if (newId == null || newId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };

            return BaseResponse<int?>.Create(BaseResponseStatus.Success, newId);
        }
        catch (SqlException ex)
        {
            if (ex.Message.Contains("UQ__Customer__BB77B33CF09977B6", StringComparison.OrdinalIgnoreCase))
                return new BaseResponse { Status = BaseResponseStatus.AlreadyExists };

            return new BaseResponse { Status = BaseResponseStatus.DatabaseError };
        }
        catch (Exception)
        {
            return new BaseResponse { Status = BaseResponseStatus.UnknownError };
        }
    }

    public virtual async Task<BaseResponse> DeleteCustomerByIdAsync(int customerId, CancellationToken ct)
    {
        try
        {
            CustomerEntity? existingCustomer = await _customerRepository.GetByIdAsync(customerId, ct);
            if (existingCustomer == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

            await _customerRepository.DeleteAsync(customerId, ct);

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

    public virtual async Task<BaseResponse> GetCustomerByIdAsync(int customerId, CancellationToken ct)
    {
        try
        {
            CustomerEntity? entity = await _customerRepository.GetByIdAsync(customerId, ct);

            if (entity == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

            CustomerModel responseModel = new CustomerModel
            {
                CustomerId = entity.CustomerId,
                BusinessId = entity.BusinessId,
                FullName = entity.FullName,
                Email = entity.Email,
                Phone = entity.Phone,
                CreatedAtUtc = entity.CreatedAtUtc
            };

            return BaseResponse<CustomerModel>.Create(BaseResponseStatus.IsValid, responseModel);
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

    public virtual async Task<BaseResponse> UpdateCustomerAsync(UpdateCustomerRequestModel model, CancellationToken ct)
    {
        try
        {
            if (model.CustomerId <= 0 || string.IsNullOrWhiteSpace(model.FullName))
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            var existsStatus = await GetCustomerByIdAsync(model.CustomerId, ct);
            if (existsStatus.Status != BaseResponseStatus.IsValid)
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

            BusinessEntity? businessExists = await _businessRepository.GetByIdAsync(model.BusinessId, ct);
            if (businessExists == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            CustomerEntity entity = new CustomerEntity
            {
                CustomerId = model.CustomerId,
                BusinessId = model.BusinessId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone
            };

            await _customerRepository.UpdateAsync(entity, ct);

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

    public virtual async Task<BaseResponse> GetMyBookingsAsync(int customerId, CancellationToken ct)
    {
        try
        {
            CustomerEntity? customerExists = await _customerRepository.GetByIdAsync(customerId, ct);
            if (customerExists == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

            List<BookingEntity?> bookings = await _bookingRepository.GetMyBookingsAsync(customerId, ct);

            if (bookings == null || !bookings.Any())
                return new BaseResponse { Status = BaseResponseStatus.NotFound };

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

            return BaseResponse<ListOfBookings>.Create(BaseResponseStatus.Success, listOfBookings);
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

    public virtual async Task<BaseResponse> MakeBookingAsync(MakeBookingRequestModel booking, CancellationToken ct)
    {
        try
        {
            // --- Basic validation ---
            if (booking.BusinessId <= 0 || booking.ServiceId <= 0 || booking.CustomerId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            if (booking.StartAtUtc <= DateTime.UtcNow)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(booking.BusinessId, ct);
            if (business == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(booking.ServiceId, ct);
            if (service == null || service.BusinessId != booking.BusinessId)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            // --- Validate customer ---
            CustomerEntity? customer = await _customerRepository.GetByIdAsync(booking.CustomerId, ct);
            if (customer == null || customer.BusinessId != booking.BusinessId)
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

            // --- Check holiday ---
            bool isHoliday = await _holidayRepository.ExistsAsync(booking.BusinessId, booking.StartAtUtc, ct);
            if (isHoliday)
                return new BaseResponse { Status = BaseResponseStatus.Holiday };

            // --- Check opening hours and WeekDays---
            OpeningHoursEntity? openingHours = await _openingHoursRepository.GetByDateAsync(
                booking.BusinessId,
                booking.StartAtUtc,
                ct);

            if (openingHours == null)
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingHours };

            //Match booking time with opening hours
            TimeSpan startTime = booking.StartAtUtc.TimeOfDay;
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMin));

            if (startTime < openingHours.OpenTimeUtc || endTime > openingHours.CloseTimeUtc)
                return new BaseResponse { Status = BaseResponseStatus.Busy };

            BookingEntity newBooking = new BookingEntity
            {
                BusinessId = booking.BusinessId,
                ServiceId = booking.ServiceId,
                CustomerId = booking.CustomerId,
                StartAtUtc = booking.StartAtUtc,
                Notes = booking.Notes
            };

            int? bookingId = await _bookingRepository.MakeBookingAsync(newBooking, service.DurationMin, ct);

            if (bookingId == null || bookingId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.CreationFailed };

            return BaseResponse<int?>.Create(BaseResponseStatus.Success, bookingId);
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

    public virtual async Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        try
        {
            if (bookingId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            BookingEntity? existingBooking = await _bookingRepository.GetByIdAsync(bookingId, ct);
            if (existingBooking == null)
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

    public virtual async Task<BaseResponse> UpdateBookingAsync(int oldBookingId, MakeBookingRequestModel booking, CancellationToken ct)
    {
        try
        {
            // --- Basic validation ---
            if (oldBookingId <= 0 || booking.BusinessId <= 0 || booking.ServiceId <= 0 || booking.CustomerId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            // --- Validate old booking ---
            var oldBooking = await _bookingRepository.GetByIdAsync(oldBookingId, ct);
            if (oldBooking == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBooking };

            if (booking.StartAtUtc <= DateTime.UtcNow)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(booking.BusinessId, ct);
            if (business == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(booking.ServiceId, ct);
            if (service == null || service.BusinessId != booking.BusinessId)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            // --- Validate customer ---
            CustomerEntity? customer = await _customerRepository.GetByIdAsync(booking.CustomerId, ct);
            if (customer == null || customer.BusinessId != booking.BusinessId)
                return new BaseResponse { Status = BaseResponseStatus.InvalidCustomer };

            // --- Check holiday ---
            bool isHoliday = await _holidayRepository.ExistsAsync(booking.BusinessId, booking.StartAtUtc, ct);
            if (isHoliday)
                return new BaseResponse { Status = BaseResponseStatus.Holiday };

            // --- Check opening hours and WeekDays---
            OpeningHoursEntity? openingHours = await _openingHoursRepository.GetByDateAsync(
                booking.BusinessId,
                booking.StartAtUtc,
                ct);

            if (openingHours == null)
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingHours };

            TimeSpan startTime = booking.StartAtUtc.TimeOfDay;
            TimeSpan endTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMin));

            if (startTime < openingHours.OpenTimeUtc || endTime > openingHours.CloseTimeUtc)
                return new BaseResponse { Status = BaseResponseStatus.Busy };

            BookingEntity newBooking = new BookingEntity
            {
                BusinessId = booking.BusinessId,
                ServiceId = booking.ServiceId,
                CustomerId = booking.CustomerId,
                StartAtUtc = booking.StartAtUtc,
                Notes = booking.Notes
            };

            int? newBookingId = await _bookingRepository.UpdateBookingAsync(oldBookingId, newBooking, service.DurationMin, ct);

            if (newBookingId == null || newBookingId <= 0)
                return new BaseResponse { Status = BaseResponseStatus.Busy };

            return BaseResponse<int?>.Create(BaseResponseStatus.Success, newBookingId);
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

    public virtual async Task<BaseResponse> GetFreeSlotsForDayAsync(int businessId, int serviceId, DateTime dateUtc, CancellationToken ct)
    {
        try
        {
            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
                return new BaseResponse { Status = BaseResponseStatus.InvalidBusiness };

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (service == null || service.BusinessId != businessId)
                return new BaseResponse { Status = BaseResponseStatus.InvalidService };

            if (service.DurationMin <= 0)
                return new BaseResponse { Status = BaseResponseStatus.ValidationError };

            // Check holiday
            if (await _holidayRepository.ExistsAsync(businessId, dateUtc, ct))
                return new BaseResponse { Status = BaseResponseStatus.Holiday };

            // Get opening hours
            OpeningHoursEntity? openingHours = await _openingHoursRepository.GetByDateAsync(businessId, dateUtc, ct);
            if (openingHours == null)
                return new BaseResponse { Status = BaseResponseStatus.NoWorkingHours };

            // Get booked slots
            List<GetFreeSlotModel> bookedSlots = await _bookingRepository
                .GetBookingsForDayAsync(businessId, serviceId, dateUtc, ct);

            var freeSlots = new List<FreeSlotResponseModel>();

            TimeSpan slotStart = openingHours.OpenTimeUtc;
            TimeSpan slotEndBoundary = openingHours.CloseTimeUtc;
            TimeSpan duration = TimeSpan.FromMinutes(service.DurationMin);

            // Generate free slots
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

            //Is there any free slot for per day 
            if (freeSlots.Count == 0)
                return new BaseResponse { Status = BaseResponseStatus.Busy };

            return BaseResponse<List<FreeSlotResponseModel>>.Create(BaseResponseStatus.Success, freeSlots);
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

    public virtual async Task<BaseResponse> HasFreeSlotsForMonthAsync(int businessId, int serviceId, int month, int? year, CancellationToken ct)
    {
        try
        {
            // --- Validate business ---
            BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
            if (business == null)
                return BaseResponse<bool>.Create(BaseResponseStatus.InvalidBusiness, false);

            // --- Validate service ---
            ServiceEntity? service = await _serviceRepository.GetByIdAsync(serviceId, ct);
            if (service == null || service.BusinessId != businessId)
                return BaseResponse<bool>.Create(BaseResponseStatus.InvalidService, false);


            if (month < 1 || month > 12)
                return BaseResponse<bool>.Create(BaseResponseStatus.ValidationError, false);

            int checkYear = year ?? DateTime.UtcNow.Year;

            int daysInMonth = DateTime.DaysInMonth(checkYear, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(checkYear, month, day);

                // Check free slots for the day
                var freeSlotsResult = await GetFreeSlotsForDayAsync(businessId, serviceId, currentDate, ct);

                // If any free slot exists, return true immediately
                if (freeSlotsResult.Status == BaseResponseStatus.Success)
                {
                    return BaseResponse<bool>.Create(BaseResponseStatus.Success, true);
                }
            }

            // No free slots found in the whole month
            return BaseResponse<bool>.Create(BaseResponseStatus.Busy, false);
        }
        catch (SqlException)
        {
            return BaseResponse<bool>.Create(BaseResponseStatus.DatabaseError, false);
        }
        catch (Exception)
        {
            return BaseResponse<bool>.Create(BaseResponseStatus.UnknownError, false);
        }
    }

    public virtual async Task<BaseResponse> HasFreeSlotsForYearAsync(int businessId, int serviceId, int year, CancellationToken ct)
    {
        // --- Validate business ---
        BusinessEntity? business = await _businessRepository.GetByIdAsync(businessId, ct);
        if (business == null)
            return BaseResponse<bool>.Create(BaseResponseStatus.InvalidBusiness, false);

        // --- Validate service ---
        ServiceEntity? service = await _serviceRepository.GetByIdAsync(serviceId, ct);
        if (service == null || service.BusinessId != businessId)
            return BaseResponse<bool>.Create(BaseResponseStatus.InvalidService, false);

        if (year < 1 || year > 9999)
            return BaseResponse<bool>.Create(BaseResponseStatus.ValidationError, false);

        for (int month = 1; month <= 12; month++)
        {
            var monthResult = await HasFreeSlotsForMonthAsync(businessId, serviceId, month, year, ct);

            if (monthResult.Status == BaseResponseStatus.Success)
            {
                return BaseResponse<bool>.Create(BaseResponseStatus.Success, true);
            }
        }

        return BaseResponse<bool>.Create(BaseResponseStatus.Success, true);
    }
}
