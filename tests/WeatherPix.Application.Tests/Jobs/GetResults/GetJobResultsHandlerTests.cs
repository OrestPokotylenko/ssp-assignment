using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Jobs.GetResults;
using WeatherPix.Domain.Image;
using WeatherPix.Domain.Job;
using Xunit;

namespace WeatherPix.Application.Tests.Jobs.GetResults;

public class GetJobResultsHandlerTests
{
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly IImageStorage _imageStorage;
    private readonly ILogger<GetJobResultsHandler> _logger;
    private readonly GetJobResultsHandler _handler;

    private const string UserId = "test-user";

    public GetJobResultsHandlerTests()
    {
        _jobStatusRepository = Substitute.For<IJobStatusRepository>();
        _imageStorage = Substitute.For<IImageStorage>();
        _logger = Substitute.For<ILogger<GetJobResultsHandler>>();

        _handler = new GetJobResultsHandler(
            _jobStatusRepository,
            _imageStorage,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenJobExists_ReturnsImages()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        var job = new Job
        {
            OperationId = operationId,
            OwnerId = UserId,
            Status = JobStatus.Succeeded,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var images = new List<GeneratedImage>
        {
            new(6275, new Uri("https://example.com/6275.jpg")),
            new(6249, new Uri("https://example.com/6249.jpg"))
        };

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(job);

        _imageStorage
            .GetImagesAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(images);

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(operationId, result.Value.OperationId);
        Assert.Equal(JobStatus.Succeeded, result.Value.Status);
        Assert.Equal(2, result.Value.Images.Count);
    }

    [Fact]
    public async Task HandleAsync_WhenJobDoesNotExist_ReturnsNull()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);

        await _imageStorage
            .DidNotReceive()
            .GetImagesAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenJobBelongsToAnotherUser_ReturnsNull()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);

        await _imageStorage
            .DidNotReceive()
            .GetImagesAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenStorageFails_ReturnsFailure()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        var job = new Job
        {
            OperationId = operationId,
            OwnerId = UserId,
            Status = JobStatus.Succeeded,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(job);

        _imageStorage
            .GetImagesAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyCollection<GeneratedImage>>>(
                _ => throw new Exception("Blob failure"));

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            JobErrors.ResultsRetrievalFailed.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenNoImagesExist_ReturnsEmptyCollection()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        var job = new Job
        {
            OperationId = operationId,
            OwnerId = UserId,
            Status = JobStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(job);

        _imageStorage
            .GetImagesAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<GeneratedImage>());

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(operationId, result.Value.OperationId);
        Assert.Equal(JobStatus.Processing, result.Value.Status);
        Assert.Empty(result.Value.Images);
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryWithCorrectUserIdAndOperationId()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        // Act
        await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        await _jobStatusRepository
            .Received(1)
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>());
    }
}