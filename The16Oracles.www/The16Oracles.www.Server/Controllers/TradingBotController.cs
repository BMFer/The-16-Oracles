using Microsoft.AspNetCore.Mvc;
using The16Oracles.www.Server.Models;
using The16Oracles.www.Server.Services;

namespace The16Oracles.www.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingBotController : ControllerBase
{
    private readonly ITradingBotService _tradingBotService;
    private readonly ILogger<TradingBotController> _logger;

    public TradingBotController(
        ITradingBotService tradingBotService,
        ILogger<TradingBotController> logger)
    {
        _tradingBotService = tradingBotService;
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
}
