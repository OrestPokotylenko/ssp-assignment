using Microsoft.Extensions.Logging;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;
using WeatherPix.Application.Messaging;
using WeatherPix.Domain.Job;
using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Images;

public class GenerateWeatherImageHandler(
    IImageProvider imageProvider,
    IWeatherImageRenderer imageRenderer,
    IImageStorage imageStorage,
    IJobStatusRepository jobStatusRepository,
    ILogger<GenerateWeatherImageHandler> logger)
    : IGenerateWeatherImageHandler
{
    private readonly IImageProvider _imageProvider = imageProvider;
    private readonly IWeatherImageRenderer _imageRenderer = imageRenderer;
    private readonly IImageStorage _imageStorage = imageStorage;
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;

    private readonly ILogger<GenerateWeatherImageHandler> _logger = logger;

    public async Task<Result> HandleAsync(
        GenerateImageMessage message,
        CancellationToken ct)
    {
        try
        {
            await MarkStationProcessingAsync(message, ct);

            await GenerateAndStoreImageAsync(message, ct);

            await MarkStationSucceededAsync(message, ct);

            await UpdateJobStatusIfFinishedAsync(
                message.OperationId,
                ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to generate image for station {StationId}, operation {OperationId}",
                message.StationId,
                message.OperationId);

            await HandleFailureAsync(message, ct);

            return Result.Failure(
                ImageErrors.GenerationFailed);
        }
    }

    private async Task GenerateAndStoreImageAsync(
        GenerateImageMessage message,
        CancellationToken ct)
    {
        var weatherData = MapToWeatherStation(message);
        var query = BuildImageQuery(message);

        _logger.LogInformation(
            "Downloading image for station {StationId}",
            message.StationId);

        await using var sourceImage =
            await _imageProvider.GetImageAsync(query, ct);

        _logger.LogInformation(
            "Rendering image for station {StationId}",
            message.StationId);

        await using var renderedImage =
            await _imageRenderer.RenderAsync(
                sourceImage,
                weatherData,
                ct);

        _logger.LogInformation(
            "Uploading image for station {StationId}",
            message.StationId);

        await _imageStorage.UploadAsync(
            message.OperationId,
            message.StationId,
            renderedImage,
            ct);

        _logger.LogInformation(
            "Image uploaded for station {StationId}",
            message.StationId);
    }

    private Task MarkStationProcessingAsync(
        GenerateImageMessage message,
        CancellationToken ct)
    {
        return _jobStatusRepository.UpdateStationStatusAsync(
            message.OperationId,
            message.StationId,
            JobStatus.Processing,
            ct);
    }

    private Task MarkStationSucceededAsync(
        GenerateImageMessage message,
        CancellationToken ct)
    {
        return _jobStatusRepository.UpdateStationStatusAsync(
            message.OperationId,
            message.StationId,
            JobStatus.Succeeded,
            ct);
    }

    private async Task HandleFailureAsync(
        GenerateImageMessage message,
        CancellationToken ct)
    {
        try
        {
            await _jobStatusRepository.UpdateStationStatusAsync(
                message.OperationId,
                message.StationId,
                JobStatus.Failed,
                ct);

            await UpdateJobStatusIfFinishedAsync(
                message.OperationId,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update failure status for station {StationId}, operation {OperationId}",
                message.StationId,
                message.OperationId);
        }
    }

    private async Task UpdateJobStatusIfFinishedAsync(
        Guid operationId,
        CancellationToken ct)
    {
        var progress = await _jobStatusRepository.GetProgressAsync(
            operationId,
            ct);

        if (progress.Succeeded + progress.Failed == progress.Total)
        {
            await _jobStatusRepository.UpdateStatusAsync(
                operationId,
                progress.Failed > 0
                    ? JobStatus.Failed
                    : JobStatus.Succeeded,
                ct);
        }
    }

    private static string BuildImageQuery(
    GenerateImageMessage message)
    {
        return $"{message.WeatherDescription} weather {message.Region}";
    }

    private WeatherStation MapToWeatherStation(GenerateImageMessage message)
    {
        return new WeatherStation(
            message.StationId,
            message.Name,
            message.Latitude,
            message.Longitude,
            message.Region,
            message.MeasuredAt,
            message.WindDirection,
            message.WeatherDescription,
            message.AirPressure,
            message.TemperatureCelsius,
            message.FeelTemperatureCelsius,
            message.VisibilityMeters,
            message.WindGustMetersPerSecond,
            message.WindSpeedMetersPerSecond,
            message.HumidityPercentage
            );
    }
}