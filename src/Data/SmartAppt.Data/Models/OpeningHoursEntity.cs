namespace Data.SmartAppt.SQL.Models;

public class OpeningHoursEntity
{
    public int OpeningHoursId { get; set; }
    public int BusinessId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpenTimeUtc { get; set; }
    public TimeSpan CloseTimeUtc { get; set; }
}