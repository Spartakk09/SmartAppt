using Business.SmartAppt.Models;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Business_BOController : ControllerBase
{
    protected readonly IBusiness_BO_Service _businessService;
    public Business_BOController(IBusiness_BO_Service businessService)
    {
        _businessService = businessService;
    }

    [Route("Create")]
    [HttpPost]
    public async Task<BaseResponse> CreateAsync(BusinessCreateRequestModel model, CancellationToken ct)
    {
        var response = await _businessService.CreateAsync(model, ct);
        return response;
    }

    [Route("Update")]
    [HttpPut]
    public async Task<BaseResponse> UpdateAsync(BusinessModel model, CancellationToken ct)
    {
        var response = await _businessService.UpdateAsync(model, ct);
        return response; // directly return BaseResponse
    }

    // Get business by ID
    [Route("GetById/{businessId}")]
    [HttpGet]
    public async Task<BaseResponse> GetByIdAsync(int businessId, CancellationToken ct)
    {
        var response = await _businessService.GetByIdAsync(businessId, ct);
        return response;
    }

    // Delete a business
    [Route("DeleteById/{businessId}")]
    [HttpDelete]
    public async Task<BaseResponse> DeleteAsync(int businessId, CancellationToken ct)
    {
        var response = await _businessService.DeleteAsync(businessId, ct);
        return response;
    }
}
