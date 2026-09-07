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

    public async Task<Result> HandleAsync(Guid operationId, CancellationToken ct)
    {
        try
        {
            await _jobStatusRepository.UpdateStatusAsync(
                operationId,
                JobStatus.Processing,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to mark job {OperationId} as processing",
                operationId);

            return Result.Failure(JobErrors.StatusUpdateFailed);
        }

        IReadOnlyCollection<WeatherStation> stations;

        try
        {
            stations = await _weatherStationProvider.GetStationsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve weather stations for job {OperationId}",
                operationId);

            await TryMarkFailedAsync(operationId, ct);

            return Result.Failure(JobErrors.WeatherRetrievalFailed);
        }

        var messages = stations
            .Take(_options.Count)
            .Select(station => new GenerateImageMessage(
                operationId,
                station.StationId,
                station.Name,
                station.Region,
                station.Latitude,
                station.Longitude,
                station.TemperatureCelsius,
                station.HumidityPercentage,
                station.WindDirection,
                station.WindSpeedMetersPerSecond,
                station.WindGustMetersPerSecond,
                station.AirPressureHpa,
                station.VisibilityMeters,
                station.PrecipitationMillimeters,
                station.MeasuredAt))
            .ToList();

        try
        {
            await _messagePublisher.PublishBatchAsync(messages, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to publish image jobs for operation {OperationId}",
                operationId);

            await TryMarkFailedAsync(operationId, ct);

            return Result.Failure(JobErrors.ImageQueueFailed);
        }

        return Result.Success();
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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to mark job {OperationId} as failed",
                operationId);
        }
    }
}