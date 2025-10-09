using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public interface IProfitabilityAnalyzer
{
    Task<decimal> CalculateProfitabilityScoreAsync(string pairId, CancellationToken cancellationToken = default);
    Task<List<TradingPairConfiguration>> GetRankedTradingPairsAsync(CancellationToken cancellationToken = default);
    Task UpdateProfitabilityScoresAsync(CancellationToken cancellationToken = default);
}
