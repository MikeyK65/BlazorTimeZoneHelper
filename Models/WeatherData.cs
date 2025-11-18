namespace BlazorTimeZoneHelper.Models;

public class WeatherData
{
    public required string LocationName { get; set; }
    public required string TimeZoneId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Temperature { get; set; }
    public int WeatherCode { get; set; }
    public double WindSpeed { get; set; }
    public double Humidity { get; set; }
    public double Precipitation { get; set; }
    public string LocalTime { get; set; } = string.Empty;
    public bool IsLoading { get; set; } = true;
    public bool HasError { get; set; } = false;

    public string WeatherDescription => GetWeatherDescription(WeatherCode);
    public string WeatherIcon => GetWeatherIcon(WeatherCode);
    public string TemperatureColor => GetTemperatureColor(Temperature);

    private static string GetWeatherDescription(int code)
    {
        return code switch
        {
            0 => "Clear sky",
            1 or 2 or 3 => "Partly cloudy",
            45 or 48 => "Foggy",
            51 or 53 or 55 => "Drizzle",
            61 or 63 or 65 => "Rain",
            71 or 73 or 75 => "Snow",
            77 => "Snow grains",
            80 or 81 or 82 => "Rain showers",
            85 or 86 => "Snow showers",
            95 => "Thunderstorm",
            96 or 99 => "Thunderstorm with hail",
            _ => "Unknown"
        };
    }

    private static string GetWeatherIcon(int code)
    {
        return code switch
        {
            0 => "??",
            1 or 2 => "???",
            3 => "?",
            45 or 48 => "???",
            51 or 53 or 55 => "???",
            61 or 63 or 65 => "???",
            71 or 73 or 75 => "???",
            77 => "??",
            80 or 81 or 82 => "???",
            85 or 86 => "???",
            95 => "??",
            96 or 99 => "??",
            _ => "???"
        };
    }

    private static string GetTemperatureColor(double temp)
    {
        return temp switch
        {
            < 0 => "#0066cc",      // Very cold - deep blue
            < 10 => "#3399ff",     // Cold - light blue
            < 20 => "#66cc66",     // Cool - green
            < 25 => "#ffcc00",     // Mild - yellow
            < 30 => "#ff9933",     // Warm - orange
            _ => "#ff3333"         // Hot - red
        };
    }
}
