namespace Data.SmartAppt.SQL.Models;

public class BusinessEntity
{
    public int BusinessId { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string TimeZoneIana { get; set; } = "Asia/Yerevan";
    public string? SettingsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
