namespace Data.SmartAppt.SQL.Models;

public class BookingWithCustomerDetailsModel
{
    public int BookingId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }

    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public int DurationMin { get; set; }
    public decimal Price { get; set; }

    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
