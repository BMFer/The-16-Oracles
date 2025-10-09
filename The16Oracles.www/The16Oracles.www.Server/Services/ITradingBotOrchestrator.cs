using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public interface ITradingBotOrchestrator
{
    Task<CascadeExecutionResponse> ExecuteCascadeAsync(CascadeExecutionRequest request, CancellationToken cancellationToken = default);
    Task<List<TradingPairStatusResponse>> GetAllPairStatusesAsync(CancellationToken cancellationToken = default);
    Task<TradingPairStatusResponse> GetPairStatusAsync(string pairId, CancellationToken cancellationToken = default);
    Task<bool> AddTradingPairAsync(AddTradingPairRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateProfitabilityRankAsync(string pairId, int newRank, CancellationToken cancellationToken = default);
    Task<bool> EnableDisablePairAsync(string pairId, bool enabled, CancellationToken cancellationToken = default);
}
