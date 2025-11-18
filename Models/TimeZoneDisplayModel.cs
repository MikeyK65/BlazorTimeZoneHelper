namespace BlazorTimeZoneHelper.Models;

public class TimeZoneDisplayModel
{
    public required string TimeZoneId { get; set; }
    public required string DisplayName { get; set; }
    public DateTime CurrentTime { get; set; }
    public bool IsWithinWorkingHours { get; set; }
    public required TimeZoneInfo TimeZoneInfo { get; set; }
}
