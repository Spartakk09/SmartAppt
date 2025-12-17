using Business.SmartAppt.Models;
using Business.SmartAppt.Models.Customer;
using Business.SmartAppt.Services;
using Microsoft.AspNetCore.Mvc;

namespace SmartAppt.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    protected readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [Route("Create")]
    [HttpPost]
    public async Task<BaseResponse> CreateAsync(CreateCustomerRequestModel model, CancellationToken ct)
    {
        var response = await _customerService.CreateCustomerAsync(model, ct);
        return response;
    }

    [Route("{customerId}")]
    [HttpGet]
    public async Task<BaseResponse> GetByIdAsync(int customerId, CancellationToken ct)
    {
        var response = await _customerService.GetCustomerByIdAsync(customerId, ct);
        return response;
    }

    [Route("Update")]
    [HttpPut]
    public async Task<BaseResponse> UpdateAsync(UpdateCustomerRequestModel model, CancellationToken ct)
    {
        var response = await _customerService.UpdateCustomerAsync(model, ct);
        return response;
    }

    [Route("{customerId}")]
    [HttpDelete]
    public async Task<BaseResponse> DeleteAsync(int customerId, CancellationToken ct)
    {
        var response = await _customerService.DeleteCustomerByIdAsync(customerId, ct);
        return response;
    }

    [Route("GetMyBookings/{customerId}")]
    [HttpGet]
    public async Task<BaseResponse> GetMyBookingsAsync(int customerId, CancellationToken ct)
    {
        var response = await _customerService.GetMyBookingsAsync(customerId, ct);
        return response;
    }

    [Route("MakeBooking")]
    [HttpPost]
    public async Task<BaseResponse> CreateBookingAsync(MakeBookingRequestModel model, CancellationToken ct)
    {
        var response = await _customerService.MakeBookingAsync(model, ct);
        return response;
    }

    [Route("CancelBooking/{bookingId}")]
    [HttpDelete]
    public async Task<BaseResponse> CancelBookingAsync(int bookingId, CancellationToken ct)
    {
        var response = await _customerService.CancelBookingAsync(bookingId, ct);
        return response;
    }

    [Route("UpdateBooking")]
    [HttpPut]
    public async Task<BaseResponse> UpdateBookingAsync(int oldBookingId, MakeBookingRequestModel newBooking, CancellationToken ct)
    {
        var response = await _customerService.UpdateBookingAsync(oldBookingId, newBooking, ct);
        return response;
    }

    [Route("FreeSlotsForPerDay")]
    [HttpGet]
    public async Task<BaseResponse> GetFreeSlotsForDayAsync(int businessId, int serviceId, DateTime date, CancellationToken ct)
    {
        var result = await _customerService.GetFreeSlotsForDayAsync(businessId, serviceId, date, ct);

        return result;
    }

    [Route("FreeSlotInMonth")]
    [HttpGet]
    public async Task<BaseResponse> FreeSlotsForMonthAsync(int businessId, int serviceId, int month, int? year, CancellationToken ct)
    {
        var response = await _customerService.HasFreeSlotsForMonthAsync(businessId, serviceId, month, year, ct);
        return response;
    }
}
