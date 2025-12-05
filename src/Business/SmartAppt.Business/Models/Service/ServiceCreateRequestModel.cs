namespace Business.SmartAppt.Models.Service;

public class ServiceCreateRequestModel
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = null!;
    public int DurationMin { get; set; }
    public decimal Price { get; set; }
}
