using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Images.Handler;
using WeatherPix.Application.Images.QueryBuilding;
using WeatherPix.Application.Images.WeatherDetermination;
using WeatherPix.Application.Messaging;
using WeatherPix.Domain.Image;
using WeatherPix.Domain.Job;
using WeatherPix.Domain.WeatherStation;
using Xunit;

namespace WeatherPix.Application.Tests.Images;

public class GenerateWeatherImageHandlerTests
{
    private readonly IImageProvider _imageProvider;
    private readonly IWeatherImageRenderer _imageRenderer;
    private readonly IImageStorage _imageStorage;
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly IWeatherImageQueryBuilder _queryBuilder;
    private readonly IWeatherConditionResolver _weatherConditionResolver;
    private readonly ILogger<GenerateWeatherImageHandler> _logger;

    private readonly GenerateWeatherImageHandler _handler;

    public GenerateWeatherImageHandlerTests()
    {
        _imageProvider = Substitute.For<IImageProvider>();
        _imageRenderer = Substitute.For<IWeatherImageRenderer>();
        _imageStorage = Substitute.For<IImageStorage>();
        _jobStatusRepository = Substitute.For<IJobStatusRepository>();
        _queryBuilder = Substitute.For<IWeatherImageQueryBuilder>();
        _weatherConditionResolver = Substitute.For<IWeatherConditionResolver>();
        _logger = Substitute.For<ILogger<GenerateWeatherImageHandler>>();

        _weatherConditionResolver
            .Resolve(Arg.Any<WeatherStation>())
            .Returns(CreateWeatherConditions());

        _queryBuilder
            .Build(Arg.Any<WeatherConditions>())
            .Returns("overcast evening landscape");

        _handler = new GenerateWeatherImageHandler(
            _imageProvider,
            _imageRenderer,
            _imageStorage,
            _jobStatusRepository,
            _queryBuilder,
            _weatherConditionResolver,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenGenerationSucceeds_MarksStationSucceeded()
    {
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
                Arg.Any<WeatherStation>(),
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

        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

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

        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

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

        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

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

        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

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

        var result = await _handler.HandleAsync(
            message,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        await _jobStatusRepository.DidNotReceive()
            .UpdateStatusAsync(
                Arg.Any<Guid>(),
                Arg.Any<JobStatus>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UsesResolvedConditionsToBuildQuery()
    {
        var message = CreateMessage();

        SetupSuccessfulGeneration();

        var conditions = CreateWeatherConditions();

        _weatherConditionResolver
            .Resolve(Arg.Any<WeatherStation>())
            .Returns(conditions);

        _queryBuilder
            .Build(conditions)
            .Returns("overcast evening landscape");

        _jobStatusRepository
            .GetProgressAsync(
                message.OperationId,
                Arg.Any<CancellationToken>())
            .Returns(new JobProgress(
                Total: 5,
                Succeeded: 1,
                Failed: 0));

        await _handler.HandleAsync(
            message,
            CancellationToken.None);

        _weatherConditionResolver.Received(1)
            .Resolve(Arg.Any<WeatherStation>());

        _queryBuilder.Received(1)
            .Build(conditions);

        await _imageProvider.Received(1)
            .GetImageAsync(
                "overcast evening landscape",
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
                Arg.Any<WeatherStation>(),
                Arg.Any<CancellationToken>())
            .Returns(new MemoryStream([4, 5, 6]));
    }

    private static WeatherConditions CreateWeatherConditions()
    {
        return new WeatherConditions(
            SkyCondition.Overcast,
            PrecipitationCondition.None,
            WindCondition.Calm,
            VisibilityCondition.Clear,
            DayPeriod.Evening);
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