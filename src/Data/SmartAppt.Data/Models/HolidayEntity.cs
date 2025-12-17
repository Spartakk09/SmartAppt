namespace Data.SmartAppt.SQL.Models;

public class HolidayEntity
{
    public int HolidayId { get; set; }
    public int BusinessId { get; set; }
    public DateTime HolidayDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
