using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Images.Contracts;
using WeatherPix.Infrastructure.Options.Pexels;

namespace WeatherPix.Infrastructure.Images;

public class PexelsImageProvider(
    HttpClient httpClient,
    IOptions<PexelsOptions> options)
    : IImageProvider
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly PexelsOptions _options = options.Value;

    public async Task<Stream> GetImageAsync(
        string query,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"v1/search?query={Uri.EscapeDataString(query)}&per_page=1");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(_options.ApiKey);

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PexelsSearchResponse>(
                cancellationToken: ct);

        var imageUrl = result?
            .Photos
            .FirstOrDefault()?
            .Src
            .Large;

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new InvalidOperationException(
                $"Pexels returned no image for query '{query}'.");
        }

        return await _httpClient.GetStreamAsync(
            imageUrl,
            ct);
    }
}