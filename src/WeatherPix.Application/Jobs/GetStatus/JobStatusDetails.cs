using WeatherPix.Domain.Job;

namespace WeatherPix.Application.Jobs.GetStatus;

public record JobStatusDetails(
    Job Job,
    JobProgress Progress);