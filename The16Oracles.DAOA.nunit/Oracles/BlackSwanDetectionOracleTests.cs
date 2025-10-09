using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Oracles;

namespace The16Oracles.DAOA.nunit.Oracles;

[TestFixture]
public class BlackSwanDetectionOracleTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private BlackSwanDetectionOracle _oracle;

    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _oracle = new BlackSwanDetectionOracle(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [Test]
    public void Name_ShouldReturnCorrectValue()
    {
        // Assert
        Assert.That(_oracle.Name, Is.EqualTo("Black Swan Detection & Early Warning"));
    }

    [Test]
    public async Task EvaluateAsync_ShouldReturnValidOracleResult()
    {
        // Arrange
        var marketChartResponse = new
        {
            prices = GeneratePriceData(30, 50000.0, 0.02)
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
                Content = new StringContent(JsonSerializer.Serialize(marketChartResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ModuleName, Is.EqualTo("Black Swan Detection & Early Warning"));
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 0.0)); // Black swan scores are negative
        Assert.That(result.Metrics, Is.Not.Null);
        Assert.That(result.Metrics.ContainsKey("RealizedVolatility7d"), Is.True);
        Assert.That(result.Metrics.ContainsKey("RealizedVolatility30d"), Is.True);
        Assert.That(result.Metrics.ContainsKey("VolatilityRatio"), Is.True);
        Assert.That(result.Metrics.ContainsKey("VaR95"), Is.True);
        Assert.That(result.Metrics.ContainsKey("RawRiskIndex"), Is.True);
        Assert.That(result.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task EvaluateAsync_ShouldFetchFromCoinGeckoApi()
    {
        // Arrange
        var marketChartResponse = new
        {
            prices = GeneratePriceData(30, 50000.0, 0.02)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains("api.coingecko.com") &&
                    req.RequestUri.ToString().Contains("bitcoin/market_chart")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(marketChartResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - Should fetch both 7d and 30d charts
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.ToString().Contains("bitcoin/market_chart")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task EvaluateAsync_ShouldDetectHighVolatility()
    {
        // Arrange - High volatility scenario
        var highVolResponse = new
        {
            prices = GeneratePriceData(30, 50000.0, 0.15) // 15% volatility
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
                Content = new StringContent(JsonSerializer.Serialize(highVolResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - High volatility should result in negative or zero score
        Assert.That(result.ConfidenceScore, Is.LessThanOrEqualTo(0));
        Assert.That(result.Metrics["RealizedVolatility7d"], Is.GreaterThan(0));
        Assert.That(result.Metrics["RealizedVolatility30d"], Is.GreaterThan(0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateVolatilityRatio()
    {
        // Arrange
        var marketChartResponse = new
        {
            prices = GeneratePriceData(30, 50000.0, 0.03)
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
                Content = new StringContent(JsonSerializer.Serialize(marketChartResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        var ratio = (double)result.Metrics["VolatilityRatio"];
        Assert.That(ratio, Is.GreaterThan(0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateVaR95()
    {
        // Arrange
        var marketChartResponse = new
        {
            prices = GeneratePriceData(30, 50000.0, 0.05)
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
                Content = new StringContent(JsonSerializer.Serialize(marketChartResponse))
            });

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - VaR95 should be a positive number representing worst 5% loss
        var var95 = (double)result.Metrics["VaR95"];
        Assert.That(var95, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void EvaluateAsync_ShouldThrowException_WhenApiReturnsInvalidData()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _oracle.EvaluateAsync(new DataBundle()));
    }

    // Helper method to generate realistic price data
    private static List<List<double>> GeneratePriceData(int days, double basePrice, double volatility)
    {
        var prices = new List<List<double>>();
        var random = new Random(42); // Fixed seed for reproducible tests
        var timestamp = DateTimeOffset.UtcNow.AddDays(-days).ToUnixTimeMilliseconds();

        for (int i = 0; i < days * 24; i++) // Hourly data
        {
            var change = (random.NextDouble() - 0.5) * volatility;
            basePrice *= (1 + change);
            prices.Add(new List<double> { timestamp, basePrice });
            timestamp += 3600000; // 1 hour in milliseconds
        }

        return prices;
    }
}
