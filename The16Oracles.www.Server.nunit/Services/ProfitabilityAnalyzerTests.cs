using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using The16Oracles.www.Server.Models;
using The16Oracles.www.Server.Services;

namespace The16Oracles.www.Server.nunit.Services;

[TestFixture]
public class ProfitabilityAnalyzerTests
{
    private Mock<IJupiterApiService> _mockJupiterApi;
    private Mock<ILogger<ProfitabilityAnalyzer>> _mockLogger;
    private TradeBotConfiguration _config;
    private ProfitabilityAnalyzer _analyzer;

    [SetUp]
    public void Setup()
    {
        _mockJupiterApi = new Mock<IJupiterApiService>();
        _mockLogger = new Mock<ILogger<ProfitabilityAnalyzer>>();

        _config = new TradeBotConfiguration
        {
            TradingPairs =
            [
                new TradingPairConfiguration
                {
                    Id = "usdc-sol",
                    StableCoinMint = "USDC_MINT",
                    TargetTokenMint = "SOL_MINT",
                    ProfitabilityRank = 1,
                    Enabled = true,
                    CurrentProfitabilityScore = 0,
                    RiskManagement = new RiskManagementConfiguration
                    {
                        SlippageBps = 30
                    }
                },
                new TradingPairConfiguration
                {
                    Id = "usdt-sol",
                    StableCoinMint = "USDT_MINT",
                    TargetTokenMint = "SOL_MINT",
                    ProfitabilityRank = 2,
                    Enabled = true,
                    CurrentProfitabilityScore = 0,
                    RiskManagement = new RiskManagementConfiguration
                    {
                        SlippageBps = 30
                    }
                },
                new TradingPairConfiguration
                {
                    Id = "disabled-pair",
                    StableCoinMint = "DAI_MINT",
                    TargetTokenMint = "SOL_MINT",
                    ProfitabilityRank = 3,
                    Enabled = false,
                    CurrentProfitabilityScore = 0,
                    RiskManagement = new RiskManagementConfiguration
                    {
                        SlippageBps = 30
                    }
                }
            ]
        };

        var options = Options.Create(_config);
        _analyzer = new ProfitabilityAnalyzer(
            _mockJupiterApi.Object,
            options,
            _mockLogger.Object);
    }

    [Test]
    public async Task CalculateProfitabilityScoreAsync_WithLowPriceImpact_ReturnsHighScore()
    {
        // Arrange
        var pairId = "usdc-sol";
        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDC_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "1000000000",
            PriceImpactPct = 0.1m // Low price impact
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        // Act
        var score = await _analyzer.CalculateProfitabilityScoreAsync(pairId);

        // Assert
        Assert.That(score, Is.GreaterThan(90m)); // 100 - (0.1 * 10) = 99
        Assert.That(score, Is.LessThanOrEqualTo(100m));
    }

    [Test]
    public async Task CalculateProfitabilityScoreAsync_WithHighPriceImpact_ReturnsLowScore()
    {
        // Arrange
        var pairId = "usdc-sol";
        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDC_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "900000000",
            PriceImpactPct = 5.0m // High price impact
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        // Act
        var score = await _analyzer.CalculateProfitabilityScoreAsync(pairId);

        // Assert
        Assert.That(score, Is.LessThan(60m)); // 100 - (5.0 * 10) = 50
        Assert.That(score, Is.GreaterThanOrEqualTo(0m));
    }

    [Test]
    public async Task CalculateProfitabilityScoreAsync_WithNonexistentPair_ReturnsZero()
    {
        // Arrange
        var pairId = "nonexistent";

        // Act
        var score = await _analyzer.CalculateProfitabilityScoreAsync(pairId);

        // Assert
        Assert.That(score, Is.EqualTo(0m));
    }

    [Test]
    public async Task CalculateProfitabilityScoreAsync_WhenApiThrows_ReturnsZero()
    {
        // Arrange
        var pairId = "usdc-sol";
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var score = await _analyzer.CalculateProfitabilityScoreAsync(pairId);

        // Assert
        Assert.That(score, Is.EqualTo(0m));
    }

    [Test]
    public async Task CalculateProfitabilityScoreAsync_ScoreBounds_NeverExceedsRange()
    {
        // Arrange
        var pairId = "usdc-sol";
        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDC_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "500000000",
            PriceImpactPct = 50.0m // Extremely high price impact
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        // Act
        var score = await _analyzer.CalculateProfitabilityScoreAsync(pairId);

        // Assert
        Assert.That(score, Is.GreaterThanOrEqualTo(0m));
        Assert.That(score, Is.LessThanOrEqualTo(100m));
    }

    [Test]
    public async Task GetRankedTradingPairsAsync_ReturnsOnlyEnabledPairs()
    {
        // Act
        var rankedPairs = await _analyzer.GetRankedTradingPairsAsync();

        // Assert
        Assert.That(rankedPairs, Has.Count.EqualTo(2));
        Assert.That(rankedPairs.All(p => p.Enabled), Is.True);
    }

    [Test]
    public async Task GetRankedTradingPairsAsync_ReturnsPairsSortedByRank()
    {
        // Act
        var rankedPairs = await _analyzer.GetRankedTradingPairsAsync();

        // Assert
        Assert.That(rankedPairs[0].ProfitabilityRank, Is.EqualTo(1));
        Assert.That(rankedPairs[1].ProfitabilityRank, Is.EqualTo(2));
        Assert.That(rankedPairs[0].Id, Is.EqualTo("usdc-sol"));
        Assert.That(rankedPairs[1].Id, Is.EqualTo("usdt-sol"));
    }

    [Test]
    public async Task GetRankedTradingPairsAsync_WhenAllDisabled_ReturnsEmpty()
    {
        // Arrange
        foreach (var pair in _config.TradingPairs)
        {
            pair.Enabled = false;
        }

        // Act
        var rankedPairs = await _analyzer.GetRankedTradingPairsAsync();

        // Assert
        Assert.That(rankedPairs, Is.Empty);
    }

    [Test]
    public async Task UpdateProfitabilityScoresAsync_UpdatesAllEnabledPairs()
    {
        // Arrange
        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDC_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "1000000000",
            PriceImpactPct = 0.5m
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        // Act
        await _analyzer.UpdateProfitabilityScoresAsync();

        // Assert
        var enabledPairs = _config.TradingPairs.Where(p => p.Enabled).ToList();
        foreach (var pair in enabledPairs)
        {
            Assert.That(pair.CurrentProfitabilityScore, Is.GreaterThan(0m));
            Assert.That(pair.LastUpdated, Is.GreaterThan(DateTime.UtcNow.AddMinutes(-1)));
        }
    }

    [Test]
    public async Task UpdateProfitabilityScoresAsync_DoesNotUpdateDisabledPairs()
    {
        // Arrange
        var disabledPair = _config.TradingPairs.First(p => !p.Enabled);
        var originalScore = disabledPair.CurrentProfitabilityScore;
        var originalTimestamp = disabledPair.LastUpdated;

        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDC_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "1000000000",
            PriceImpactPct = 0.5m
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        // Act
        await _analyzer.UpdateProfitabilityScoresAsync();

        // Assert
        Assert.That(disabledPair.CurrentProfitabilityScore, Is.EqualTo(originalScore));
        Assert.That(disabledPair.LastUpdated, Is.EqualTo(originalTimestamp));
    }

    [Test]
    public async Task UpdateProfitabilityScoresAsync_HandlesPartialFailures()
    {
        // Arrange
        var callCount = 0;
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new Exception("API error for first pair");
                }
                return new JupiterQuoteResponse
                {
                    InputMint = "USDT_MINT",
                    OutputMint = "SOL_MINT",
                    InAmount = "1000000000",
                    OutAmount = "1000000000",
                    PriceImpactPct = 0.5m
                };
            });

        // Act
        await _analyzer.UpdateProfitabilityScoresAsync();

        // Assert
        var firstPair = _config.TradingPairs.First(p => p.Id == "usdc-sol");
        var secondPair = _config.TradingPairs.First(p => p.Id == "usdt-sol");

        Assert.That(firstPair.CurrentProfitabilityScore, Is.EqualTo(0m)); // Failed
        Assert.That(secondPair.CurrentProfitabilityScore, Is.GreaterThan(0m)); // Succeeded
    }
}
