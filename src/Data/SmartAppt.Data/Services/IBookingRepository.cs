using Data.SmartAppt.SQL.Models;

namespace Data.SmartAppt.SQL.Services;

public interface IBookingRepository
{
    Task<List<BookingEntity?>> GetMyBookingsAsync(int customerId, CancellationToken ct);
    Task<int?> MakeBookingAsync(BookingEntity booking, int durationMin, CancellationToken ct);
    Task<bool> CancelBookingAsync(int bookingId, CancellationToken ct);
    Task<int?> UpdateBookingAsync(int oldBookingId, BookingEntity booking, int durationMin, CancellationToken ct);
    Task<BookingEntity?> GetByIdAsync(int bookingId, CancellationToken ct);
    Task<List<GetFreeSlotModel>> GetBookingsForDayAsync(int businessId, int serviceId, DateTime dateUtc, CancellationToken ct);
    Task<List<BookingWithCustomerDetailsModel>> GetDailyBookingsWithCustomersAsync(int businessId, DateTime dateUtc, CancellationToken ct);
    Task<List<BookingEntity>> GetAllBookingsByBusinessIdAsync(int businessId, CancellationToken ct);
    Task<bool> DecideBookingStatusAsync(int bookingId, string status, CancellationToken ct);
    Task<List<BookingCountByDayModel>> GetMonthlyCalendarAsync(int businessId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
}
