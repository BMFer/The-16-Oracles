using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public interface IJupiterApiService
{
    Task<JupiterQuoteResponse> GetQuoteAsync(JupiterQuoteRequest request, CancellationToken cancellationToken = default);
    Task<JupiterSwapResponse> GetSwapTransactionAsync(JupiterSwapRequest request, CancellationToken cancellationToken = default);
}
