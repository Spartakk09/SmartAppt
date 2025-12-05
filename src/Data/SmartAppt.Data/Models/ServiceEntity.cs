namespace Data.SmartAppt.SQL.Models;

public class ServiceEntity
{
    public int ServiceId { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = null!;
    public int DurationMin { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
