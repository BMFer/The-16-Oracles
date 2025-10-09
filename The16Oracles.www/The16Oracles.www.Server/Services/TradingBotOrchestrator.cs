using Microsoft.Extensions.Options;
using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public class TradingBotOrchestrator : ITradingBotOrchestrator
{
    private readonly IJupiterApiService _jupiterApi;
    private readonly ISolanaTransactionService _solanaService;
    private readonly IRiskManagementService _riskService;
    private readonly IProfitabilityAnalyzer _profitabilityAnalyzer;
    private readonly TradeBotConfiguration _config;
    private readonly ILogger<TradingBotOrchestrator> _logger;
    private readonly object _configLock = new();

    public TradingBotOrchestrator(
        IJupiterApiService jupiterApi,
        ISolanaTransactionService solanaService,
        IRiskManagementService riskService,
        IProfitabilityAnalyzer profitabilityAnalyzer,
        IOptions<TradeBotConfiguration> config,
        ILogger<TradingBotOrchestrator> logger)
    {
        _jupiterApi = jupiterApi;
        _solanaService = solanaService;
        _riskService = riskService;
        _profitabilityAnalyzer = profitabilityAnalyzer;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<CascadeExecutionResponse> ExecuteCascadeAsync(
        CascadeExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new CascadeExecutionResponse
        {
            Success = true
        };

        try
        {
            _logger.LogInformation(
                "Starting cascade execution with initial amount: {Amount} SOL",
                request.InitialAmountSol);

            // Get ranked trading pairs
            var rankedPairs = await _profitabilityAnalyzer.GetRankedTradingPairsAsync(cancellationToken);

            // Filter to specific pairs if requested
            if (request.SpecificPairIds?.Any() == true)
            {
                rankedPairs = rankedPairs
                    .Where(p => request.SpecificPairIds.Contains(p.Id))
                    .ToList();
            }

            // Limit cascade depth
            rankedPairs = rankedPairs.Take(request.MaxCascadeDepth).ToList();

            if (!rankedPairs.Any())
            {
                response.Success = false;
                response.ErrorMessage = "No enabled trading pairs available for cascade";
                return response;
            }

            decimal currentAmount = request.InitialAmountSol;
            int stepNumber = 1;

            foreach (var pair in rankedPairs)
            {
                var stepResult = new CascadeStepResult
                {
                    StepNumber = stepNumber,
                    TradingPairId = pair.Id
                };

                try
                {
                    _logger.LogInformation(
                        "Executing cascade step {Step}: {PairId} with {Amount} SOL",
                        stepNumber,
                        pair.Id,
                        currentAmount);

                    // Execute trade for this pair
                    var tradeResult = await ExecutePairTradeAsync(
                        pair,
                        currentAmount,
                        cancellationToken);

                    stepResult.Success = tradeResult.Success;
                    stepResult.TransactionSignature = tradeResult.TransactionSignature;
                    stepResult.Details = tradeResult.Details;
                    stepResult.ErrorMessage = tradeResult.ErrorMessage;

                    response.Steps.Add(stepResult);

                    if (!tradeResult.Success)
                    {
                        _logger.LogWarning(
                            "Cascade step {Step} failed: {Error}",
                            stepNumber,
                            tradeResult.ErrorMessage);

                        if (request.StopOnFailure)
                        {
                            response.Success = false;
                            response.ErrorMessage = $"Cascade stopped at step {stepNumber}: {tradeResult.ErrorMessage}";
                            break;
                        }
                    }
                    else
                    {
                        // Update current amount for next iteration
                        // In a real cascade, you'd convert the output back to SOL equivalent
                        // For now, we'll use the output amount if it's SOL, otherwise keep current
                        if (tradeResult.Details?.OutputMint == _config.Solana.SolMint)
                        {
                            currentAmount = tradeResult.Details.OutputAmount;
                        }

                        _logger.LogInformation(
                            "Cascade step {Step} completed. Signature: {Signature}",
                            stepNumber,
                            tradeResult.TransactionSignature);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in cascade step {Step}", stepNumber);
                    stepResult.Success = false;
                    stepResult.ErrorMessage = ex.Message;
                    response.Steps.Add(stepResult);

                    if (request.StopOnFailure)
                    {
                        response.Success = false;
                        response.ErrorMessage = $"Cascade stopped at step {stepNumber}: {ex.Message}";
                        break;
                    }
                }

                stepNumber++;
            }

            // Calculate final profit
            response.FinalAmountSol = currentAmount;
            response.TotalProfitSol = currentAmount - request.InitialAmountSol;

            _logger.LogInformation(
                "Cascade execution completed. Initial: {Initial} SOL, Final: {Final} SOL, Profit: {Profit} SOL",
                request.InitialAmountSol,
                response.FinalAmountSol,
                response.TotalProfitSol);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing cascade");
            response.Success = false;
            response.ErrorMessage = $"Cascade execution failed: {ex.Message}";
            return response;
        }
    }

    private async Task<TradeExecutionResponse> ExecutePairTradeAsync(
        TradingPairConfiguration pair,
        decimal amountSol,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify minimum balance
            if (!await _solanaService.VerifyMinimumBalanceAsync(cancellationToken))
            {
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Insufficient SOL balance. Minimum required: {pair.RiskManagement.MinBalanceSol} SOL"
                };
            }

            // Risk checks
            var riskCheck = await _riskService.CheckTradeRiskAsync(amountSol, cancellationToken);
            if (!riskCheck.Passed)
            {
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Risk check failed: {string.Join(", ", riskCheck.Violations)}"
                };
            }

            // Prepare trade (stablecoin -> target token)
            var amountLamports = (long)(amountSol * 1_000_000_000);

            // Get quote from Jupiter
            var quoteRequest = new JupiterQuoteRequest
            {
                InputMint = pair.StableCoinMint,
                OutputMint = pair.TargetTokenMint,
                Amount = amountLamports,
                SlippageBps = pair.RiskManagement.SlippageBps
            };

            var quote = await _jupiterApi.GetQuoteAsync(quoteRequest, cancellationToken);

            // Validate quote
            if (quote.PriceImpactPct > 1.0m)
            {
                _logger.LogWarning(
                    "High price impact detected for pair {PairId}: {PriceImpact}%",
                    pair.Id,
                    quote.PriceImpactPct);
                return new TradeExecutionResponse
                {
                    Success = false,
                    ErrorMessage = $"Price impact too high: {quote.PriceImpactPct}%. Trade rejected."
                };
            }

            // Get swap transaction
            var swapRequest = new JupiterSwapRequest
            {
                QuoteResponse = quote,
                UserPublicKey = _solanaService.GetWalletPublicKey(),
                WrapAndUnwrapSol = true
            };

            var swapResponse = await _jupiterApi.GetSwapTransactionAsync(swapRequest, cancellationToken);

            // Execute transaction
            var signature = await _solanaService.ExecuteSwapTransactionAsync(
                swapResponse.SwapTransaction,
                cancellationToken);

            // Record trade
            await _riskService.RecordTradeAsync(amountSol, cancellationToken);

            var outputAmount = decimal.Parse(quote.OutAmount) / 1_000_000_000m;

            return new TradeExecutionResponse
            {
                Success = true,
                TransactionSignature = signature,
                Details = new TradeDetails
                {
                    InputMint = pair.StableCoinMint,
                    OutputMint = pair.TargetTokenMint,
                    InputAmount = amountSol,
                    OutputAmount = outputAmount,
                    PriceImpactPct = quote.PriceImpactPct,
                    ExecutedAt = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing trade for pair {PairId}", pair.Id);
            return new TradeExecutionResponse
            {
                Success = false,
                ErrorMessage = $"Trade execution failed: {ex.Message}"
            };
        }
    }

    public async Task<List<TradingPairStatusResponse>> GetAllPairStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var statuses = new List<TradingPairStatusResponse>();

        foreach (var pair in _config.TradingPairs)
        {
            var status = await GetPairStatusAsync(pair.Id, cancellationToken);
            statuses.Add(status);
        }

        return statuses;
    }

    public async Task<TradingPairStatusResponse> GetPairStatusAsync(
        string pairId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pair = _config.TradingPairs.FirstOrDefault(p => p.Id == pairId);
            if (pair == null)
            {
                return new TradingPairStatusResponse { Id = pairId };
            }

            var stableCoinBalance = await _solanaService.GetTokenBalanceAsync(
                pair.StableCoinMint,
                cancellationToken);
            var targetTokenBalance = await _solanaService.GetTokenBalanceAsync(
                pair.TargetTokenMint,
                cancellationToken);
            var dailyVolume = await _riskService.GetDailyVolumeAsync(cancellationToken);

            return new TradingPairStatusResponse
            {
                Id = pair.Id,
                Enabled = pair.Enabled,
                ProfitabilityRank = pair.ProfitabilityRank,
                CurrentProfitabilityScore = pair.CurrentProfitabilityScore,
                StableCoinBalance = stableCoinBalance,
                TargetTokenBalance = targetTokenBalance,
                DailyVolumeSol = dailyVolume,
                TradesExecutedToday = 0,
                LastTradeAt = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for pair {PairId}", pairId);
            return new TradingPairStatusResponse { Id = pairId };
        }
    }

    public Task<bool> AddTradingPairAsync(
        AddTradingPairRequest request,
        CancellationToken cancellationToken = default)
    {
        lock (_configLock)
        {
            if (_config.TradingPairs.Any(p => p.Id == request.Id))
            {
                _logger.LogWarning("Trading pair already exists: {PairId}", request.Id);
                return Task.FromResult(false);
            }

            var newPair = new TradingPairConfiguration
            {
                Id = request.Id,
                StableCoinMint = request.StableCoinMint,
                TargetTokenMint = request.TargetTokenMint,
                ProfitabilityRank = request.ProfitabilityRank,
                RiskManagement = request.RiskManagement ?? _config.RiskManagement,
                Enabled = true
            };

            _config.TradingPairs.Add(newPair);
            _logger.LogInformation("Added new trading pair: {PairId}", request.Id);
            return Task.FromResult(true);
        }
    }

    public Task<bool> UpdateProfitabilityRankAsync(
        string pairId,
        int newRank,
        CancellationToken cancellationToken = default)
    {
        lock (_configLock)
        {
            var pair = _config.TradingPairs.FirstOrDefault(p => p.Id == pairId);
            if (pair == null)
            {
                _logger.LogWarning("Trading pair not found: {PairId}", pairId);
                return Task.FromResult(false);
            }

            pair.ProfitabilityRank = newRank;
            _logger.LogInformation(
                "Updated profitability rank for {PairId} to {Rank}",
                pairId,
                newRank);
            return Task.FromResult(true);
        }
    }

    public Task<bool> EnableDisablePairAsync(
        string pairId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        lock (_configLock)
        {
            var pair = _config.TradingPairs.FirstOrDefault(p => p.Id == pairId);
            if (pair == null)
            {
                _logger.LogWarning("Trading pair not found: {PairId}", pairId);
                return Task.FromResult(false);
            }

            pair.Enabled = enabled;
            _logger.LogInformation(
                "Trading pair {PairId} {Status}",
                pairId,
                enabled ? "enabled" : "disabled");
            return Task.FromResult(true);
        }
    }
}
