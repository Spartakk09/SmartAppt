namespace Business.SmartAppt.Models.Service;

public class ServiceModel
{
    public int ServiceId { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = null!;
    public int DurationMin { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
