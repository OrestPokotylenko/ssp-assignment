namespace WeatherPix.Infrastructure.Queue.Contracts;

public sealed record StartJobMessage(
    Guid OperationId);