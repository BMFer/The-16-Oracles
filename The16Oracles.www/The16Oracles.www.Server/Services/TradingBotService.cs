using Microsoft.Extensions.Options;
using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public class TradingBotService : ITradingBotService
{
    private readonly IJupiterApiService _jupiterApi;
    private readonly ISolanaTransactionService _solanaService;
    private readonly IRiskManagementService _riskService;
    private readonly TradeBotConfiguration _config;
    private readonly ILogger<TradingBotService> _logger;

    private DateTime? _lastTradeTime;
    private readonly object _tradeLock = new();

    public TradingBotService(
        IJupiterApiService jupiterApi,
        ISolanaTransactionService solanaService,
        IRiskManagementService riskService,
        IOptions<TradeBotConfiguration> config,
        ILogger<TradingBotService> logger)
    {
        _jupiterApi = jupiterApi;
        _solanaService = solanaService;
        _riskService = riskService;
        _config = config.Value;
        _logger = logger;
    }

    public bool IsEnabled => _config.Trading.Enabled;

    public async Task<TradeExecutionResponse> ExecuteTradeAsync(
        TradeExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        lock (_tradeLock)
        {
            if (!IsEnabled)
            {
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = "Trading bot is disabled. Enable it in configuration."
                };
            }
        }

        try
        {
            _logger.LogInformation(
                "Starting trade execution: {Direction}, Amount: {Amount} SOL",
                request.Direction,
                request.AmountSol);

            // Step 1: Verify minimum balance
            if (!await _solanaService.VerifyMinimumBalanceAsync(cancellationToken))
            {
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Insufficient SOL balance. Minimum required: {_config.RiskManagement.MinBalanceSol} SOL"
                };
            }

            // Step 2: Risk checks
            var riskCheck = await _riskService.CheckTradeRiskAsync(request.AmountSol, cancellationToken);
            if (!riskCheck.Passed)
            {
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Risk check failed: {string.Join(", ", riskCheck.Violations)}"
                };
            }

            // Step 3: Prepare trade parameters
            var (inputMint, outputMint) = request.Direction == TradeDirection.SolToToken
                ? (_config.Solana.SolMint, _config.Solana.TokenMint)
                : (_config.Solana.TokenMint, _config.Solana.SolMint);

            var amountLamports = (long)(request.AmountSol * 1_000_000_000);

            // Step 4: Get quote from Jupiter
            var quoteRequest = new JupiterQuoteRequest
            {
                InputMint = inputMint,
                OutputMint = outputMint,
                Amount = amountLamports,
                SlippageBps = _config.RiskManagement.SlippageBps
            };

            var quote = await _jupiterApi.GetQuoteAsync(quoteRequest, cancellationToken);

            // Step 5: Validate quote
            if (quote.PriceImpactPct > 1.0m)
            {
                _logger.LogWarning("High price impact detected: {PriceImpact}%", quote.PriceImpactPct);
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Price impact too high: {quote.PriceImpactPct}%. Trade rejected for safety."
                };
            }

            // Step 6: Get swap transaction
            var swapRequest = new JupiterSwapRequest
            {
                QuoteResponse = quote,
                UserPublicKey = _solanaService.GetWalletPublicKey(),
                WrapAndUnwrapSol = true
            };

            var swapResponse = await _jupiterApi.GetSwapTransactionAsync(swapRequest, cancellationToken);

            // Step 7: Execute transaction
            var signature = await _solanaService.ExecuteSwapTransactionAsync(
                swapResponse.SwapTransaction,
                cancellationToken);

            // Step 8: Record trade
            await _riskService.RecordTradeAsync(request.AmountSol, cancellationToken);

            lock (_tradeLock)
            {
                _lastTradeTime = DateTime.UtcNow;
            }

            // Step 9: Return success
            var outputAmount = decimal.Parse(quote.OutAmount) / 1_000_000_000m;

            _logger.LogInformation(
                "Trade executed successfully. Signature: {Signature}, Output: {Output}",
                signature,
                outputAmount);

            return new TradeExecutionResponse
            {
                Success = true,
                TransactionSignature = signature,
                Details = new TradeDetails
                {
                    InputMint = inputMint,
                    OutputMint = outputMint,
                    InputAmount = request.AmountSol,
                    OutputAmount = outputAmount,
                    PriceImpactPct = quote.PriceImpactPct,
                    ExecutedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing trade");
            return new TradeExecutionResponse
            {
                Success = false,
                ErrorMessage = $"Trade execution failed: {ex.Message}"
            };
        }
    }

    public async Task<BotStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var solBalance = await _solanaService.GetSolBalanceAsync(cancellationToken);
            var tokenBalance = await _solanaService.GetTokenBalanceAsync(
                _config.Solana.TokenMint,
                cancellationToken);
            var dailyVolume = await _riskService.GetDailyVolumeAsync(cancellationToken);

            return new BotStatusResponse
            {
                IsRunning = true,
                IsEnabled = IsEnabled,
                CurrentSolBalance = solBalance,
                CurrentTokenBalance = tokenBalance,
                DailyVolumeSol = dailyVolume,
                TradesExecutedToday = 0, // Could be tracked in risk service
                LastTradeAt = _lastTradeTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bot status");
            return new BotStatusResponse
            {
                IsRunning = false,
                IsEnabled = IsEnabled
            };
        }
    }
}
