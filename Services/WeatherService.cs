using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BlazorTimeZoneHelper.Models;

namespace BlazorTimeZoneHelper.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    
    // Major city coordinates for each timezone
    private readonly Dictionary<string, (double Lat, double Lon, string City)> _timezoneLocations = new()
    {
        { "GMT Standard Time", (51.5074, -0.1278, "London") },
        { "Eastern Standard Time", (40.7128, -74.0060, "New York") },
        { "Romance Standard Time", (40.4168, -3.7038, "Madrid") },
        { "AUS Eastern Standard Time", (-33.8688, 151.2093, "Sydney") },
        { "Central European Standard Time", (52.2297, 21.0122, "Warsaw") },
        { "India Standard Time", (28.6139, 77.2090, "New Delhi") },
        { "Pacific Standard Time", (34.0522, -118.2437, "Los Angeles") },
        { "Central Standard Time", (41.8781, -87.6298, "Chicago") },
        { "Mountain Standard Time", (39.7392, -104.9903, "Denver") },
        { "China Standard Time", (39.9042, 116.4074, "Beijing") },
        { "Tokyo Standard Time", (35.6762, 139.6503, "Tokyo") },
        { "Singapore Standard Time", (1.3521, 103.8198, "Singapore") },
        { "W. Europe Standard Time", (48.8566, 2.3522, "Paris") },
        { "Central Europe Standard Time", (52.5200, 13.4050, "Berlin") },
        { "E. Europe Standard Time", (50.4501, 30.5234, "Kyiv") },
        { "Russian Standard Time", (55.7558, 37.6173, "Moscow") },
        { "Arabian Standard Time", (25.2048, 55.2708, "Dubai") },
        { "Egypt Standard Time", (30.0444, 31.2357, "Cairo") },
        { "South Africa Standard Time", (-26.2041, 28.0473, "Johannesburg") },
        { "New Zealand Standard Time", (-36.8485, 174.7633, "Auckland") }
    };

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.open-meteo.com/");
    }

    public async Task<WeatherData> GetWeatherForTimeZoneAsync(string timeZoneId)
    {
        var weatherData = new WeatherData
        {
            LocationName = "Unknown Location",
            TimeZoneId = timeZoneId,
            IsLoading = true
        };

        try
        {
            // Get location for this timezone
            if (!_timezoneLocations.TryGetValue(timeZoneId, out var location))
            {
                // Try to find a fallback based on timezone display name
                var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                var displayParts = tzInfo.DisplayName.Split(')', '(');
                var cityName = displayParts.Length > 1 ? displayParts[1].Trim() : "Unknown";
                
                weatherData.LocationName = cityName;
                weatherData.HasError = true;
                weatherData.IsLoading = false;
                return weatherData;
            }

            weatherData.LocationName = location.City;
            weatherData.Latitude = location.Lat;
            weatherData.Longitude = location.Lon;

            // Call Open-Meteo API
            var url = $"v1/forecast?latitude={location.Lat}&longitude={location.Lon}" +
                      $"&current=temperature_2m,relative_humidity_2m,precipitation,weather_code,wind_speed_10m" +
                      $"&temperature_unit=celsius&wind_speed_unit=kmh";

            var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);

            if (response?.Current != null)
            {
                weatherData.Temperature = response.Current.Temperature;
                weatherData.WeatherCode = response.Current.WeatherCode;
                weatherData.WindSpeed = response.Current.WindSpeed;
                weatherData.Humidity = response.Current.Humidity;
                weatherData.Precipitation = response.Current.Precipitation;
                weatherData.LocalTime = response.Current.Time;
                weatherData.IsLoading = false;
                weatherData.HasError = false;
            }
            else
            {
                weatherData.HasError = true;
                weatherData.IsLoading = false;
            }
        }
        catch
        {
            weatherData.HasError = true;
            weatherData.IsLoading = false;
        }

        return weatherData;
    }

    // Open-Meteo API response models
    private class OpenMeteoResponse
    {
        [JsonPropertyName("current")]
        public CurrentWeather? Current { get; set; }
    }

    private class CurrentWeather
    {
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("temperature_2m")]
        public double Temperature { get; set; }

        [JsonPropertyName("relative_humidity_2m")]
        public double Humidity { get; set; }

        [JsonPropertyName("precipitation")]
        public double Precipitation { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }
    }
}
