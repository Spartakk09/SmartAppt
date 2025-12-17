using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Razor.SmartAppt.API.Pages.Bookings
{
    public class IndexModel : PageModel
    {
        private readonly IBusinessService _businessService;

        public IndexModel(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [BindProperty]
        public int BusinessId { get; set; }

        [BindProperty]
        public string Status { get; set; } = "All"; // Default all

        [BindProperty]
        public DateTime? Date { get; set; }

        [BindProperty]
        public int Skip { get; set; } = 0;

        [BindProperty]
        public int Take { get; set; } = 50;

        public List<BookingModel> Bookings { get; set; } = new();

        public string? Message { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostLoadAsync()
        {
            if (BusinessId <= 0)
            {
                Message = "BusinessId is required.";
                Bookings.Clear();
                return Page();
            }

            if (Skip < 0) Skip = 0;
            if (Take <= 0) Take = 50;

            await LoadBookingsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDecideAsync(int bookingId, bool confirm)
        {
            if (BusinessId <= 0 || bookingId <= 0)
            {
                Message = "Invalid BusinessId or BookingId.";
                return Page();
            }

            // Fetch pending bookings only
            var response = await _businessService.GetBookingsAsync(
                BusinessId,
                "Pending",
                null,
                0,
                Take,
                HttpContext.RequestAborted);

            if (response.Status != BaseResponseStatus.Success || response is not BaseResponse<List<BookingModel>> listResponse || listResponse.Data == null)
            {
                Message = $"Failed to load pending bookings: {response.Status}";
                await LoadBookingsAsync();
                return Page();
            }

            var booking = listResponse.Data.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                Message = "Only pending bookings can be approved or canceled.";
                await LoadBookingsAsync();
                return Page();
            }

            string newStatus = confirm ? "Confirmed" : "Canceled";
            var decideResult = await _businessService.DecideBookingsAsync(bookingId, newStatus, HttpContext.RequestAborted);

            // Reload all bookings with current filter after decision
            await LoadBookingsAsync();
            return Page();
        }

        private async Task LoadBookingsAsync()
        {
            string? statusFilter = Status == "All" ? null : Status;
            DateOnly? dateFilter = Date.HasValue ? DateOnly.FromDateTime(Date.Value) : null;

            var response = await _businessService.GetBookingsAsync(
                BusinessId,
                statusFilter,
                dateFilter,
                Skip,
                Take,
                HttpContext.RequestAborted);

            if (response.Status == BaseResponseStatus.Success && response is BaseResponse<List<BookingModel>> listResponse && listResponse.Data != null)
            {
                Bookings = listResponse.Data;
                Message += $" Loaded {Bookings.Count} bookings.";
            }
            else
            {
                Bookings.Clear();
                Message += $" Load failed: {response.Status}";
            }
        }
    }
}