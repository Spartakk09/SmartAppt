using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Booking;
using Business.SmartAppt.Models.Service;

public interface IBusinessService
{
    Task<BaseResponse> CreateBusinessAsync(BusinessCreateRequestModel model, CancellationToken ct);
    Task<BaseResponse> DeleteBusinessAsync(int businessId, CancellationToken ct);
    Task<BaseResponse> UpdateBusinessByIdAsync(int businessId, BusinessCreateRequestModel model, CancellationToken ct);
    Task<BaseResponse> AddServicesAsync(ServiceCreateRequestModel model, CancellationToken ct);
    Task<BaseResponse> DeleteServiceAsync(int serviceId, CancellationToken ct);
    Task<BaseResponse> DisableServiceAsync(int serviceId, CancellationToken ct);
    Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct);
    Task<BaseResponse> GetDailyBookingsWithCustomersAsync(int businessId, DateTime dateUtc, CancellationToken ct);
    Task<BaseResponse> GetAllBookingsByBusinessIdAsync(int businessId, CancellationToken ct);
    Task<BaseResponse> DecideBookingsAsync(int bookingId, string status, CancellationToken ct);
    Task<BaseResponse> GetMonthlyCalendarAsync(int businessId, int month, int? year, CancellationToken ct);
}
