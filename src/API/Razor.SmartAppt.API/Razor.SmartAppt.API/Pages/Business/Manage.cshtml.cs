using Business.SmartAppt.Models;
using Data.SmartAppt.SQL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.SmartAppt.API.Pages.Business
{
    public class ManageModel : PageModel
    {
        private readonly IBusinessService _businessService;

        [BindProperty]
        public int SearchId { get; set; }

        [BindProperty]
        public BusinessModel Business { get; set; } = new BusinessModel();

        public string? Message { get; set; }

        public ManageModel(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        public async Task OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                SearchId = id.Value;
                await LoadBusinessAsync(id.Value);
            }
        }

        public async Task<IActionResult> OnPostLoadAsync()
        {
            if (SearchId <= 0)
            {
                Message = "Please enter a valid Business Id.";
                return Page();
            }

            await LoadBusinessAsync(SearchId);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (Business == null || Business.BusinessId <= 0)
            {
                Message = "No business loaded.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Business.Name))
            {
                ModelState.AddModelError("Business.Name", "Name is required.");
                return Page();
            }

            var requestModel = new BusinessCreateRequestModel
            {
                Name = Business.Name,
                Email = Business.Email,
                Phone = Business.Phone,
                TimeZoneIana = Business.TimeZoneIana,
                SettingsJson = Business.SettingsJson
            };

            var result = await _businessService.UpdateBusinessByIdAsync(Business.BusinessId, requestModel, HttpContext.RequestAborted);

            Message = result.Status.ToString();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (Business == null || Business.BusinessId <= 0)
            {
                Message = "No business loaded to delete.";
                return Page();
            }

            var result = await _businessService.DeleteBusinessAsync(Business.BusinessId, HttpContext.RequestAborted);

            Message = result.Status.ToString();

            Business = new BusinessModel();
            SearchId = 0;

            return Page();
        }

        private async Task LoadBusinessAsync(int id)
        {
            BaseResponse response = await _businessService.GetBusinessByIdAsync(id, HttpContext.RequestAborted);

            if (response == null || response.Status != BaseResponseStatus.Success)
            {
                Message = $"Business not found or invalid. Status: {response?.Status}";
                Business = new BusinessModel();
                return;
            }

            // Extract the Data from the BaseResponse
            BusinessModel? businessData = (response as BaseResponse<BusinessModel>)?.Data;

            Business = businessData?? new BusinessModel();
            SearchId = Business.BusinessId;
            Message = "Business loaded successfully.";
        }
    }
}
