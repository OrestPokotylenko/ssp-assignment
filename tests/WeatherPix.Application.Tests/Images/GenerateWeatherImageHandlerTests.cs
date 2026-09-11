using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Images.Handler;
using WeatherPix.Application.Messaging;
using WeatherPix.Domain.Job;
using Xunit;

namespace WeatherPix.Application.Tests.Images;

public class GenerateWeatherImageHandlerTests
{
    private readonly IImageProvider _imageProvider;
    private readonly IWeatherImageRenderer _imageRenderer;
    private readonly IImageStorage _imageStorage;
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly ILogger<GenerateWeatherImageHandler> _logger;

    private readonly GenerateWeatherImageHandler _handler;

    public GenerateWeatherImageHandlerTests()
    {
        _imageProvider = Substitute.For<IImageProvider>();
        _imageRenderer = Substitute.For<IWeatherImageRenderer>();
        _imageStorage = Substitute.For<IImageStorage>();
        _jobStatusRepository = Substitute.For<IJobStatusRepository>();
        _logger = Substitute.For<ILogger<GenerateWeatherImageHandler>>();

        _handler = new GenerateWeatherImageHandler(
            _imageProvider,
            _imageRenderer,
            _imageStorage,
            _jobStatusRepository,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenGenerationSucceeds_MarksStationSucceeded()
    {
        // Arrange
        var message = CreateMessage();

        var sourceStream = new MemoryStream([1, 2, 3]);
        var renderedStream = new MemoryStream([4, 5, 6]);

        _imageProvider
            .GetImageAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(sourceStream);

        _imageRenderer
            .RenderAsync(
                Arg.Any<Stream>(),
                Arg.Any<WeatherPix.Domain.WeatherStation.WeatherStation>(),
                Arg.Any<CancellationToken>())
            .Returns(renderedStream);

        _jobStatusRepository
            .GetProgressAsync(
                message.OperationId,
                Arg.Any<CancellationToken>())
            .Returns(new JobProgress(
                Total: 5,
                Succeeded: 1,
                Failed: 0));

        // Act
        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _jobStatusRepository.Received(1)
            .UpdateStationStatusAsync(
                message.OperationId,
                message.StationId,
                JobStatus.Processing,
                Arg.Any<CancellationToken>());

        await _jobStatusRepository.Received(1)
            .UpdateStationStatusAsync(
                message.OperationId,
                message.StationId,
                JobStatus.Succeeded,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenGenerationFails_MarksStationFailed()
    {
        // Arrange
        var message = CreateMessage();

        _imageProvider
            .GetImageAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Stream>(
                new Exception("Pexels failed")));

        _jobStatusRepository
            .GetProgressAsync(
                message.OperationId,
                Arg.Any<CancellationToken>())
            .Returns(new JobProgress(
                Total: 5,
                Succeeded: 0,
                Failed: 1));

        // Act
        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        await _jobStatusRepository.Received(1)
            .UpdateStationStatusAsync(
                message.OperationId,
                message.StationId,
                JobStatus.Failed,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAllStationsSucceeded_MarksJobSucceeded()
    {
        // Arrange
        var message = CreateMessage();

        SetupSuccessfulGeneration();

        _jobStatusRepository
            .GetProgressAsync(
                message.OperationId,
                Arg.Any<CancellationToken>())
            .Returns(new JobProgress(
                Total: 5,
                Succeeded: 5,
                Failed: 0));

        // Act
        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _jobStatusRepository.Received(1)
            .UpdateStatusAsync(
                message.OperationId,
                JobStatus.Succeeded,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAllStationsFinishedWithFailure_MarksJobFailed()
    {
        // Arrange
        var message = CreateMessage();

        SetupSuccessfulGeneration();

        _jobStatusRepository
            .GetProgressAsync(
                message.OperationId,
                Arg.Any<CancellationToken>())
            .Returns(new JobProgress(
                Total: 5,
                Succeeded: 4,
                Failed: 1));

        // Act
        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _jobStatusRepository.Received(1)
            .UpdateStatusAsync(
                message.OperationId,
                JobStatus.Failed,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStationsStillProcessing_DoesNotUpdateJobStatus()
    {
        // Arrange
        var message = CreateMessage();

        SetupSuccessfulGeneration();

        _jobStatusRepository
            .GetProgressAsync(
                message.OperationId,
                Arg.Any<CancellationToken>())
            .Returns(new JobProgress(
                Total: 5,
                Succeeded: 2,
                Failed: 0));

        // Act
        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _jobStatusRepository.DidNotReceive()
            .UpdateStatusAsync(
                Arg.Any<Guid>(),
                Arg.Any<JobStatus>(),
                Arg.Any<CancellationToken>());
    }

    private void SetupSuccessfulGeneration()
    {
        _imageProvider
            .GetImageAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new MemoryStream([1, 2, 3]));

        _imageRenderer
            .RenderAsync(
                Arg.Any<Stream>(),
                Arg.Any<WeatherPix.Domain.WeatherStation.WeatherStation>(),
                Arg.Any<CancellationToken>())
            .Returns(new MemoryStream([4, 5, 6]));
    }

    private static GenerateImageMessage CreateMessage()
    {
        return new GenerateImageMessage(
            Guid.NewGuid(),
            6275,
            "Meetstation Arnhem",
            52.07,
            5.88,
            "Arnhem",
            DateTimeOffset.UtcNow,
            "Zwaar bewolkt",
            "OZO",
            1017.4,
            16.8,
            16.8,
            49900,
            3.3,
            1.9,
            67);
    }
}