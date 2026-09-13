namespace WeatherPix.Functions.Jobs.Contracts;

public sealed record ErrorResponse(
    string Code,
    string Message
);