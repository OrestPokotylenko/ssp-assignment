namespace WeatherPix.Application.Messaging;

public record StartJobMessage(
    Guid OperationId)
    : IMessage;