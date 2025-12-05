namespace Business.SmartAppt.Models.Customer;

public class CreateCustomerRequestModel
{
    public int BusinessId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
