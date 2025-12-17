using Business.SmartAppt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Razor.SmartAppt.API.Pages.Business
{
    public class CreateModel : PageModel
    {
        private readonly IBusinessService _businessService;

        [BindProperty]
        public BusinessCreateRequestModel Business { get; set; } = new();

        public BaseResponseStatus? ResultStatus { get; set; }
        public int? CreatedBusinessId { get; set; }

        public CreateModel(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Business.Name))
            {
                ModelState.AddModelError("Business.Name", "Name is required");
                return Page();
            }

            var response = await _businessService.CreateBusinessAsync(Business, HttpContext.RequestAborted);

            ResultStatus = response.Status;

            if (response is BaseResponse<int> typedResponse)
            {
                CreatedBusinessId = typedResponse.Data;
            }

            return Page();
        }
    }
}
