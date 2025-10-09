using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using The16Oracles.www.Server.Models;
using The16Oracles.www.Server.Services;

namespace The16Oracles.www.Server.nunit.Services;

[TestFixture]
public class TradingBotOrchestratorTests
{
    private Mock<IJupiterApiService> _mockJupiterApi;
    private Mock<ISolanaTransactionService> _mockSolanaService;
    private Mock<IRiskManagementService> _mockRiskService;
    private Mock<IProfitabilityAnalyzer> _mockProfitabilityAnalyzer;
    private Mock<ILogger<TradingBotOrchestrator>> _mockLogger;
    private TradeBotConfiguration _config;
    private TradingBotOrchestrator _orchestrator;

    [SetUp]
    public void Setup()
    {
        _mockJupiterApi = new Mock<IJupiterApiService>();
        _mockSolanaService = new Mock<ISolanaTransactionService>();
        _mockRiskService = new Mock<IRiskManagementService>();
        _mockProfitabilityAnalyzer = new Mock<IProfitabilityAnalyzer>();
        _mockLogger = new Mock<ILogger<TradingBotOrchestrator>>();

        _config = new TradeBotConfiguration
        {
            Solana = new SolanaConfiguration
            {
                RpcUrl = "https://api.mainnet-beta.solana.com",
                SolMint = "So11111111111111111111111111111111111111112"
            },
            TradingPairs = new List<TradingPairConfiguration>
            {
                new TradingPairConfiguration
                {
                    Id = "usdc-sol",
                    StableCoinMint = "USDC_MINT",
                    TargetTokenMint = "SOL_MINT",
                    ProfitabilityRank = 1,
                    Enabled = true,
                    RiskManagement = new RiskManagementConfiguration
                    {
                        MaxTradeNotionalSol = 10m,
                        SlippageBps = 30,
                        MinBalanceSol = 0.1m
                    }
                },
                new TradingPairConfiguration
                {
                    Id = "usdt-sol",
                    StableCoinMint = "USDT_MINT",
                    TargetTokenMint = "SOL_MINT",
                    ProfitabilityRank = 2,
                    Enabled = true,
                    RiskManagement = new RiskManagementConfiguration
                    {
                        MaxTradeNotionalSol = 10m,
                        SlippageBps = 30,
                        MinBalanceSol = 0.1m
                    }
                }
            }
        };

        var options = Options.Create(_config);
        _orchestrator = new TradingBotOrchestrator(
            _mockJupiterApi.Object,
            _mockSolanaService.Object,
            _mockRiskService.Object,
            _mockProfitabilityAnalyzer.Object,
            options,
            _mockLogger.Object);
    }

    [Test]
    public async Task GetAllPairStatusesAsync_ReturnsAllPairs()
    {
        // Arrange
        _mockSolanaService.Setup(x => x.GetTokenBalanceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100m);
        _mockRiskService.Setup(x => x.GetDailyVolumeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50m);

        // Act
        var result = await _orchestrator.GetAllPairStatusesAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Id, Is.EqualTo("usdc-sol"));
        Assert.That(result[1].Id, Is.EqualTo("usdt-sol"));
    }

    [Test]
    public async Task GetPairStatusAsync_WithValidId_ReturnsPairStatus()
    {
        // Arrange
        var pairId = "usdc-sol";
        _mockSolanaService.Setup(x => x.GetTokenBalanceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100m);
        _mockRiskService.Setup(x => x.GetDailyVolumeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50m);

        // Act
        var result = await _orchestrator.GetPairStatusAsync(pairId);

        // Assert
        Assert.That(result.Id, Is.EqualTo(pairId));
        Assert.That(result.Enabled, Is.True);
        Assert.That(result.ProfitabilityRank, Is.EqualTo(1));
    }

    [Test]
    public async Task GetPairStatusAsync_WithInvalidId_ReturnsEmptyStatus()
    {
        // Arrange
        var pairId = "nonexistent";

        // Act
        var result = await _orchestrator.GetPairStatusAsync(pairId);

        // Assert
        Assert.That(result.Id, Is.EqualTo(pairId));
        Assert.That(result.Enabled, Is.False);
    }

    [Test]
    public async Task AddTradingPairAsync_WithNewPair_ReturnsTrue()
    {
        // Arrange
        var request = new AddTradingPairRequest
        {
            Id = "dai-sol",
            StableCoinMint = "DAI_MINT",
            TargetTokenMint = "SOL_MINT",
            ProfitabilityRank = 3
        };

        // Act
        var result = await _orchestrator.AddTradingPairAsync(request);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_config.TradingPairs, Has.Count.EqualTo(3));
        Assert.That(_config.TradingPairs.Any(p => p.Id == "dai-sol"), Is.True);
    }

    [Test]
    public async Task AddTradingPairAsync_WithExistingPair_ReturnsFalse()
    {
        // Arrange
        var request = new AddTradingPairRequest
        {
            Id = "usdc-sol",
            StableCoinMint = "USDC_MINT",
            TargetTokenMint = "SOL_MINT",
            ProfitabilityRank = 1
        };

        // Act
        var result = await _orchestrator.AddTradingPairAsync(request);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_config.TradingPairs, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UpdateProfitabilityRankAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var pairId = "usdc-sol";
        var newRank = 5;

        // Act
        var result = await _orchestrator.UpdateProfitabilityRankAsync(pairId, newRank);

        // Assert
        Assert.That(result, Is.True);
        var pair = _config.TradingPairs.First(p => p.Id == pairId);
        Assert.That(pair.ProfitabilityRank, Is.EqualTo(newRank));
    }

    [Test]
    public async Task UpdateProfitabilityRankAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var pairId = "nonexistent";
        var newRank = 5;

        // Act
        var result = await _orchestrator.UpdateProfitabilityRankAsync(pairId, newRank);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task EnableDisablePairAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var pairId = "usdc-sol";
        var enabled = false;

        // Act
        var result = await _orchestrator.EnableDisablePairAsync(pairId, enabled);

        // Assert
        Assert.That(result, Is.True);
        var pair = _config.TradingPairs.First(p => p.Id == pairId);
        Assert.That(pair.Enabled, Is.False);
    }

    [Test]
    public async Task EnableDisablePairAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var pairId = "nonexistent";
        var enabled = true;

        // Act
        var result = await _orchestrator.EnableDisablePairAsync(pairId, enabled);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExecuteCascadeAsync_WithNoPairs_ReturnsFailure()
    {
        // Arrange
        _config.TradingPairs.Clear();
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 1.0m,
            MaxCascadeDepth = 3
        };
        _mockProfitabilityAnalyzer.Setup(x => x.GetRankedTradingPairsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TradingPairConfiguration>());

        // Act
        var result = await _orchestrator.ExecuteCascadeAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("No enabled trading pairs"));
    }

    [Test]
    public async Task ExecuteCascadeAsync_WithSuccessfulTrade_ReturnsSuccess()
    {
        // Arrange
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 1.0m,
            MaxCascadeDepth = 1,
            StopOnFailure = true
        };

        var rankedPairs = new List<TradingPairConfiguration> { _config.TradingPairs[0] };
        _mockProfitabilityAnalyzer.Setup(x => x.GetRankedTradingPairsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rankedPairs);

        _mockSolanaService.Setup(x => x.VerifyMinimumBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockSolanaService.Setup(x => x.GetWalletPublicKey())
            .Returns("WalletPublicKey");
        _mockSolanaService.Setup(x => x.ExecuteSwapTransactionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("signature123");

        _mockRiskService.Setup(x => x.CheckTradeRiskAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RiskCheckResult { Passed = true });
        _mockRiskService.Setup(x => x.RecordTradeAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDC_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "1050000000",
            PriceImpactPct = 0.1m
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var swapResponse = new JupiterSwapResponse
        {
            SwapTransaction = "base64Transaction",
            LastValidBlockHeight = "12345"
        };
        _mockJupiterApi.Setup(x => x.GetSwapTransactionAsync(It.IsAny<JupiterSwapRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(swapResponse);

        // Act
        var result = await _orchestrator.ExecuteCascadeAsync(request);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Steps, Has.Count.EqualTo(1));
        Assert.That(result.Steps[0].Success, Is.True);
        Assert.That(result.Steps[0].TradingPairId, Is.EqualTo("usdc-sol"));
    }

    [Test]
    public async Task ExecuteCascadeAsync_WithFailedRiskCheck_StopsOnFailure()
    {
        // Arrange
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 1.0m,
            MaxCascadeDepth = 2,
            StopOnFailure = true
        };

        var rankedPairs = _config.TradingPairs.ToList();
        _mockProfitabilityAnalyzer.Setup(x => x.GetRankedTradingPairsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(rankedPairs);

        _mockSolanaService.Setup(x => x.VerifyMinimumBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockRiskService.Setup(x => x.CheckTradeRiskAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RiskCheckResult
            {
                Passed = false,
                Violations = new List<string> { "Exceeds daily limit" }
            });

        // Act
        var result = await _orchestrator.ExecuteCascadeAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Steps, Has.Count.EqualTo(1));
        Assert.That(result.Steps[0].Success, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Cascade stopped"));
    }

    [Test]
    public async Task ExecuteCascadeAsync_WithSpecificPairs_ExecutesOnlyThosePairs()
    {
        // Arrange
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 1.0m,
            MaxCascadeDepth = 3,
            SpecificPairIds = new List<string> { "usdt-sol" }
        };

        var rankedPairs = _config.TradingPairs.Where(p => p.Id == "usdt-sol").ToList();
        _mockProfitabilityAnalyzer.Setup(x => x.GetRankedTradingPairsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_config.TradingPairs.ToList());

        _mockSolanaService.Setup(x => x.VerifyMinimumBalanceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockSolanaService.Setup(x => x.GetWalletPublicKey())
            .Returns("WalletPublicKey");
        _mockSolanaService.Setup(x => x.ExecuteSwapTransactionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("signature123");

        _mockRiskService.Setup(x => x.CheckTradeRiskAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RiskCheckResult { Passed = true });
        _mockRiskService.Setup(x => x.RecordTradeAsync(It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var quote = new JupiterQuoteResponse
        {
            InputMint = "USDT_MINT",
            OutputMint = "SOL_MINT",
            InAmount = "1000000000",
            OutAmount = "1050000000",
            PriceImpactPct = 0.1m
        };
        _mockJupiterApi.Setup(x => x.GetQuoteAsync(It.IsAny<JupiterQuoteRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        var swapResponse = new JupiterSwapResponse
        {
            SwapTransaction = "base64Transaction",
            LastValidBlockHeight = "12345"
        };
        _mockJupiterApi.Setup(x => x.GetSwapTransactionAsync(It.IsAny<JupiterSwapRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(swapResponse);

        // Act
        var result = await _orchestrator.ExecuteCascadeAsync(request);

        // Assert
        Assert.That(result.Steps.All(s => s.TradingPairId == "usdt-sol"), Is.True);
    }
}
