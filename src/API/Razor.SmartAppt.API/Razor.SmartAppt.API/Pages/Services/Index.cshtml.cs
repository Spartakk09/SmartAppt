using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Business;
using Data.SmartAppt.SQL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.SmartAppt.API.Pages.Services
{
    public class IndexModel : PageModel
    {
        private readonly IBusinessService _businessService;

        public IndexModel(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        // ===================== PROPERTIES =====================

        [BindProperty]
        public int BusinessId { get; set; }

        [BindProperty]
        public int Skip { get; set; } = 0;

        [BindProperty]
        public int Take { get; set; } = 10;

        [BindProperty]
        public ServiceCreateRequestModel NewService { get; set; } = new();

        public List<ServiceModel> Services { get; set; } = new();

        public string? Message { get; set; }

        public void OnGet() { }

        // ===================== LOAD =====================

        public async Task<IActionResult> OnPostLoadAsync()
        {
            if (BusinessId <= 0)
            {
                Message = "Invalid Business Id.";
                Services.Clear();
                return Page();
            }

            await LoadServicesAsync();
            return Page();
        }

        // ===================== ADD =====================

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (BusinessId <= 0)
            {
                Message = "BusinessId is required.";
                return Page();
            }

            NewService.BusinessId = BusinessId;

            var result = await _businessService.AddServicesAsync(
                NewService,
                HttpContext.RequestAborted);

            Message = $"Add Result: {result.Status}";

            NewService = new ServiceCreateRequestModel();

            await LoadServicesAsync();
            return Page();
        }

        // ===================== DISABLE =====================

        public async Task<IActionResult> OnPostDisableAsync(int serviceId)
        {
            if (serviceId <= 0)
            {
                Message = "Invalid Service Id.";
                return Page();
            }

            var result = await _businessService.DisableServiceAsync(
                serviceId,
                HttpContext.RequestAborted);

            Message = $"Disable Result: {result.Status}";

            await LoadServicesAsync();
            return Page();
        }

        // ===================== DELETE =====================

        public async Task<IActionResult> OnPostDeleteAsync(int serviceId)
        {
            if (serviceId <= 0)
            {
                Message = "Invalid Service Id.";
                return Page();
            }

            var result = await _businessService.DeleteServiceAsync(
                serviceId,
                HttpContext.RequestAborted);

            Message = $"Delete Result: {result.Status}";

            await LoadServicesAsync();
            return Page();
        }

        // ===================== PRIVATE LOADER =====================

        private async Task LoadServicesAsync()
        {
            if (BusinessId <= 0)
            {
                Message = "BusinessId lost during postback.";
                Services.Clear();
                return;
            }

            var response = await _businessService.GetServicesByBusinessIdAsync(
                BusinessId,
                Skip,
                Take,
                HttpContext.RequestAborted);

            if (response.Status == BaseResponseStatus.Success)
            {
                Services =
                    (response as BaseResponse<List<ServiceModel>>)?.Data
                    ?? new List<ServiceModel>();

                Message = $"Loaded {Services.Count} services.";
            }
            else
            {
                Services.Clear();
                Message = $"Load failed: {response.Status}";
            }
        }
    }
}
