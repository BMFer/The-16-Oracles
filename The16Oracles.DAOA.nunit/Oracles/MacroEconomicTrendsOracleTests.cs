using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Oracles;

namespace The16Oracles.DAOA.nunit.Oracles;

[TestFixture]
public class MacroEconomicTrendsOracleTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private Mock<IConfiguration> _mockConfiguration;
    private MacroEconomicTrendsOracle _oracle;

    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["FRED:ApiKey"]).Returns("test-api-key");

        _oracle = new MacroEconomicTrendsOracle(_httpClient, _mockConfiguration.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public void Constructor_ShouldThrowException_WhenApiKeyIsMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["FRED:ApiKey"]).Returns((string)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new MacroEconomicTrendsOracle(_httpClient, mockConfig.Object));
    }

    [Test]
    public void Name_ShouldReturnCorrectValue()
    {
        // Assert
        Assert.That(_oracle.Name, Is.EqualTo("Macro Economics Trends"));
    }

    [Test]
    public async Task EvaluateAsync_ShouldReturnValidOracleResult()
    {
        // Arrange
        var fredResponse = new
        {
            observations = new[]
            {
                new { date = "2025-01-01", value = "4.5" }
            }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(fredResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ModuleName, Is.EqualTo("Macro Economics Trends"));
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 1.0));
        Assert.That(result.Metrics, Is.Not.Null);
        Assert.That(result.Metrics.ContainsKey("InterestRate10Y"), Is.True);
        Assert.That(result.Metrics.ContainsKey("CPI"), Is.True);
        Assert.That(result.Metrics.ContainsKey("GDP"), Is.True);
        Assert.That(result.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task EvaluateAsync_ShouldFetchDataFromFredApi()
    {
        // Arrange
        var fredResponse = new
        {
            observations = new[]
            {
                new { date = "2025-01-01", value = "3.8" }
            }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("api.stlouisfed.org") &&
                    req.RequestUri.ToString().Contains("test-api-key")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(fredResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.AtLeast(3), // Should fetch DGS10, CPIAUCSL, and GDP
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public void EvaluateAsync_ShouldThrowException_WhenFredReturnsInvalidData()
    {
        // Arrange
        var fredResponse = new
        {
            observations = new[]
            {
                new { date = "2025-01-01", value = "." } // Invalid value
            }
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(fredResponse))
            });

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _oracle.EvaluateAsync(new DataBundle()));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateConfidenceScore()
    {
        // Arrange - Setup specific values to test score calculation
        var setupResponse = (string seriesId) =>
        {
            var value = seriesId switch
            {
                "DGS10" => "2.5",      // Interest rate
                "CPIAUCSL" => "150.0", // CPI
                "GDP" => "20000.0",    // GDP
                _ => "0"
            };

            return new
            {
                observations = new[]
                {
                    new { date = "2025-01-01", value = value }
                }
            };
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("DGS10")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(setupResponse("DGS10")))
            });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("CPIAUCSL")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(setupResponse("CPIAUCSL")))
            });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("GDP")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(setupResponse("GDP")))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 1.0));
        Assert.That(result.Metrics["InterestRate10Y"], Is.EqualTo(2.5));
        Assert.That(result.Metrics["CPI"], Is.EqualTo(150.0));
        Assert.That(result.Metrics["GDP"], Is.EqualTo(20000.0));
    }
}
