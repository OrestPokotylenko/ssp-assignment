using WeatherPix.Application.Common.Results;

namespace WeatherPix.Application.Jobs.GetResults;

public static class JobErrors
{
    public static readonly Error ResultsRetrievalFailed = new(
        "Job.ResultsRetrievalFailed",
        "Failed to retrieve job results.");
}