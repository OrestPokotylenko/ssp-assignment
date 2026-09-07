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
    public async Task HandleAsync_WhenJobExists_ReturnsJob()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        var job = new Job
        {
            OperationId = operationId,
            Status = JobStatus.Processing,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _jobStatusRepository
            .GetJobAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(job);

        // Act
        var result = await _handler.HandleAsync(
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(operationId, result.Value.OperationId);
        Assert.Equal(JobStatus.Processing, result.Value.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenJobDoesNotExist_ReturnsSuccessWithNull()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        // Act
        var result = await _handler.HandleAsync(
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_ReturnsFailure()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Job?>(
                new Exception("Table Storage failed")));

        // Act
        var result = await _handler.HandleAsync(
            operationId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            JobErrors.StatusRetrievalFailed.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task HandleAsync_CallsRepositoryWithCorrectOperationId()
    {
        // Arrange
        var operationId = Guid.NewGuid();

        _jobStatusRepository
            .GetJobAsync(
                operationId,
                Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        // Act
        await _handler.HandleAsync(
            operationId,
            CancellationToken.None);

        // Assert
        await _jobStatusRepository
            .Received(1)
            .GetJobAsync(
                operationId,
                Arg.Any<CancellationToken>());
    }
}