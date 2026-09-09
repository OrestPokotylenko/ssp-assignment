using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Domain.Image;
using WeatherPix.Infrastructure.Options.Storage;
using WeatherPix.Infrastructure.Storage.Sas;

namespace WeatherPix.Infrastructure.Storage;

public class BlobImageStorage(
    BlobServiceClient blobServiceClient,
    IOptions<StorageOptions> options)
    : IImageStorage
{
    private readonly BlobContainerClient _containerClient =
        blobServiceClient.GetBlobContainerClient(
            options.Value.GeneratedImagesContainerName);

    private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
    private readonly StorageOptions _options = options.Value;

    public async Task<IReadOnlyCollection<GeneratedImage>> GetImagesAsync(
        Guid operationId,
        CancellationToken ct)
    {
        var prefix = $"{operationId}/";
        var images = new List<GeneratedImage>();

        var sasContext = await CreateSasContextAsync(ct);

        await foreach (var blob in _containerClient.GetBlobsAsync(
                           traits: BlobTraits.None,
                           states: BlobStates.None,
                           prefix: prefix,
                           cancellationToken: ct))
        {
            var generatedImage = CreateGeneratedImage(blob.Name, sasContext);

            if (generatedImage is not null)
            {
                images.Add(generatedImage);
            }
        }

        return images;
    }

    private async Task<SasContext> CreateSasContextAsync(
        CancellationToken ct)
    {
        var startsOn = DateTimeOffset.UtcNow.AddMinutes(-_options.SasClockSkewMinutes);
        var expiresOn = startsOn.AddMinutes(_options.SasExpirationMinutes);

        var delegationKey =
            await _blobServiceClient.GetUserDelegationKeyAsync(
                startsOn,
                expiresOn,
                ct);

        return new SasContext(
            delegationKey.Value,
            startsOn,
            expiresOn);
    }

    private GeneratedImage? CreateGeneratedImage(
        string blobName,
        SasContext sasContext)
    {
        var fileName = Path.GetFileNameWithoutExtension(blobName);

        if (!int.TryParse(fileName, out var stationId))
        {
            return null;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            StartsOn = sasContext.StartsOn,
            ExpiresOn = sasContext.ExpiresOn
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sas = sasBuilder.ToSasQueryParameters(
            sasContext.DelegationKey,
            _blobServiceClient.AccountName);

        var uriBuilder = new UriBuilder(blobClient.Uri)
        {
            Query = sas.ToString()
        };

        return new GeneratedImage(
            stationId,
            uriBuilder.Uri);
    }

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