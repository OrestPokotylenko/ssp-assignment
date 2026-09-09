using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Security.Claims;

namespace WeatherPix.Functions.Tests.Helpers.Auth;

internal class TestHttpRequestData(
    FunctionContext functionContext,
    HttpHeadersCollection headers)
    : HttpRequestData(functionContext)
{
    public override Stream Body => Stream.Null;

    public override HttpHeadersCollection Headers { get; } = headers;

    public override IReadOnlyCollection<IHttpCookie> Cookies => [];

    public override Uri Url =>
        new("https://localhost");

    public override IEnumerable<ClaimsIdentity> Identities => [];

    public override string Method => "GET";

    public override HttpResponseData CreateResponse()
    {
        throw new NotImplementedException();
    }
}