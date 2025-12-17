using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Customer;

namespace Business.SmartAppt.Services;

public interface ICustomerService
{
    Task<BaseResponse> CreateCustomerAsync(CreateCustomerRequestModel model, CancellationToken ct);
    Task<BaseResponse> UpdateCustomerAsync(UpdateCustomerRequestModel model, CancellationToken ct);
    Task<BaseResponse> GetCustomerByIdAsync(int customerId, CancellationToken ct);
    Task<BaseResponse> DeleteCustomerByIdAsync(int customerId, CancellationToken ct);
    Task<BaseResponse> GetMyBookingsAsync(int customerId, CancellationToken ct);
    Task<BaseResponse> MakeBookingAsync(MakeBookingRequestModel booking, CancellationToken ct);
    Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct);
    Task<BaseResponse> UpdateBookingAsync(int oldBookingId, MakeBookingRequestModel newBooking, CancellationToken ct);
    Task<BaseResponse> GetFreeSlotsForDayAsync(int businessId, int serviceId, DateTime date, CancellationToken ct);
    Task<BaseResponse> HasFreeSlotsForMonthAsync(int businessId, int serviceId, int month, int? year, CancellationToken ct);
}
