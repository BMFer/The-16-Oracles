using The16Oracles.DAOA.Models.SplToken;

namespace The16Oracles.DAOA.Interfaces
{
    /// <summary>
    /// Service interface for executing SPL Token CLI commands
    /// </summary>
    public interface ISplTokenService
    {
        // Account Management

        /// <summary>
        /// List all token accounts by owner
        /// </summary>
        Task<SplTokenCommandResponse> GetAccountsAsync(TokenAccountsRequest request);

        /// <summary>
        /// Get wallet address
        /// </summary>
        Task<SplTokenCommandResponse> GetAddressAsync(TokenAddressRequest request);

        /// <summary>
        /// Get token account balance
        /// </summary>
        Task<SplTokenCommandResponse> GetBalanceAsync(TokenBalanceRequest request);

        /// <summary>
        /// Create a new token account
        /// </summary>
        Task<SplTokenCommandResponse> CreateAccountAsync(CreateTokenAccountRequest request);

        /// <summary>
        /// Close a token account
        /// </summary>
        Task<SplTokenCommandResponse> CloseAccountAsync(CloseTokenAccountRequest request);

        /// <summary>
        /// Cleanup unnecessary token accounts
        /// </summary>
        Task<SplTokenCommandResponse> GarbageCollectAsync(GarbageCollectRequest request);

        // Token Operations

        /// <summary>
        /// Create a new token
        /// </summary>
        Task<SplTokenCommandResponse> CreateTokenAsync(CreateTokenRequest request);

        /// <summary>
        /// Mint new tokens
        /// </summary>
        Task<SplTokenCommandResponse> MintAsync(MintTokensRequest request);

        /// <summary>
        /// Burn tokens from an account
        /// </summary>
        Task<SplTokenCommandResponse> BurnAsync(BurnTokensRequest request);

        /// <summary>
        /// Transfer tokens between accounts
        /// </summary>
        Task<SplTokenCommandResponse> TransferAsync(TransferTokensRequest request);

        /// <summary>
        /// Get token supply
        /// </summary>
        Task<SplTokenCommandResponse> GetSupplyAsync(TokenSupplyRequest request);

        /// <summary>
        /// Close a token mint
        /// </summary>
        Task<SplTokenCommandResponse> CloseMintAsync(CloseMintRequest request);

        // Token Delegation

        /// <summary>
        /// Approve a delegate for a token account
        /// </summary>
        Task<SplTokenCommandResponse> ApproveAsync(ApproveRequest request);

        /// <summary>
        /// Revoke a delegate's authority
        /// </summary>
        Task<SplTokenCommandResponse> RevokeAsync(RevokeRequest request);

        // Token Authority

        /// <summary>
        /// Authorize a new signing keypair to a token or token account
        /// </summary>
        Task<SplTokenCommandResponse> AuthorizeAsync(AuthorizeRequest request);

        // Freeze/Thaw Operations

        /// <summary>
        /// Freeze a token account
        /// </summary>
        Task<SplTokenCommandResponse> FreezeAsync(FreezeAccountRequest request);

        /// <summary>
        /// Thaw a token account
        /// </summary>
        Task<SplTokenCommandResponse> ThawAsync(ThawAccountRequest request);

        // Native SOL Wrapping

        /// <summary>
        /// Wrap native SOL in a SOL token account
        /// </summary>
        Task<SplTokenCommandResponse> WrapAsync(WrapRequest request);

        /// <summary>
        /// Unwrap a SOL token account
        /// </summary>
        Task<SplTokenCommandResponse> UnwrapAsync(UnwrapRequest request);

        /// <summary>
        /// Sync a native SOL token account to its underlying lamports
        /// </summary>
        Task<SplTokenCommandResponse> SyncNativeAsync(SyncNativeRequest request);

        // Display

        /// <summary>
        /// Query details of an SPL Token mint, account, or multisig by address
        /// </summary>
        Task<SplTokenCommandResponse> DisplayAsync(DisplayRequest request);

        // Generic Command Execution

        /// <summary>
        /// Execute a generic SPL Token CLI command
        /// </summary>
        Task<SplTokenCommandResponse> ExecuteCommandAsync(SplTokenCommandRequest request);
    }
}
