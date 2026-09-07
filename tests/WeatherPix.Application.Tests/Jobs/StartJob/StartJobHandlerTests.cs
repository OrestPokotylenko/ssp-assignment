using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Jobs.StartJob;
using WeatherPix.Application.Messaging;
using WeatherPix.Application.Options.WeatherStation;
using WeatherPix.Domain.Job;
using WeatherPix.Domain.WeatherStation;
using Xunit;

namespace WeatherPix.Application.Tests.Jobs.StartJob;

public class StartJobHandlerTests
{
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly IWeatherStationProvider _weatherStationProvider;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<StartJobHandler> _logger;

    private readonly StartJobHandler _handler;

    private readonly Guid _operationId = Guid.NewGuid();

    public StartJobHandlerTests()
    {
        _jobStatusRepository =
            Substitute.For<IJobStatusRepository>();

        _weatherStationProvider =
            Substitute.For<IWeatherStationProvider>();

        _messagePublisher =
            Substitute.For<IMessagePublisher>();

        _logger =
            Substitute.For<ILogger<StartJobHandler>>();

        var options = Microsoft.Extensions.Options.Options.Create(
            new WeatherStationOptions
            {
                Count = 50
            });

        _handler = new StartJobHandler(
            _jobStatusRepository,
            _weatherStationProvider,
            _messagePublisher,
            options,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenEverythingSucceeds_PublishesImageMessages()
    {
        // Arrange
        var stations = CreateStations(5);

        _weatherStationProvider
            .GetStationsAsync(Arg.Any<CancellationToken>())
            .Returns(stations);

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _jobStatusRepository
            .Received(1)
            .UpdateStatusAsync(
                _operationId,
                JobStatus.Processing,
                Arg.Any<CancellationToken>());

        await _messagePublisher
            .Received(1)
            .PublishBatchAsync(
                Arg.Is<IReadOnlyCollection<GenerateImageMessage>>(
                    messages =>
                        messages.Count == 5 &&
                        messages.All(x =>
                            x.OperationId == _operationId)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStatusUpdateFails_ReturnsFailure()
    {
        // Arrange
        _jobStatusRepository
            .UpdateStatusAsync(
                _operationId,
                JobStatus.Processing,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Table Storage failed")));

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        await _weatherStationProvider
            .DidNotReceive()
            .GetStationsAsync(
                Arg.Any<CancellationToken>());

        await _messagePublisher
            .DidNotReceive()
            .PublishBatchAsync(
                Arg.Any<IReadOnlyCollection<GenerateImageMessage>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenWeatherProviderFails_MarksJobFailed()
    {
        // Arrange
        _weatherStationProvider
            .GetStationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<
                IReadOnlyCollection<WeatherStation>>(
                new Exception("Buienradar failed")));

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        await _jobStatusRepository
            .Received(1)
            .UpdateStatusAsync(
                _operationId,
                JobStatus.Failed,
                Arg.Any<CancellationToken>());

        await _messagePublisher
            .DidNotReceive()
            .PublishBatchAsync(
                Arg.Any<IReadOnlyCollection<GenerateImageMessage>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenPublishingFails_MarksJobFailed()
    {
        // Arrange
        _weatherStationProvider
            .GetStationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(CreateStations(5));

        _messagePublisher
            .PublishBatchAsync(
                Arg.Any<IReadOnlyCollection<GenerateImageMessage>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Service Bus failed")));

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        await _jobStatusRepository
            .Received(1)
            .UpdateStatusAsync(
                _operationId,
                JobStatus.Failed,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMarkFailedAlsoFails_StillReturnsFailure()
    {
        // Arrange
        _weatherStationProvider
            .GetStationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(CreateStations(5));

        _messagePublisher
            .PublishBatchAsync(
                Arg.Any<IReadOnlyCollection<GenerateImageMessage>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Service Bus failed")));

        _jobStatusRepository
            .UpdateStatusAsync(
                _operationId,
                JobStatus.Failed,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Table Storage update failed")));

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task HandleAsync_WhenMoreStationsReturned_OnlyConfiguredAmountIsPublished()
    {
        // Arrange

        var options = Microsoft.Extensions.Options.Options.Create(
            new WeatherStationOptions
            {
                Count = 3
            });

        var handler = new StartJobHandler(
            _jobStatusRepository,
            _weatherStationProvider,
            _messagePublisher,
            options,
            _logger);

        _weatherStationProvider
            .GetStationsAsync(
                Arg.Any<CancellationToken>())
            .Returns(CreateStations(10));

        // Act
        var result = await handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _messagePublisher
            .Received(1)
            .PublishBatchAsync(
                Arg.Is<IReadOnlyCollection<GenerateImageMessage>>(
                    messages => messages.Count == 3),
                Arg.Any<CancellationToken>());
    }

    private static IReadOnlyCollection<WeatherStation> CreateStations(
        int count)
    {
        return [.. Enumerable.Range(1, count)
            .Select(i => new WeatherStation(
                i,
                $"Station {i}",
                $"Region {i}",
                52.0 + i,
                5.0 + i,
                15,
                70,
                "ZO",
                3,
                5,
                1015,
                10000,
                0,
                DateTimeOffset.UtcNow))];
    }

    [Fact]
    public async Task HandleAsync_MapsStationDataToGenerateImageMessage()
    {
        // Arrange
        var station = new WeatherStation(
            123,
            "De Bilt",
            "Utrecht",
            52.1,
            5.18,
            15.7,
            81,
            "ZZO",
            2.0,
            3.6,
            1016.8,
            48900,
            0,
            DateTimeOffset.UtcNow);

        _weatherStationProvider
            .GetStationsAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { station });

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _messagePublisher
            .Received(1)
            .PublishBatchAsync(
                Arg.Is<IReadOnlyCollection<GenerateImageMessage>>(messages =>
                    messages.Count == 1 &&
                    messages.Single().OperationId == _operationId &&
                    messages.Single().StationId == station.StationId &&
                    messages.Single().Name == station.Name &&
                    messages.Single().TemperatureCelsius == station.TemperatureCelsius),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenConfiguredCountExceedsAvailable_PublishesAllAvailable()
    {
        // Arrange
        _weatherStationProvider
            .GetStationsAsync(Arg.Any<CancellationToken>())
            .Returns(CreateStations(4));

        // Act
        var result = await _handler.HandleAsync(
            _operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _messagePublisher
            .Received(1)
            .PublishBatchAsync(
                Arg.Is<IReadOnlyCollection<GenerateImageMessage>>(
                    messages => messages.Count == 4),
                Arg.Any<CancellationToken>());
    }
}