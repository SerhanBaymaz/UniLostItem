using System.Text.Json;
using API.Extensions;
using API.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Xunit;

namespace Tests.API_Tests.Extensions;

public class RateLimitingExtensionsTests : IDisposable
{
    public RateLimitingExtensionsTests()
    {
        // Setup a no-op logger to prevent null reference or side effects in static Log class
        Log.Logger = new LoggerConfiguration().CreateLogger();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Log.CloseAndFlush();
        }
    }

    [Fact]
    public void AddRateLimitingServices_ShouldRegisterRateLimiterOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string> {
            {"RateLimiting:PermitLimit", "50"},
            {"RateLimiting:WindowSeconds", "30"},
            {"RateLimiting:QueueLimit", "10"},
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        // Act
        services.AddRateLimitingServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<RateLimiterOptions>>();

        // Assert
        options.Should().NotBeNull();
        options!.Value.Should().NotBeNull();
        options.Value.RejectionStatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        options.Value.GlobalLimiter.Should().NotBeNull();
        options.Value.OnRejected.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleRejected_ShouldSetResponseCorrectly()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";
        // UserAgent is a StringValues, so we can set it via Headers
        context.Request.Headers.UserAgent = "TestUserAgent";
        // Setup RemoteIpAddress
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        int permitLimit = 100;
        int windowSeconds = 60;

        // Act
        await RateLimitingExtensions.HandleRejected(context, CancellationToken.None, permitLimit, windowSeconds);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        context.Response.ContentType.Should().StartWith("application/json");

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var response = JsonSerializer.Deserialize<StandardApiResponse<object>>(body, options);

        response.Should().NotBeNull();
        response!.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Rate limit exceeded. Please try again later.");
        
        response.Error.Should().NotBeNull();
        response.Error!.Detail.Should().Be("You have sent too many requests in a given amount of time.");
        response.Error.Type.Should().Be("https://tools.ietf.org/html/rfc6585#section-4");
    }
}
