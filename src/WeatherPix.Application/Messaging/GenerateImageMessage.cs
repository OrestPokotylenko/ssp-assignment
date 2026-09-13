namespace WeatherPix.Application.Messaging;

public sealed record GenerateImageMessage(
    Guid OperationId,
    int StationId,
    string? Name,
    double? Latitude,
    double? Longitude,
    string? Region,
    DateTimeOffset MeasuredAt,
    string? WeatherDescription,
    string? WindDirection,
    double? AirPressure,
    double? TemperatureCelsius,
    double? FeelTemperatureCelsius,
    double? VisibilityMeters,
    double? WindGustMetersPerSecond,
    double? WindSpeedMetersPerSecond,
    double? HumidityPercentage
) : IMessage;