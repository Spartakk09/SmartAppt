using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Booking;
using Business.SmartAppt.Models.Service;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusinessController : ControllerBase
{
    protected readonly IBusinessService _businessService;
    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [Route("Create")]
    [HttpPost]
    public async Task<BaseResponse> CreateBusinessAsync(BusinessCreateRequestModel model, CancellationToken ct)
    {
        var response = await _businessService.CreateBusinessAsync(model, ct);
        return response;
    }

    [Route("Update/{businessId}")]
    [HttpPut]
    public async Task<BaseResponse> UpdateBusinessByIdAsync(int businessId, BusinessCreateRequestModel model, CancellationToken ct)
    {
        var response = await _businessService.UpdateBusinessByIdAsync(businessId, model, ct);
        return response;
    }

    [Route("Delete/{businessId}")]
    [HttpDelete]
    public async Task<BaseResponse> DeleteBusinessAsync(int businessId, CancellationToken ct)
    {
        var response = await _businessService.DeleteBusinessAsync(businessId, ct);
        return response;
    }

    [Route("AddService")]
    [HttpPost]
    public async Task<BaseResponse> AddServices(ServiceCreateRequestModel model, CancellationToken ct)
    {
        var response = await _businessService.AddServicesAsync(model, ct);
        return response;
    }

    [Route("DeleteService/{serviceId}")]
    [HttpDelete]
    public async Task<BaseResponse> DeleteServiceAsync(int serviceId, CancellationToken ct)
    {
        var response = await _businessService.DeleteServiceAsync(serviceId, ct);
        return response;
    }

    [Route("DisableService/{serviceId}")]
    [HttpPut]
    public async Task<BaseResponse> DisableServiceAsync(int serviceId, CancellationToken ct)
    {
        var response = await _businessService.DisableServiceAsync(serviceId, ct);
        return response;
    }

    [Route("CancelBooking/{bookingId}")]
    [HttpPut]
    public async Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        var response = await _businessService.CancelBookingAsync(bookingId, ct);
        return response;
    }

    [Route("DailyBookingsWithCustomerDetails/{businessId}")]
    [HttpGet]
    public async Task<BaseResponse> GetDailyBookings(int businessId, DateTime dateUtc, CancellationToken ct)
    {
        var result = await _businessService.GetDailyBookingsWithCustomersAsync(businessId, dateUtc, ct);
        return result;
    }

    [Route("AllBookingsForBusiness/{businessId}")]
    [HttpGet]
    public async Task<BaseResponse> GetAllBookingsByBusinessIdAsync(int businessId, CancellationToken ct)
    {
        var result = await _businessService.GetAllBookingsByBusinessIdAsync(businessId, ct);
        return result;
    }

    [Route("Decide-Booking/{bookingId}")]
    [HttpPut]
    public async Task<BaseResponse> DecideBookingsAsync(int bookingId, string status, CancellationToken ct)
    {
        var result = await _businessService.DecideBookingsAsync(bookingId, status, ct);
        return result;
    }

    [Route("Monthly-Calendar/{businessId}")]
    [HttpGet]
    public async Task<BaseResponse> GetMonthlyCalendarAsync(int businessId, int month, int? year, CancellationToken ct)
    {
        var result = await _businessService.GetMonthlyCalendarAsync(businessId, month, year, ct);
        return result;
    }
}
