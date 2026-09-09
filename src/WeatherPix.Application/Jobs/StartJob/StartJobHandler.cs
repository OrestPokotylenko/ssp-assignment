using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Common.Results;
using WeatherPix.Application.Messaging;
using WeatherPix.Application.Options.WeatherStation;
using WeatherPix.Domain.Job;
using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Application.Jobs.StartJob;

public class StartJobHandler(
    IJobStatusRepository jobStatusRepository,
    IWeatherStationProvider weatherStationProvider,
    IMessagePublisher messagePublisher,
    IOptions<WeatherStationOptions> options,
    ILogger<StartJobHandler> logger) : IStartJobHandler
{
    private readonly IJobStatusRepository _jobStatusRepository = jobStatusRepository;
    private readonly IWeatherStationProvider _weatherStationProvider = weatherStationProvider;
    private readonly IMessagePublisher _messagePublisher = messagePublisher;
    private readonly WeatherStationOptions _options = options.Value;

    private readonly ILogger<StartJobHandler> _logger = logger;

    public async Task<Result> HandleAsync(
        Guid operationId,
        CancellationToken ct)
    {
        if (!await TryMarkProcessingAsync(operationId, ct))
        {
            return Result.Failure(JobErrors.StatusUpdateFailed);
        }

        var stationsResult = await TryGetStationsAsync(operationId, ct);

        if (stationsResult.IsFailure || stationsResult.Value is null)
        {
            return Result.Failure(
                stationsResult.Error ?? JobErrors.WeatherRetrievalFailed);
        }

        var selectedStations = stationsResult.Value
            .Take(_options.Count)
            .ToList();

        var messages = CreateMessages(
            operationId,
            selectedStations);

        if (!await TryCreateStationJobsAsync(
                operationId,
                selectedStations,
                ct))
        {
            await TryMarkFailedAsync(operationId, ct);

            return Result.Failure(
                JobErrors.StatusUpdateFailed);
        }

        if (!await TryPublishMessagesAsync(
                operationId,
                messages,
                ct))
        {
            await TryMarkFailedAsync(operationId, ct);

            return Result.Failure(
                JobErrors.ImageQueueFailed);
        }

        return Result.Success();
    }

    private async Task<bool> TryMarkProcessingAsync(
    Guid operationId,
    CancellationToken ct)
    {
        try
        {
            await _jobStatusRepository.UpdateStatusAsync(
                operationId,
                JobStatus.Processing,
                ct);

            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to mark job {OperationId} as processing",
                operationId);

            return false;
        }
    }

    private async Task<Result<IReadOnlyCollection<WeatherStation>>> TryGetStationsAsync(
    Guid operationId,
    CancellationToken ct)
    {
        try
        {
            var stations =
                await _weatherStationProvider.GetStationsAsync(ct);

            return Result<IReadOnlyCollection<WeatherStation>>
                .Success(stations);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve weather stations for job {OperationId}",
                operationId);

            await TryMarkFailedAsync(operationId, ct);

            return Result<IReadOnlyCollection<WeatherStation>>
                .FailureWith(JobErrors.WeatherRetrievalFailed);
        }
    }

    private static IReadOnlyCollection<GenerateImageMessage> CreateMessages(
    Guid operationId,
    IReadOnlyCollection<WeatherStation> stations)
    {
        return
        [
            .. stations.Select(station =>
            new GenerateImageMessage(
                operationId,
                station.StationId,
                station.Name,
                station.Latitude,
                station.Longitude,
                station.Region,
                station.MeasuredAt,
                station.WeatherDescription,
                station.WindDirection,
                station.AirPressure,
                station.TemperatureCelsius,
                station.FeelTemperatureCelsius,
                station.VisibilityMeters,
                station.WindGustMetersPerSecond,
                station.WindSpeedMetersPerSecond,
                station.HumidityPercentage))
        ];
    }

    private async Task<bool> TryCreateStationJobsAsync(
    Guid operationId,
    IReadOnlyCollection<WeatherStation> stations,
    CancellationToken ct)
    {
        try
        {
            await _jobStatusRepository.CreateStationJobsAsync(
                operationId,
                [.. stations.Select(x => x.StationId)],
                ct);

            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create station jobs for operation {OperationId}",
                operationId);

            return false;
        }
    }

    private async Task<bool> TryPublishMessagesAsync(
    Guid operationId,
    IReadOnlyCollection<GenerateImageMessage> messages,
    CancellationToken ct)
    {
        try
        {
            await _messagePublisher.PublishBatchAsync(
                messages,
                ct);

            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish image jobs for operation {OperationId}",
                operationId);

            return false;
        }
    }

    private async Task TryMarkFailedAsync(
        Guid operationId,
        CancellationToken ct)
    {
        try
        {
            await _jobStatusRepository.UpdateStatusAsync(
                operationId,
                JobStatus.Failed,
                ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to mark job {OperationId} as failed",
                operationId);
        }
    }
}