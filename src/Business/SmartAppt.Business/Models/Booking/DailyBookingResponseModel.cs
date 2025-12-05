namespace Business.SmartAppt.Models.Booking;
public class DailyBookingResponseModel
{
    public int BookingId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }

    public ServiceResponseModel Service { get; set; } = null!;
    public CustomerResponseModel Customer { get; set; } = null!;
}

public class ServiceResponseModel
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = null!;
    public int DurationMin { get; set; }
    public decimal Price { get; set; }
}

public class CustomerResponseModel
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}