using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Oracles;

namespace The16Oracles.DAOA.nunit.Oracles;

[TestFixture]
public class DeFiLiquidityMovementsOracleTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private DeFiLiquidityMovementsOracle _oracle;

    [SetUp]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _oracle = new DeFiLiquidityMovementsOracle(_httpClient);
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
        Assert.That(_oracle.Name, Is.EqualTo("DeFi Liquidity Movements"));
    }

    [Test]
    public async Task EvaluateAsync_ShouldReturnValidOracleResult()
    {
        // Arrange
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 105_000_000_000.0 }
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 10_000_000_000.0 },
            new { name = "Compound", category = "Lending", tvl = 5_000_000_000.0 },
            new { name = "Uniswap", category = "Dexes", tvl = 8_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ModuleName, Is.EqualTo("DeFi Liquidity Movements"));
        Assert.That(result.ConfidenceScore, Is.InRange(-1.0, 1.0));
        Assert.That(result.Metrics, Is.Not.Null);
        Assert.That(result.Metrics.ContainsKey("TotalTVL_USD"), Is.True);
        Assert.That(result.Metrics.ContainsKey("TVLShift1d_Pct"), Is.True);
        Assert.That(result.Metrics.ContainsKey("LendingTVL_USD"), Is.True);
        Assert.That(result.Metrics.ContainsKey("LendingRatio"), Is.True);
        Assert.That(result.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateTVLShiftPercentage()
    {
        // Arrange - 5% increase in TVL
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 105_000_000_000.0 }
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 15_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        var tvlShift = (double)result.Metrics["TVLShift1d_Pct"];
        Assert.That(tvlShift, Is.EqualTo(0.05).Within(0.001)); // 5% increase
        Assert.That(result.Metrics["TotalTVL_USD"], Is.EqualTo(105_000_000_000.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateLendingRatio()
    {
        // Arrange
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 100_000_000_000.0 }
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 10_000_000_000.0 },
            new { name = "Compound", category = "Lending", tvl = 5_000_000_000.0 },
            new { name = "Uniswap", category = "Dexes", tvl = 20_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert
        var lendingTvl = (double)result.Metrics["LendingTVL_USD"];
        var lendingRatio = (double)result.Metrics["LendingRatio"];
        Assert.That(lendingTvl, Is.EqualTo(15_000_000_000.0)); // Aave + Compound
        Assert.That(lendingRatio, Is.EqualTo(0.15).Within(0.001)); // 15B / 100B = 0.15
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculatePositiveScore_WhenTVLIncreases()
    {
        // Arrange - Positive TVL shift with lending ratio
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 110_000_000_000.0 }
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 50_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - Score should be positive (TVL increased)
        Assert.That(result.ConfidenceScore, Is.GreaterThan(0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldCalculateNegativeScore_WhenTVLDecreases()
    {
        // Arrange - Negative TVL shift
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 90_000_000_000.0 }
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 40_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - Score should be negative (TVL decreased)
        Assert.That(result.ConfidenceScore, Is.LessThan(0));
    }

    [Test]
    public void EvaluateAsync_ShouldThrowException_WhenNotEnoughTVLData()
    {
        // Arrange - Only one data point
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _oracle.EvaluateAsync(new DataBundle()));
    }

    [Test]
    public async Task EvaluateAsync_ShouldFilterOnlyLendingProtocols()
    {
        // Arrange
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 100_000_000_000.0 }
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 10_000_000_000.0 },
            new { name = "Compound", category = "lending", tvl = 5_000_000_000.0 }, // lowercase
            new { name = "Uniswap", category = "Dexes", tvl = 20_000_000_000.0 },
            new { name = "MakerDAO", category = "CDP", tvl = 15_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - Should only include Aave and Compound (case insensitive)
        var lendingTvl = (double)result.Metrics["LendingTVL_USD"];
        Assert.That(lendingTvl, Is.EqualTo(15_000_000_000.0));
    }

    [Test]
    public async Task EvaluateAsync_ShouldClampScoreBetweenNegativeOneAndOne()
    {
        // Arrange - Extreme TVL shift that would exceed bounds
        var tvlHistory = new[]
        {
            new { date = 1640000000, totalLiquidityUSD = 100_000_000_000.0 },
            new { date = 1640086400, totalLiquidityUSD = 300_000_000_000.0 } // 200% increase
        };

        var protocols = new[]
        {
            new { name = "Aave", category = "Lending", tvl = 150_000_000_000.0 }
        };

        SetupHttpResponse("https://api.llama.fi/charts", tvlHistory);
        SetupHttpResponse("https://api.llama.fi/protocols", protocols);

        // Act
        var result = await _oracle.EvaluateAsync(new DataBundle());

        // Assert - Score should be clamped to 1.0
        Assert.That(result.ConfidenceScore, Is.LessThanOrEqualTo(1.0));
        Assert.That(result.ConfidenceScore, Is.GreaterThanOrEqualTo(-1.0));
    }

    private void SetupHttpResponse(string url, object responseObject)
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null && req.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(responseObject))
            });
    }
}
