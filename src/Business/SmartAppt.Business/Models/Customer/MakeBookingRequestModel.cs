namespace Business.SmartAppt.Models.Customer;

public class MakeBookingRequestModel
{
    public int BusinessId { get; set; }
    public int ServiceId { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public string? Notes { get; set; }
}
