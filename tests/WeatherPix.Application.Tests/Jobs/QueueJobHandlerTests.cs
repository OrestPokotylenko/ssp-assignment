using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Jobs;
using WeatherPix.Domain.Jobs;
using Xunit;

namespace WeatherPix.Application.Tests.Jobs;

public class QueueJobHandlerTests
{
    private readonly IJobQueuePublisher _queuePublisher;
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly ILogger<QueueJobHandler> _logger;
    private readonly QueueJobHandler _handler;

    public QueueJobHandlerTests()
    {
        _queuePublisher = Substitute.For<IJobQueuePublisher>();
        _jobStatusRepository = Substitute.For<IJobStatusRepository>();
        _logger = Substitute.For<ILogger<QueueJobHandler>>();

        _handler = new QueueJobHandler(
            _queuePublisher,
            _jobStatusRepository,
            _logger);
    }

    [Fact]
    public async Task QueueJobAsync_WhenEverythingSucceeds_ReturnsOperationId()
    {
        // Act
        var result = await _handler.QueueJobAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _jobStatusRepository.Received(1)
            .CreateJobAsync(
                Arg.Is<Job>(job => job.OperationId == result.Value),
                Arg.Any<CancellationToken>());

        await _queuePublisher.Received(1)
            .PublishJobAsync(
                Arg.Is<Job>(job => job.OperationId == result.Value),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueJobAsync_WhenJobCreationFails_ReturnsCreationFailed()
    {
        // Arrange
        _jobStatusRepository
            .CreateJobAsync(
                Arg.Any<Job>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Table Storage failed")));

        // Act
        var result = await _handler.QueueJobAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(JobErrors.CreationFailed.Code, result.Error!.Code);

        await _queuePublisher.DidNotReceive()
            .PublishJobAsync(
                Arg.Any<Job>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueJobAsync_WhenQueuePublishFails_MarksJobFailedAndReturnsQueueFailed()
    {
        // Arrange
        _queuePublisher
            .PublishJobAsync(
                Arg.Any<Job>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Service Bus failed")));

        // Act
        var result = await _handler.QueueJobAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(JobErrors.QueueFailed.Code, result.Error!.Code);

        await _jobStatusRepository.Received(1)
            .MarkFailedAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueJobAsync_WhenMarkFailedAlsoFails_StillReturnsQueueFailed()
    {
        // Arrange
        _queuePublisher
            .PublishJobAsync(
                Arg.Any<Job>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Service Bus failed")));

        _jobStatusRepository
            .MarkFailedAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Table Storage update failed")));

        // Act
        var result = await _handler.QueueJobAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(JobErrors.QueueFailed.Code, result.Error!.Code);

        await _jobStatusRepository.Received(1)
            .MarkFailedAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }
}