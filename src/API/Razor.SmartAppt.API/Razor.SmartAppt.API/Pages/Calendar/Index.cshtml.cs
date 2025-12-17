using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.SmartAppt.API.Pages.Calendar
{
    public class IndexModel : PageModel
    {
        private readonly IBusinessService _businessService;

        public IndexModel(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        // ===================== BIND PROPS =====================

        [BindProperty]
        public int BusinessId { get; set; }

        [BindProperty]
        public int Month { get; set; } = DateTime.UtcNow.Month;

        [BindProperty]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [BindProperty]
        public int? SelectedDay { get; set; }

        public List<MonthlyCalendarModel>? MonthlyCalendar { get; set; }

        public List<DailyBookingResponseModel>? DailyBookings { get; set; }

        public string? Message { get; set; }

        public void OnGet() { }

        // ===================== LOAD CALENDAR =====================

        public async Task<IActionResult> OnPostLoadCalendarAsync()
        {
            if (BusinessId <= 0 || Month < 1 || Month > 12 || Year < 1)
            {
                Message = "Invalid calendar input.";
                return Page();
            }

            await LoadCalendarAsync();

            SelectedDay = null;
            DailyBookings = null;

            return Page();
        }

        // ===================== LOAD DAILY BOOKINGS =====================

        public async Task<IActionResult> OnPostLoadBookingsAsync(int day)
        {
            if (BusinessId <= 0)
            {
                Message = "Invalid BusinessId.";
                return Page();
            }

            SelectedDay = day;

            await LoadCalendarAsync();
            await LoadDailyBookingsAsync(day);

            return Page();
        }

        // ===================== CANCEL BOOKING =====================

        public async Task<IActionResult> OnPostCancelBookingAsync(int bookingId, int day)
        {
            if (BusinessId <= 0 || bookingId <= 0)
            {
                Message = "Invalid cancel request.";
                return Page();
            }

            var result = await _businessService.CancelBookingAsync(
                bookingId,
                HttpContext.RequestAborted);

            Message = $"Cancel Result: {result.Status}";

            SelectedDay = day;

            await LoadCalendarAsync();
            await LoadDailyBookingsAsync(day);

            return Page();
        }

        // ===================== PRIVATE LOADERS =====================

        private async Task LoadCalendarAsync()
        {
            var response = await _businessService.GetMonthlyCalendarAsync(
                BusinessId, Month, Year, HttpContext.RequestAborted);

            if (response.Status == BaseResponseStatus.Success)
            {
                MonthlyCalendar =
                    (response as BaseResponse<List<MonthlyCalendarModel>>)?.Data;
            }
            else
            {
                MonthlyCalendar = null;
                Message = $"Calendar load failed: {response.Status}";
            }
        }

        private async Task LoadDailyBookingsAsync(int day)
        {
            var dateUtc = new DateTime(Year, Month, day);

            var response = await _businessService.GetDailyBookingsWithCustomersAsync(
                BusinessId,
                dateUtc,
                HttpContext.RequestAborted);

            if (response.Status == BaseResponseStatus.Success)
            {
                DailyBookings =
                    (response as BaseResponse<List<DailyBookingResponseModel>>)?.Data;
            }
            else
            {
                DailyBookings = null;
                Message = $"Daily bookings failed: {response.Status}";
            }
        }
    }
}