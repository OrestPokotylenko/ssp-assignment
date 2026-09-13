using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Jobs.GetStatus;
using WeatherPix.Domain.Job;
using Xunit;

namespace WeatherPix.Application.Tests.Jobs.GetStatus;

public class GetJobStatusHandlerTests
{
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly ILogger<GetJobStatusHandler> _logger;
    private readonly GetJobStatusHandler _handler;

    private const string UserId = "test-user";

    public GetJobStatusHandlerTests()
    {
        _jobStatusRepository =
            Substitute.For<IJobStatusRepository>();

        _logger =
            Substitute.For<ILogger<GetJobStatusHandler>>();

        _handler = new GetJobStatusHandler(
            _jobStatusRepository,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenJobExists_ReturnsJobWithProgress()
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

        var progress = new JobProgress(
            Total: 40,
            Succeeded: 15,
            Failed: 2);

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(job);

        _jobStatusRepository
            .GetProgressAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(progress);

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            operationId,
            result.Value.Job.OperationId);

        Assert.Equal(
            JobStatus.Processing,
            result.Value.Job.Status);

        Assert.Equal(
            40,
            result.Value.Progress.Total);

        Assert.Equal(
            15,
            result.Value.Progress.Succeeded);

        Assert.Equal(
            2,
            result.Value.Progress.Failed);

        await _jobStatusRepository
            .Received(1)
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>());

        await _jobStatusRepository
            .Received(1)
            .GetProgressAsync(
                operationId,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenJobDoesNotExist_ReturnsSuccessWithNull()
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

        await _jobStatusRepository
            .DidNotReceive()
            .GetProgressAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                UserId,
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Job?>(
                new Exception("Table Storage failed")));

        // Act
        var result = await _handler.HandleAsync(
            UserId,
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        Assert.Equal(
            JobErrors.StatusRetrievalFailed.Code,
            result.Error!.Code);

        await _jobStatusRepository
            .DidNotReceive()
            .GetProgressAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
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

    [Fact]
    public async Task HandleAsync_WhenJobBelongsToAnotherUser_ReturnsSuccessWithNull()
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

        await _jobStatusRepository
            .DidNotReceive()
            .GetProgressAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }
}