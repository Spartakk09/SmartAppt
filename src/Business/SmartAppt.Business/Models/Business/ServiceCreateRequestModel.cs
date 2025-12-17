namespace Business.SmartAppt.Models.Business;

public class ServiceCreateRequestModel
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = null!;
    public int DurationMin { get; set; }
    public decimal Price { get; set; }
}
