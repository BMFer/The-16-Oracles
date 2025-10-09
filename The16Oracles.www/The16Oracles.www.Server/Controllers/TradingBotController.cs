using Microsoft.AspNetCore.Mvc;
using The16Oracles.www.Server.Models;
using The16Oracles.www.Server.Services;

namespace The16Oracles.www.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingBotController : ControllerBase
{
    private readonly ITradingBotService _tradingBotService;
    private readonly ITradingBotOrchestrator _orchestrator;
    private readonly ILogger<TradingBotController> _logger;

    public TradingBotController(
        ITradingBotService tradingBotService,
        ITradingBotOrchestrator orchestrator,
        ILogger<TradingBotController> logger)
    {
        _tradingBotService = tradingBotService;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Get the current status of the trading bot
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(BotStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BotStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        try
        {
            var status = await _tradingBotService.GetStatusAsync(cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bot status");
            return StatusCode(500, new { error = "Failed to get bot status" });
        }
    }

    /// <summary>
    /// Execute a trade (SOL to Token or Token to SOL)
    /// </summary>
    [HttpPost("trade")]
    [ProducesResponseType(typeof(TradeExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TradeExecutionResponse>> ExecuteTrade(
        [FromBody] TradeExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.AmountSol <= 0)
            {
                return BadRequest(new { error = "Amount must be greater than 0" });
            }

            if (!_tradingBotService.IsEnabled)
            {
                return BadRequest(new { error = "Trading bot is disabled" });
            }

            var result = await _tradingBotService.ExecuteTradeAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing trade");
            return StatusCode(500, new TradeExecutionResponse
            {
                Success = false,
                ErrorMessage = $"Internal error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Execute a SOL to Token swap
    /// </summary>
    [HttpPost("swap/sol-to-token")]
    [ProducesResponseType(typeof(TradeExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TradeExecutionResponse>> SwapSolToToken(
        [FromBody] decimal amountSol,
        CancellationToken cancellationToken)
    {
        var request = new TradeExecutionRequest
        {
            Direction = TradeDirection.SolToToken,
            AmountSol = amountSol
        };

        return await ExecuteTrade(request, cancellationToken);
    }

    /// <summary>
    /// Execute a Token to SOL swap
    /// </summary>
    [HttpPost("swap/token-to-sol")]
    [ProducesResponseType(typeof(TradeExecutionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TradeExecutionResponse>> SwapTokenToSol(
        [FromBody] decimal amountSol,
        CancellationToken cancellationToken)
    {
        var request = new TradeExecutionRequest
        {
            Direction = TradeDirection.TokenToSol,
            AmountSol = amountSol
        };

        return await ExecuteTrade(request, cancellationToken);
    }

    /// <summary>
    /// Get all trading pairs with their statuses
    /// </summary>
    [HttpGet("pairs")]
    [ProducesResponseType(typeof(List<TradingPairStatusResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TradingPairStatusResponse>>> GetAllPairs(
        CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _orchestrator.GetAllPairStatusesAsync(cancellationToken);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all trading pairs");
            return StatusCode(500, new { error = "Failed to get trading pairs" });
        }
    }

    /// <summary>
    /// Get status of a specific trading pair
    /// </summary>
    [HttpGet("pairs/{pairId}")]
    [ProducesResponseType(typeof(TradingPairStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TradingPairStatusResponse>> GetPairStatus(
        string pairId,
        CancellationToken cancellationToken)
    {
        try
        {
            var status = await _orchestrator.GetPairStatusAsync(pairId, cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for pair {PairId}", pairId);
            return StatusCode(500, new { error = "Failed to get pair status" });
        }
    }

    /// <summary>
    /// Add a new trading pair
    /// </summary>
    [HttpPost("pairs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddTradingPair(
        [FromBody] AddTradingPairRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _orchestrator.AddTradingPairAsync(request, cancellationToken);
            if (!success)
            {
                return BadRequest(new { error = "Trading pair already exists or invalid request" });
            }

            return Ok(new { message = "Trading pair added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding trading pair");
            return StatusCode(500, new { error = "Failed to add trading pair" });
        }
    }

    /// <summary>
    /// Update profitability rank of a trading pair
    /// </summary>
    [HttpPut("pairs/{pairId}/ranking")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateProfitabilityRank(
        string pairId,
        [FromBody] UpdateProfitabilityRankRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _orchestrator.UpdateProfitabilityRankAsync(
                pairId,
                request.NewRank,
                cancellationToken);

            if (!success)
            {
                return NotFound(new { error = "Trading pair not found" });
            }

            return Ok(new { message = "Profitability rank updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profitability rank for pair {PairId}", pairId);
            return StatusCode(500, new { error = "Failed to update profitability rank" });
        }
    }

    /// <summary>
    /// Enable or disable a trading pair
    /// </summary>
    [HttpPut("pairs/{pairId}/enabled")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> EnableDisablePair(
        string pairId,
        [FromBody] bool enabled,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await _orchestrator.EnableDisablePairAsync(pairId, enabled, cancellationToken);
            if (!success)
            {
                return NotFound(new { error = "Trading pair not found" });
            }

            return Ok(new { message = $"Trading pair {(enabled ? "enabled" : "disabled")} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling/disabling pair {PairId}", pairId);
            return StatusCode(500, new { error = "Failed to enable/disable trading pair" });
        }
    }

    /// <summary>
    /// Execute cascade trading strategy across multiple pairs
    /// </summary>
    [HttpPost("execute-cascade")]
    [ProducesResponseType(typeof(CascadeExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CascadeExecutionResponse>> ExecuteCascade(
        [FromBody] CascadeExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.InitialAmountSol <= 0)
            {
                return BadRequest(new { error = "Initial amount must be greater than 0" });
            }

            if (!_tradingBotService.IsEnabled)
            {
                return BadRequest(new { error = "Trading bot is disabled" });
            }

            var result = await _orchestrator.ExecuteCascadeAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing cascade");
            return StatusCode(500, new CascadeExecutionResponse
            {
                Success = false,
                ErrorMessage = $"Internal error: {ex.Message}"
            });
        }
    }
}
