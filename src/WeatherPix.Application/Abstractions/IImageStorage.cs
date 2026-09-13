using WeatherPix.Domain.Image;

namespace WeatherPix.Application.Abstractions;

public interface IImageStorage
{
    Task UploadAsync(
        Guid operationId,
        int stationId,
        Stream image,
        CancellationToken ct);

    Task<IReadOnlyCollection<GeneratedImage>> GetImagesAsync(
        Guid operationId,
        CancellationToken ct);
}