namespace Business.SmartAppt.Models.Business;

public class BookingModel
{
    public int BookingId { get; set; }
    public int BusinessId { get; set; }
    public int ServiceId { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
