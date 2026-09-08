namespace WeatherPix.Application.Abstractions;

public interface IImageStorage
{
    Task UploadAsync(
        Guid operationId,
        int stationId,
        Stream image,
        CancellationToken ct);
}