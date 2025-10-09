namespace The16Oracles.www.Server.Services;

public interface ISolanaTransactionService
{
    Task<string> ExecuteSwapTransactionAsync(string base64Transaction, CancellationToken cancellationToken = default);
    Task<decimal> GetSolBalanceAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTokenBalanceAsync(string tokenMint, CancellationToken cancellationToken = default);
    Task<bool> VerifyMinimumBalanceAsync(CancellationToken cancellationToken = default);
    string GetWalletPublicKey();
}
