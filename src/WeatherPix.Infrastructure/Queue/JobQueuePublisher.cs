using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using WeatherPix.Application.Abstractions;
using WeatherPix.Domain.Jobs;
using WeatherPix.Infrastructure.Options.ServiceBus;
using WeatherPix.Infrastructure.Queue.Contracts;

namespace WeatherPix.Infrastructure.Queue;

public class JobQueuePublisher(ServiceBusClient client, IOptions<ServiceBusOptions> options) : IJobQueuePublisher
{
    private readonly ServiceBusSender _sender = 
        client.CreateSender(options.Value.StartJobsQueueName);

    public async Task PublishJobAsync(Job job, CancellationToken ct)
    {
        var payload = new StartJobMessage(job.OperationId);

        var message = new ServiceBusMessage(BinaryData.FromObjectAsJson(payload))
        {
            ContentType = "application/json",
            MessageId = job.OperationId.ToString(),
        };

        await _sender.SendMessageAsync(message, ct);
    }
}