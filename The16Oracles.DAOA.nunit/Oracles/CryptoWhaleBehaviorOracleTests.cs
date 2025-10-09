using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Oracles;

namespace The16Oracles.DAOA.nunit.Oracles;

[TestFixture]
public class CryptoWhaleBehaviorOracleTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private Mock<IConfiguration> _mockConfiguration;
    private CryptoWhaleBehaviorOracle _oracle;

    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["WhaleBehavior:ApiKey"]).Returns("test-api-key");
        _mockConfiguration.Setup(c => c.GetSection("WhaleBehavior:MinValueUsd").Value).Returns("1000000");
        _mockConfiguration.Setup(c => c.GetSection("WhaleBehavior:MaxFlowUsdCap").Value).Returns("1000000000");

        _oracle = new CryptoWhaleBehaviorOracle(_httpClient, _mockConfiguration.Object);
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
        mockConfig.Setup(c => c["WhaleBehavior:ApiKey"]).Returns((string)null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new CryptoWhaleBehaviorOracle(_httpClient, mockConfig.Object));
    }

    [Test]
    public void Name_ShouldReturnCorrectValue()
    {
        // Assert
        Assert.That(_oracle.Name, Is.EqualTo("Crypto Whale Behavior"));
    }

    [Test]
    public async Task EvaluateAsync_ShouldReturnValidOracleResult()
    {
        // Arrange
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 5_000_000.0,
                    from = new { address = "0x123", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ModuleName, Is.EqualTo("Crypto Whale Behavior"));
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 1.0));
        Assert.That(result.Metrics, Is.Not.Null);
        Assert.That(result.Metrics.ContainsKey("TotalTransfers"), Is.True);
        Assert.That(result.Metrics.ContainsKey("TotalVolumeUSD"), Is.True);
        Assert.That(result.Metrics.ContainsKey("AccumulationUSD"), Is.True);
        Assert.That(result.Metrics.ContainsKey("DistributionUSD"), Is.True);
        Assert.That(result.Metrics.ContainsKey("NetFlowUSD"), Is.True);
        Assert.That(result.Metrics.ContainsKey("DistinctSources"), Is.True);
        Assert.That(result.Metrics.ContainsKey("ClusterRatio"), Is.True);
        Assert.That(result.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateAccumulationFromExchangeToNonExchange()
    {
        // Arrange - Whale withdrawing from exchange (accumulation)
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 10_000_000.0,
                    from = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" },
                    to = new { address = "0xWhale", owner = "Unknown", owner_type = "unknown" }
                },
                new
                {
                    amount_usd = 5_000_000.0,
                    from = new { address = "0xCoinbase", owner = "Coinbase", owner_type = "exchange" },
                    to = new { address = "0xWhale2", owner = "Unknown", owner_type = "unknown" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result.Metrics["AccumulationUSD"], Is.EqualTo(15_000_000.0));
        Assert.That(result.Metrics["DistributionUSD"], Is.EqualTo(0.0));
        Assert.That(result.Metrics["NetFlowUSD"], Is.EqualTo(15_000_000.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateDistributionFromNonExchangeToExchange()
    {
        // Arrange - Whale depositing to exchange (distribution)
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 8_000_000.0,
                    from = new { address = "0xWhale", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 4_000_000.0,
                    from = new { address = "0xWhale2", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xCoinbase", owner = "Coinbase", owner_type = "exchange" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result.Metrics["AccumulationUSD"], Is.EqualTo(0.0));
        Assert.That(result.Metrics["DistributionUSD"], Is.EqualTo(12_000_000.0));
        Assert.That(result.Metrics["NetFlowUSD"], Is.EqualTo(-12_000_000.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateNetFlow()
    {
        // Arrange - Mixed accumulation and distribution
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 10_000_000.0,
                    from = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" },
                    to = new { address = "0xWhale", owner = "Unknown", owner_type = "unknown" }
                },
                new
                {
                    amount_usd = 3_000_000.0,
                    from = new { address = "0xWhale2", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xCoinbase", owner = "Coinbase", owner_type = "exchange" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - Net flow should be 10M - 3M = 7M (positive = accumulation)
        Assert.That(result.Metrics["NetFlowUSD"], Is.EqualTo(7_000_000.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateClusterRatio()
    {
        // Arrange - 5 transactions from 2 distinct sources (high clustering)
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 1_000_000.0,
                    from = new { address = "0xWhale1", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 1_000_000.0,
                    from = new { address = "0xWhale1", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xCoinbase", owner = "Coinbase", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 1_000_000.0,
                    from = new { address = "0xWhale1", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xKraken", owner = "Kraken", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 1_000_000.0,
                    from = new { address = "0xWhale2", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 1_000_000.0,
                    from = new { address = "0xWhale2", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xCoinbase", owner = "Coinbase", owner_type = "exchange" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - ClusterRatio = 1 - (2/5) = 0.6
        Assert.That(result.Metrics["DistinctSources"], Is.EqualTo(2));
        Assert.That(result.Metrics["ClusterRatio"], Is.EqualTo(0.6).Within(0.001));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateTotalMetrics()
    {
        // Arrange
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 5_000_000.0,
                    from = new { address = "0xWhale1", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xBinance", owner = "Binance", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 3_000_000.0,
                    from = new { address = "0xWhale2", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xCoinbase", owner = "Coinbase", owner_type = "exchange" }
                },
                new
                {
                    amount_usd = 2_000_000.0,
                    from = new { address = "0xWhale3", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xKraken", owner = "Kraken", owner_type = "exchange" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result.Metrics["TotalTransfers"], Is.EqualTo(3));
        Assert.That(result.Metrics["TotalVolumeUSD"], Is.EqualTo(10_000_000.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldClampScoreBetweenNegativeOneAndOne()
    {
        // Arrange - Extreme net flow scenario
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 999_999_999_999.0,
                    from = new { address = "0xWhale", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0xWhale", owner = "Unknown", owner_type = "unknown" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 1.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldFetchFromWhaleAlertApi()
    {
        // Arrange
        var whaleResponse = new
        {
            transactions = new[]
            {
                new
                {
                    amount_usd = 1_000_000.0,
                    from = new { address = "0x123", owner = "Unknown", owner_type = "unknown" },
                    to = new { address = "0x456", owner = "Unknown", owner_type = "unknown" }
                }
            }
        };

        SetupWhaleAlertResponse(whaleResponse);

        // Act
        await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.ToString().Contains("api.whale-alert.io") &&
                req.RequestUri.ToString().Contains("test-api-key")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public void EvaluateAsync_ShouldThrowException_WhenApiReturnsNull()
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
                Content = new StringContent("null")
            });

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _oracle.EvaluateAsync(new DataBundle()));
    }

    private void SetupWhaleAlertResponse(object responseObject)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseObject))
            });
    }
}
