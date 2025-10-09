using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using The16Oracles.www.Server.Controllers;
using The16Oracles.www.Server.Models;
using The16Oracles.www.Server.Services;

namespace The16Oracles.www.Server.nunit.Controllers;

[TestFixture]
public class TradingBotControllerTests
{
    private Mock<ITradingBotService> _mockTradingBotService;
    private Mock<ITradingBotOrchestrator> _mockOrchestrator;
    private Mock<ILogger<TradingBotController>> _mockLogger;
    private TradingBotController _controller;

    [SetUp]
    public void Setup()
    {
        _mockTradingBotService = new Mock<ITradingBotService>();
        _mockOrchestrator = new Mock<ITradingBotOrchestrator>();
        _mockLogger = new Mock<ILogger<TradingBotController>>();
        _controller = new TradingBotController(
            _mockTradingBotService.Object,
            _mockOrchestrator.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task GetStatus_ReturnsOkResult_WithBotStatus()
    {
        // Arrange
        var expectedStatus = new BotStatusResponse
        {
            IsRunning = true,
            IsEnabled = true,
            CurrentSolBalance = 10.5m,
            CurrentTokenBalance = 1000000m,
            DailyVolumeSol = 150.0m,
            TradesExecutedToday = 5
        };
        _mockTradingBotService.Setup(x => x.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetStatus(CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(expectedStatus));
    }

    [Test]
    public async Task ExecuteTrade_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new TradeExecutionRequest
        {
            Direction = TradeDirection.SolToToken,
            AmountSol = 1.0m
        };
        var expectedResponse = new TradeExecutionResponse
        {
            Success = true,
            TransactionSignature = "3xH...abc",
            Details = new TradeDetails
            {
                InputMint = "So11111111111111111111111111111111111111112",
                OutputMint = "TokenMint",
                InputAmount = 1.0m,
                OutputAmount = 125000m,
                PriceImpactPct = 0.15m,
                ExecutedAt = DateTime.UtcNow
            }
        };
        _mockTradingBotService.Setup(x => x.IsEnabled).Returns(true);
        _mockTradingBotService.Setup(x => x.ExecuteTradeAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExecuteTrade(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(expectedResponse));
    }

    [Test]
    public async Task ExecuteTrade_WithInvalidAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new TradeExecutionRequest
        {
            Direction = TradeDirection.SolToToken,
            AmountSol = 0m
        };

        // Act
        var result = await _controller.ExecuteTrade(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ExecuteTrade_WhenBotDisabled_ReturnsBadRequest()
    {
        // Arrange
        var request = new TradeExecutionRequest
        {
            Direction = TradeDirection.SolToToken,
            AmountSol = 1.0m
        };
        _mockTradingBotService.Setup(x => x.IsEnabled).Returns(false);

        // Act
        var result = await _controller.ExecuteTrade(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetAllPairs_ReturnsOkResult_WithAllPairs()
    {
        // Arrange
        var expectedPairs = new List<TradingPairStatusResponse>
        {
            new TradingPairStatusResponse
            {
                Id = "usdc-sol",
                Enabled = true,
                ProfitabilityRank = 1,
                CurrentProfitabilityScore = 95.5m
            },
            new TradingPairStatusResponse
            {
                Id = "usdt-sol",
                Enabled = true,
                ProfitabilityRank = 2,
                CurrentProfitabilityScore = 92.3m
            }
        };
        _mockOrchestrator.Setup(x => x.GetAllPairStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPairs);

        // Act
        var result = await _controller.GetAllPairs(CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var pairs = okResult?.Value as List<TradingPairStatusResponse>;
        Assert.That(pairs, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetPairStatus_ReturnsOkResult_WithPairStatus()
    {
        // Arrange
        var pairId = "usdc-sol";
        var expectedStatus = new TradingPairStatusResponse
        {
            Id = pairId,
            Enabled = true,
            ProfitabilityRank = 1,
            CurrentProfitabilityScore = 95.5m
        };
        _mockOrchestrator.Setup(x => x.GetPairStatusAsync(pairId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _controller.GetPairStatus(pairId, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(expectedStatus));
    }

    [Test]
    public async Task AddTradingPair_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new AddTradingPairRequest
        {
            Id = "dai-sol",
            StableCoinMint = "DAI_MINT",
            TargetTokenMint = "SOL_MINT",
            ProfitabilityRank = 3
        };
        _mockOrchestrator.Setup(x => x.AddTradingPairAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AddTradingPair(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task AddTradingPair_WhenPairExists_ReturnsBadRequest()
    {
        // Arrange
        var request = new AddTradingPairRequest
        {
            Id = "usdc-sol",
            StableCoinMint = "USDC_MINT",
            TargetTokenMint = "SOL_MINT",
            ProfitabilityRank = 1
        };
        _mockOrchestrator.Setup(x => x.AddTradingPairAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AddTradingPair(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateProfitabilityRank_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var pairId = "usdc-sol";
        var request = new UpdateProfitabilityRankRequest { NewRank = 2 };
        _mockOrchestrator.Setup(x => x.UpdateProfitabilityRankAsync(pairId, request.NewRank, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateProfitabilityRank(pairId, request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateProfitabilityRank_WhenPairNotFound_ReturnsNotFound()
    {
        // Arrange
        var pairId = "nonexistent";
        var request = new UpdateProfitabilityRankRequest { NewRank = 2 };
        _mockOrchestrator.Setup(x => x.UpdateProfitabilityRankAsync(pairId, request.NewRank, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateProfitabilityRank(pairId, request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task EnableDisablePair_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var pairId = "usdc-sol";
        var enabled = false;
        _mockOrchestrator.Setup(x => x.EnableDisablePairAsync(pairId, enabled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.EnableDisablePair(pairId, enabled, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task EnableDisablePair_WhenPairNotFound_ReturnsNotFound()
    {
        // Arrange
        var pairId = "nonexistent";
        var enabled = true;
        _mockOrchestrator.Setup(x => x.EnableDisablePairAsync(pairId, enabled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.EnableDisablePair(pairId, enabled, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task ExecuteCascade_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 1.0m,
            MaxCascadeDepth = 3,
            StopOnFailure = true
        };
        var expectedResponse = new CascadeExecutionResponse
        {
            Success = true,
            FinalAmountSol = 1.05m,
            TotalProfitSol = 0.05m,
            Steps = new List<CascadeStepResult>
            {
                new CascadeStepResult
                {
                    StepNumber = 1,
                    TradingPairId = "usdc-sol",
                    Success = true,
                    TransactionSignature = "sig1"
                }
            }
        };
        _mockTradingBotService.Setup(x => x.IsEnabled).Returns(true);
        _mockOrchestrator.Setup(x => x.ExecuteCascadeAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ExecuteCascade(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as CascadeExecutionResponse;
        Assert.That(response?.Success, Is.True);
        Assert.That(response?.TotalProfitSol, Is.EqualTo(0.05m));
    }

    [Test]
    public async Task ExecuteCascade_WithInvalidAmount_ReturnsBadRequest()
    {
        // Arrange
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 0m,
            MaxCascadeDepth = 3
        };

        // Act
        var result = await _controller.ExecuteCascade(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task ExecuteCascade_WhenBotDisabled_ReturnsBadRequest()
    {
        // Arrange
        var request = new CascadeExecutionRequest
        {
            InitialAmountSol = 1.0m,
            MaxCascadeDepth = 3
        };
        _mockTradingBotService.Setup(x => x.IsEnabled).Returns(false);

        // Act
        var result = await _controller.ExecuteCascade(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }
}
