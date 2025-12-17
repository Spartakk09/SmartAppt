using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.SmartAppt.API.Pages;
public class IndexModel : PageModel
{
    private readonly IBusinessService _businessService;

    public IndexModel(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [BindProperty]
    public int BusinessId { get; set; }

    public int TodayBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CanceledToday { get; set; }

    public string? Message { get; set; }

    public async Task OnGetAsync()
    {
        if (BusinessId > 0)
        {
            await LoadMetricsAsync();
        }
    }

    private async Task LoadMetricsAsync()
    {
        // Load Today Bookings
        var today = DateTime.UtcNow.Date;

        var todayResp = await _businessService.GetBookingsAsync(BusinessId, null, DateOnly.FromDateTime(today), 0, 1000, HttpContext.RequestAborted);
        TodayBookings = todayResp.Status == BaseResponseStatus.Success && todayResp is BaseResponse<List<BookingModel>> tList && tList.Data != null
            ? tList.Data.Count
            : 0;

        // Pending Bookings
        var pendingResp = await _businessService.GetBookingsAsync(BusinessId, "Pending", null, 0, 1000, HttpContext.RequestAborted);
        PendingBookings = pendingResp.Status == BaseResponseStatus.Success && pendingResp is BaseResponse<List<BookingModel>> pList && pList.Data != null
            ? pList.Data.Count
            : 0;

        // Canceled today
        var canceledResp = await _businessService.GetBookingsAsync(BusinessId, "Canceled", DateOnly.FromDateTime(today), 0, 1000, HttpContext.RequestAborted);
        CanceledToday = canceledResp.Status == BaseResponseStatus.Success && canceledResp is BaseResponse<List<BookingModel>> cList && cList.Data != null
            ? cList.Data.Count
            : 0;
    }
}
