namespace WeatherPix.Application.Messaging;

public sealed record GenerateImageMessage(
    Guid OperationId,
    int StationId,
    string Name,
    string Region,
    double Latitude,
    double Longitude,
    double TemperatureCelsius,
    double HumidityPercentage,
    string WindDirection,
    double WindSpeedMetersPerSecond,
    double WindGustMetersPerSecond,
    double AirPressureHpa,
    double VisibilityMeters,
    double PrecipitationMillimeters,
    DateTimeOffset MeasuredAt
) : IMessage;