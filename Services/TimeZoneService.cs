using BlazorTimeZoneHelper.Models;

namespace BlazorTimeZoneHelper.Services;

public class TimeZoneService
{
    private const string SELECTED_TIMEZONES_KEY = "timeZoneHelper_selectedTimezones";
    private const string DISPLAY_MODE_KEY = "timeZoneHelper_displayMode";

    public enum DisplayMode
    {
        Grid,
        List,
        Compact
    }

    private readonly List<(string Id, string DisplayName)> _availableTimeZones;
    private readonly LocalStorageService _localStorage;

    // In-memory storage for selected timezones and display preferences
    private HashSet<string> _selectedTimeZoneIds = new();
    private DisplayMode _displayMode = DisplayMode.Grid;
    private bool _isInitialized = false;

    public TimeZoneService(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
        
        // Load all system timezones
        _availableTimeZones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => (tz.Id, tz.DisplayName))
            .OrderBy(tz => tz.DisplayName)
            .ToList();

        // Default: select initial timezones (will be overridden by LoadSettingsAsync if available)
        _selectedTimeZoneIds = new HashSet<string>(new[]
        {
            "GMT Standard Time",
            "Eastern Standard Time",
            "Romance Standard Time",
            "AUS Eastern Standard Time",
            "Central European Standard Time",
            "India Standard Time"
        });
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await LoadSettingsAsync();
        _isInitialized = true;
    }

    private async Task LoadSettingsAsync()
    {
        // Load selected timezones
        var savedTimeZones = await _localStorage.GetItemAsync<List<string>>(SELECTED_TIMEZONES_KEY);
        if (savedTimeZones != null && savedTimeZones.Any())
        {
            _selectedTimeZoneIds = new HashSet<string>(savedTimeZones);
        }

        // Load display mode
        var savedMode = await _localStorage.GetItemAsync<string>(DISPLAY_MODE_KEY);
        if (!string.IsNullOrEmpty(savedMode) && Enum.TryParse<DisplayMode>(savedMode, out var mode))
        {
            _displayMode = mode;
        }
    }

    public List<(string Id, string DisplayName)> GetAvailableTimeZones()
    {
        return _availableTimeZones;
    }

    public List<string> GetSelectedTimeZoneIds()
    {
        return _selectedTimeZoneIds.ToList();
    }

    public async Task SetSelectedTimeZonesAsync(IEnumerable<string> timeZoneIds)
    {
        _selectedTimeZoneIds = new HashSet<string>(timeZoneIds);
        await _localStorage.SetItemAsync(SELECTED_TIMEZONES_KEY, _selectedTimeZoneIds.ToList());
    }

    public DisplayMode GetDisplayMode()
    {
        return _displayMode;
    }

    public async Task SetDisplayModeAsync(DisplayMode mode)
    {
        _displayMode = mode;
        await _localStorage.SetItemAsync(DISPLAY_MODE_KEY, mode.ToString());
    }

    public List<TimeZoneDisplayModel> GetTimeZonesForReference(DateTime referenceTime, string referenceTimeZoneId)
    {
        var results = new List<TimeZoneDisplayModel>();
        
        // Get the reference timezone info
        TimeZoneInfo referenceTimeZone;
        try
        {
            referenceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(referenceTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback to UTC if timezone not found
            referenceTimeZone = TimeZoneInfo.Utc;
        }

        // Convert reference time to UTC first (assumes referenceTime is in the reference timezone)
        DateTimeOffset referenceDto = new DateTimeOffset(
            DateTime.SpecifyKind(referenceTime, DateTimeKind.Unspecified),
            referenceTimeZone.GetUtcOffset(referenceTime)
        );
        DateTimeOffset utcTime = referenceDto.ToUniversalTime();

        // Convert to each selected timezone
        foreach (var (tzId, displayName) in _availableTimeZones)
        {
            // Only include if selected
            if (!_selectedTimeZoneIds.Contains(tzId))
                continue;

            try
            {
                var targetTz = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                var targetTime = TimeZoneInfo.ConvertTime(utcTime, targetTz);
                
                var isWithinWorkingHours = IsWithinWorkingHours(targetTime.DateTime);

                results.Add(new TimeZoneDisplayModel
                {
                    TimeZoneId = tzId,
                    DisplayName = displayName,
                    CurrentTime = targetTime.DateTime,
                    IsWithinWorkingHours = isWithinWorkingHours,
                    TimeZoneInfo = targetTz
                });
            }
            catch (TimeZoneNotFoundException)
            {
                // Skip timezones that can't be found
                continue;
            }
        }

        return results;
    }

    public DateTime GetDefaultLondonTime()
    {
        // Get current date and time in London timezone
        var londonTz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var nowUtc = DateTime.UtcNow;
        var nowLondon = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, londonTz);
        
        // Return the current time in London (not hardcoded to 9 AM)
        return new DateTime(
            nowLondon.Year,
            nowLondon.Month,
            nowLondon.Day,
            nowLondon.Hour,
            nowLondon.Minute,
            nowLondon.Second,
            DateTimeKind.Unspecified
        );
    }

    private bool IsWithinWorkingHours(DateTime time)
    {
        // Working hours: 9 AM to 5 PM (17:00)
        var hour = time.Hour;
        return hour >= 9 && hour < 17;
    }

    public string FormatTimeWithOffset(DateTime time, TimeZoneInfo timeZone)
    {
        var offset = timeZone.GetUtcOffset(time);
        var offsetString = $"UTC{(offset.TotalHours >= 0 ? "+" : "")}{offset.TotalHours:0.##}";
        
        return $"{time:hh:mm tt} ({offsetString})";
    }
}
