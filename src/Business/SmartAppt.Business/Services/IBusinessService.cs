using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;

public interface IBusinessService
{
    public Task<BaseResponse> CreateBusinessAsync(BusinessCreateRequestModel model, CancellationToken ct);
    public Task<BaseResponse> DeleteBusinessAsync(int businessId, CancellationToken ct);
    public Task<BaseResponse> UpdateBusinessByIdAsync(int businessId, BusinessCreateRequestModel model, CancellationToken ct);
    public Task<BaseResponse> AddServicesAsync(ServiceCreateRequestModel model, CancellationToken ct);
    public Task<BaseResponse> DeleteServiceAsync(int serviceId, CancellationToken ct);
    public Task<BaseResponse> DisableServiceAsync(int serviceId, CancellationToken ct);
    public Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct);
    public Task<BaseResponse> GetDailyBookingsWithCustomersAsync(int businessId, DateTime dateUtc, CancellationToken ct);
    public Task<BaseResponse> DecideBookingsAsync(int bookingId, string status, CancellationToken ct);
    public Task<BaseResponse> GetMonthlyCalendarAsync(int businessId, int month, int? year, CancellationToken ct);
    public Task<BaseResponse> GetBusinessByIdAsync(int businessId, CancellationToken ct);
    public Task<BaseResponse> GetServicesByBusinessIdAsync(int businessId, int skip = 0, int take = 10, CancellationToken ct = default);
    public Task<BaseResponse> GetBookingsAsync(int businessId, string? status, DateOnly? date, int skip = 0, int take = 50, CancellationToken ct = default);
}
