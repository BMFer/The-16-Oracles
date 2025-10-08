using System.Text;
using Microsoft.Extensions.Options;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public class SolanaTransactionService : ISolanaTransactionService
{
    private readonly IRpcClient _rpcClient;
    private readonly Wallet _botWallet;
    private readonly SolanaConfiguration _config;
    private readonly RiskManagementConfiguration _riskConfig;
    private readonly ILogger<SolanaTransactionService> _logger;

    public SolanaTransactionService(
        IOptions<TradeBotConfiguration> config,
        ILogger<SolanaTransactionService> logger)
    {
        _config = config.Value.Solana;
        _riskConfig = config.Value.RiskManagement;
        _logger = logger;

        if (string.IsNullOrEmpty(_config.RpcUrl))
        {
            throw new InvalidOperationException("Solana RPC URL is not configured");
        }

        _rpcClient = ClientFactory.GetClient(_config.RpcUrl);

        if (string.IsNullOrEmpty(_config.BotPrivateKey))
        {
            throw new InvalidOperationException("Bot private key is not configured. Set TradeBot:Solana:BotPrivateKey in appsettings or user secrets.");
        }

        try
        {
            _botWallet = new Wallet(_config.BotPrivateKey);
            _logger.LogInformation("Solana wallet initialized: {PublicKey}", _botWallet.Account.PublicKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize bot wallet from private key");
            throw new InvalidOperationException("Failed to initialize bot wallet. Check private key format.", ex);
        }
    }

    public async Task<string> ExecuteSwapTransactionAsync(
        string base64Transaction,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing swap transaction");

            // Deserialize the transaction
            var transactionBytes = Convert.FromBase64String(base64Transaction);
            var transaction = Transaction.Deserialize(transactionBytes);

            // Get recent blockhash
            var blockHashResult = await _rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
            if (!blockHashResult.WasSuccessful)
            {
                throw new InvalidOperationException($"Failed to get recent blockhash: {blockHashResult.Reason}");
            }

            transaction.RecentBlockHash = blockHashResult.Result.Value.Blockhash;
            transaction.FeePayer = _botWallet.Account.PublicKey;

            // Sign the transaction
            transaction.Sign(_botWallet.Account);

            // Send transaction
            var signature = await _rpcClient.SendTransactionAsync(
                transaction.Serialize(),
                skipPreflight: false,
                commitment: Commitment.Confirmed);

            if (!signature.WasSuccessful)
            {
                _logger.LogError("Transaction failed: {Reason}", signature.Reason);
                throw new InvalidOperationException($"Failed to send transaction: {signature.Reason}");
            }

            _logger.LogInformation("Transaction sent: {Signature}", signature.Result);

            // Confirm transaction
            var confirmed = await WaitForConfirmationAsync(signature.Result, cancellationToken);
            if (!confirmed)
            {
                throw new InvalidOperationException("Transaction failed to confirm");
            }

            _logger.LogInformation("Transaction confirmed: {Signature}", signature.Result);

            return signature.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing swap transaction");
            throw;
        }
    }

    public async Task<decimal> GetSolBalanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var balanceResult = await _rpcClient.GetBalanceAsync(_botWallet.Account.PublicKey, Commitment.Confirmed);
            if (!balanceResult.WasSuccessful)
            {
                _logger.LogError("Failed to get SOL balance: {Reason}", balanceResult.Reason);
                return 0;
            }

            var lamports = balanceResult.Result.Value;
            var sol = lamports / 1_000_000_000m;

            _logger.LogDebug("SOL Balance: {Balance} SOL ({Lamports} lamports)", sol, lamports);

            return sol;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SOL balance");
            return 0;
        }
    }

    public async Task<decimal> GetTokenBalanceAsync(string tokenMint, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get token accounts by owner
            var tokenAccounts = await _rpcClient.GetTokenAccountsByOwnerAsync(
                _botWallet.Account.PublicKey.Key,
                tokenMint,
                tokenProgramId: null);

            if (!tokenAccounts.WasSuccessful || tokenAccounts.Result?.Value == null || tokenAccounts.Result.Value.Count == 0)
            {
                _logger.LogDebug("No token accounts found for mint: {Mint}", tokenMint);
                return 0;
            }

            // Get the first token account balance
            var tokenAccount = tokenAccounts.Result.Value[0];
            var balance = tokenAccount.Account.Data.Parsed.Info.TokenAmount.AmountDecimal;

            _logger.LogDebug("Token Balance for {Mint}: {Balance}", tokenMint, balance);

            return balance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token balance for mint: {Mint}", tokenMint);
            return 0;
        }
    }

    public async Task<bool> VerifyMinimumBalanceAsync(CancellationToken cancellationToken = default)
    {
        var balance = await GetSolBalanceAsync(cancellationToken);
        var hasMinimum = balance >= _riskConfig.MinBalanceSol;

        if (!hasMinimum)
        {
            _logger.LogWarning(
                "Insufficient SOL balance. Current: {Balance} SOL, Minimum required: {MinBalance} SOL",
                balance,
                _riskConfig.MinBalanceSol);
        }

        return hasMinimum;
    }

    public string GetWalletPublicKey()
    {
        return _botWallet.Account.PublicKey.Key;
    }

    private async Task<bool> WaitForConfirmationAsync(string signature, CancellationToken cancellationToken)
    {
        const int maxAttempts = 30;
        const int delayMs = 2000;

        for (int i = 0; i < maxAttempts; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                var statusResult = await _rpcClient.GetSignatureStatusesAsync(new[] { signature }, true);
                if (statusResult.WasSuccessful && statusResult.Result?.Value != null)
                {
                    var status = statusResult.Result.Value[0];
                    if (status?.ConfirmationStatus == "confirmed" || status?.ConfirmationStatus == "finalized")
                    {
                        if (status.Err != null)
                        {
                            _logger.LogError("Transaction failed with error: {Error}", status.Err);
                            return false;
                        }
                        return true;
                    }
                }

                await Task.Delay(delayMs, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking transaction status, attempt {Attempt}/{Max}", i + 1, maxAttempts);
            }
        }

        _logger.LogError("Transaction confirmation timeout after {Attempts} attempts", maxAttempts);
        return false;
    }
}
