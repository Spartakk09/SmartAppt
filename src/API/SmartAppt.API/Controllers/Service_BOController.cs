using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Service;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Service_BOController : ControllerBase
{
    protected readonly IService_BO_Service _service;

    public Service_BOController(IService_BO_Service service)
    {
        _service = service;
    }

    [Route("Create")]
    [HttpPost]
    public async Task<BaseResponse> CreateAsync(ServiceCreateRequestModel model, CancellationToken ct)
    {
        var response = await _service.CreateAsync(model, ct);
        return response;
    }

    [Route("Update")]
    [HttpPut]
    public async Task<BaseResponse> UpdateAsync(ServiceModel model, CancellationToken ct)
    {
        var response = await _service.UpdateAsync(model, ct);
        return response; 
    }

    [Route("GetById/{serviceId}")]
    [HttpGet]
    public async Task<BaseResponse> GetByIdAsync(int serviceId, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(serviceId, ct);
        return response;
    }

    [Route("DeleteById/{serviceId}")]
    [HttpDelete]
    public async Task<BaseResponse> DeleteAsync(int serviceId, CancellationToken ct)
    {
        var response = await _service.DeleteAsync(serviceId, ct);
        return response;
    }
}
