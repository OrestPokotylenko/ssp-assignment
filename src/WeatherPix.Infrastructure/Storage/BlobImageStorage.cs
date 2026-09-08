using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Infrastructure.Options.Storage;

namespace WeatherPix.Infrastructure.Storage;

public class BlobImageStorage(
    BlobServiceClient blobServiceClient,
    IOptions<StorageOptions> options)
    : IImageStorage
{
    private readonly BlobContainerClient _containerClient =
        blobServiceClient.GetBlobContainerClient(
            options.Value.GeneratedImagesContainerName);

    public async Task UploadAsync(
        Guid operationId,
        int stationId,
        Stream image,
        CancellationToken ct)
    {
        var blobName =
            $"{operationId}/{stationId}.jpg";

        var blobClient =
            _containerClient.GetBlobClient(blobName);

        if (image.CanSeek)
        {
            image.Position = 0;
        }

        await blobClient.UploadAsync(
            image,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = "image/jpeg"
                }
            },
            ct);
    }
}