using Microsoft.Extensions.Options;
using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public class ProfitabilityAnalyzer : IProfitabilityAnalyzer
{
    private readonly IJupiterApiService _jupiterApi;
    private readonly TradeBotConfiguration _config;
    private readonly ILogger<ProfitabilityAnalyzer> _logger;

    public ProfitabilityAnalyzer(
        IJupiterApiService jupiterApi,
        IOptions<TradeBotConfiguration> config,
        ILogger<ProfitabilityAnalyzer> logger)
    {
        _jupiterApi = jupiterApi;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<decimal> CalculateProfitabilityScoreAsync(
        string pairId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pair = _config.TradingPairs.FirstOrDefault(p => p.Id == pairId);
            if (pair == null)
            {
                _logger.LogWarning("Trading pair not found: {PairId}", pairId);
                return 0;
            }

            // Calculate profitability based on:
            // 1. Liquidity depth
            // 2. Price impact for standard trade size
            // 3. Recent volume
            // 4. Spread

            const decimal testAmountSol = 1.0m;
            var testAmountLamports = (long)(testAmountSol * 1_000_000_000);

            // Get quote for stablecoin -> target token
            var quoteRequest = new JupiterQuoteRequest
            {
                InputMint = pair.StableCoinMint,
                OutputMint = pair.TargetTokenMint,
                Amount = testAmountLamports,
                SlippageBps = pair.RiskManagement.SlippageBps
            };

            var quote = await _jupiterApi.GetQuoteAsync(quoteRequest, cancellationToken);

            // Score calculation:
            // - Lower price impact = higher score
            // - Base score starts at 100
            // - Deduct points for price impact
            var priceImpactPenalty = quote.PriceImpactPct * 10m; // 1% impact = 10 points penalty
            var liquidityScore = 100m - priceImpactPenalty;

            // Ensure score is between 0 and 100
            var finalScore = Math.Max(0, Math.Min(100, liquidityScore));

            _logger.LogInformation(
                "Calculated profitability score for {PairId}: {Score} (Price Impact: {Impact}%)",
                pairId,
                finalScore,
                quote.PriceImpactPct);

            return finalScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating profitability score for pair: {PairId}", pairId);
            return 0;
        }
    }

    public async Task<List<TradingPairConfiguration>> GetRankedTradingPairsAsync(
        CancellationToken cancellationToken = default)
    {
        // Return enabled pairs sorted by profitability rank (ascending = best first)
        return _config.TradingPairs
            .Where(p => p.Enabled)
            .OrderBy(p => p.ProfitabilityRank)
            .ToList();
    }

    public async Task UpdateProfitabilityScoresAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating profitability scores for all trading pairs");

        foreach (var pair in _config.TradingPairs)
        {
            if (!pair.Enabled)
                continue;

            var score = await CalculateProfitabilityScoreAsync(pair.Id, cancellationToken);
            pair.CurrentProfitabilityScore = score;
            pair.LastUpdated = DateTime.UtcNow;
        }

        _logger.LogInformation("Profitability scores updated successfully");
    }
}
