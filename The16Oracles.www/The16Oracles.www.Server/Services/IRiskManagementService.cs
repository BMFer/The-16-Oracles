using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public interface IRiskManagementService
{
    Task<RiskCheckResult> CheckTradeRiskAsync(decimal notionalSol, CancellationToken cancellationToken = default);
    Task RecordTradeAsync(decimal notionalSol, CancellationToken cancellationToken = default);
    Task<decimal> GetDailyVolumeAsync(CancellationToken cancellationToken = default);
    Task ResetDailyCountersAsync(CancellationToken cancellationToken = default);
}
