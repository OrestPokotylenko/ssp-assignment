namespace WeatherPix.Application.Messaging;

public interface IMessage
{
    Guid OperationId { get; }
}