using Microsoft.Extensions.Logging;
using NSubstitute;
using WeatherPix.Application.Abstractions;
using WeatherPix.Application.Jobs.QueueJob;
using WeatherPix.Application.Messaging;
using WeatherPix.Domain.Job;
using Xunit;

namespace WeatherPix.Application.Tests.Jobs.QueueJobs;

public class QueueJobHandlerTests
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly IJobStatusRepository _jobStatusRepository;
    private readonly ILogger<QueueJobHandler> _logger;
    private readonly QueueJobHandler _handler;

    private const string UserId = "test-user";

    public QueueJobHandlerTests()
    {
        _messagePublisher = Substitute.For<IMessagePublisher>();
        _jobStatusRepository = Substitute.For<IJobStatusRepository>();
        _logger = Substitute.For<ILogger<QueueJobHandler>>();

        _handler = new QueueJobHandler(
            _messagePublisher,
            _jobStatusRepository,
            _logger);
    }

    [Fact]
    public async Task QueueJobAsync_WhenEverythingSucceeds_ReturnsOperationId()
    {
        // Act
        var result = await _handler.QueueJobAsync(
            UserId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _jobStatusRepository.Received(1)
            .CreateJobAsync(
                Arg.Is<Job>(job => job.OperationId == result.Value),
                Arg.Any<CancellationToken>());

        await _messagePublisher.Received(1)
            .PublishAsync(
                Arg.Is<StartJobMessage>(
                    message => message.OperationId == result.Value),
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
        var result = await _handler.QueueJobAsync(
            UserId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(JobErrors.CreationFailed.Code, result.Error!.Code);

        await _messagePublisher.DidNotReceive()
            .PublishAsync(
                Arg.Any<StartJobMessage>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueJobAsync_WhenQueuePublishFails_MarksJobFailedAndReturnsQueueFailed()
    {
        // Arrange
        _messagePublisher
            .PublishAsync(
                Arg.Any<StartJobMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Service Bus failed")));

        // Act
        var result = await _handler.QueueJobAsync(
            UserId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(JobErrors.QueueFailed.Code, result.Error!.Code);

        await _jobStatusRepository.Received(1)
            .UpdateStatusAsync(
                Arg.Any<Guid>(),
                JobStatus.Failed,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueJobAsync_WhenMarkFailedAlsoFails_StillReturnsQueueFailed()
    {
        // Arrange
        _messagePublisher
            .PublishAsync(
                Arg.Any<StartJobMessage>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Service Bus failed")));

        _jobStatusRepository
            .UpdateStatusAsync(
                Arg.Any<Guid>(),
                JobStatus.Failed,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(
                new Exception("Table Storage update failed")));

        // Act
        var result = await _handler.QueueJobAsync(
            UserId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(JobErrors.QueueFailed.Code, result.Error!.Code);

        await _jobStatusRepository.Received(1)
            .UpdateStatusAsync(
                Arg.Any<Guid>(),
                JobStatus.Failed,
                Arg.Any<CancellationToken>());
    }
}