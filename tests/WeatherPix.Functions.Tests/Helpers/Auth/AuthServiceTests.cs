using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using WeatherPix.Functions.Helpers.Auth;
using WeatherPix.Functions.Helpers.Auth.Options;
using FunctionHttpRequestData = WeatherPix.Functions.Tests.Helpers.Auth.TestHttpRequestData;

namespace WeatherPix.Functions.Tests.Helpers.Auth;

public class AuthServiceTests
{
    private const string Domain = "weatherpix-test.auth0.com";
    private const string Audience = "api://weatherpix";
    private const string ReadPermission = "jobs:read";
    private const string CreatePermission = "jobs:create";
    private const string Subject = "test-client";

    private readonly IConfigurationManager<OpenIdConnectConfiguration>
        _configurationManager;

    private readonly RSA _rsa;
    private readonly RsaSecurityKey _securityKey;

    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _rsa = RSA.Create(2048);

        _securityKey = new RsaSecurityKey(_rsa)
        {
            KeyId = Guid.NewGuid().ToString()
        };

        _configurationManager =
            Substitute.For<IConfigurationManager<OpenIdConnectConfiguration>>();

        _configurationManager
            .GetConfigurationAsync(Arg.Any<CancellationToken>())
            .Returns(CreateOpenIdConfiguration());

        var options = Options.Create(
            new AuthOptions
            {
                Domain = Domain,
                Audience = Audience,
                ReadPermission = ReadPermission,
                CreatePermission = CreatePermission
            });

        _authService = new AuthService(
            options,
            _configurationManager);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenAuthorizationHeaderIsMissing_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.Unauthorized,
            result.StatusCode);
        Assert.Null(result.Subject);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenTokenIsInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateRequest(
            "Bearer invalid-token");

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.Unauthorized,
            result.StatusCode);
        Assert.Null(result.Subject);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenRequiredScopeIsMissing_ReturnsForbidden()
    {
        // Arrange
        var token = CreateToken(
            scopes:
            [
                CreatePermission
            ]);

        var request = CreateRequest(
            $"Bearer {token}");

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.Forbidden,
            result.StatusCode);
        Assert.Null(result.Subject);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenTokenAndScopeAreValid_ReturnsSuccess()
    {
        // Arrange
        var token = CreateToken(
            scopes:
            [
                CreatePermission,
                ReadPermission
            ]);

        var request = CreateRequest(
            $"Bearer {token}");

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            result.StatusCode);
        Assert.Equal(
            Subject,
            result.Subject);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenSubjectIsMissing_ReturnsUnauthorized()
    {
        // Arrange
        var token = CreateToken(
            scopes:
            [
                ReadPermission
            ],
            subject: null);

        var request = CreateRequest(
            $"Bearer {token}");

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.Unauthorized,
            result.StatusCode);
        Assert.Null(result.Subject);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenTokenIsExpired_ReturnsUnauthorized()
    {
        // Arrange
        var token = CreateToken(
            scopes:
            [
                ReadPermission
            ],
            notBefore: DateTime.UtcNow.AddMinutes(-10),
            expires: DateTime.UtcNow.AddMinutes(-5));

        var request = CreateRequest(
            $"Bearer {token}");

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.Unauthorized,
            result.StatusCode);
        Assert.Null(result.Subject);
    }

    [Fact]
    public async Task AuthorizeAsync_WhenAudienceIsInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var token = CreateToken(
            scopes:
            [
                ReadPermission
            ],
            audience: "api://wrong-api");

        var request = CreateRequest(
            $"Bearer {token}");

        // Act
        var result = await _authService.AuthorizeAsync(
            request,
            ReadPermission,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(
            System.Net.HttpStatusCode.Unauthorized,
            result.StatusCode);
        Assert.Null(result.Subject);
    }

    private OpenIdConnectConfiguration CreateOpenIdConfiguration()
    {
        var configuration =
            new OpenIdConnectConfiguration
            {
                Issuer = $"https://{Domain}/"
            };

        configuration.SigningKeys.Add(_securityKey);

        return configuration;
    }

    private string CreateToken(
        IReadOnlyCollection<string> scopes,
        DateTime? notBefore = null,
        DateTime? expires = null,
        string? audience = null,
        string? subject = Subject)
    {
        var credentials =
            new SigningCredentials(
                _securityKey,
                SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(
                "scope",
                string.Join(' ', scopes))
        };

        if (subject is not null)
        {
            claims.Add(
                new Claim(
                    "sub",
                    subject));
        }

        var now = DateTime.UtcNow;

        var token = new JwtSecurityToken(
            issuer: $"https://{Domain}/",
            audience: audience ?? Audience,
            claims: claims,
            notBefore: notBefore ?? now.AddMinutes(-1),
            expires: expires ?? now.AddMinutes(30),
            signingCredentials: credentials);

        token.Header["kid"] =
            _securityKey.KeyId;

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private static FunctionHttpRequestData CreateRequest(
        string? authorizationHeader = null)
    {
        var context =
            Substitute.For<FunctionContext>();

        var headers =
            new HttpHeadersCollection();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            headers.Add(
                "Authorization",
                authorizationHeader);
        }

        return new TestHttpRequestData(
            context,
            headers);
    }
}