namespace WeatherPix.Domain.WeatherStation;

public sealed record WeatherStation(
    int StationId,
    string? Name,
    double? Latitude,
    double? Longitude,
    string? Region,
    DateTimeOffset MeasuredAt,
    string? WindDirection,
    string? WeatherDescription,
    double? AirPressure,
    double? TemperatureCelsius,
    double? FeelTemperatureCelsius,
    double? VisibilityMeters,
    double? WindGustMetersPerSecond,
    double? WindSpeedMetersPerSecond,
    double? HumidityPercentage
);