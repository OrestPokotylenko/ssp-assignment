namespace WeatherPix.Application.Abstractions;

public interface IImageProvider
{
    Task<Stream> GetImageAsync(
        string query,
        CancellationToken ct);
}