namespace Business.SmartAppt.Models;

public class BusinessCreateRequestModel
{
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string TimeZoneIana { get; set; } = "Asia/Yerevan";
    public string? SettingsJson { get; set; }
}
