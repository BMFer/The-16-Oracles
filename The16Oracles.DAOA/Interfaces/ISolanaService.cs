using The16Oracles.DAOA.Models.Solana;

namespace The16Oracles.DAOA.Interfaces
{
    /// <summary>
    /// Service interface for executing Solana CLI commands
    /// </summary>
    public interface ISolanaService
    {
        /// <summary>
        /// Execute a generic Solana CLI command
        /// </summary>
        Task<SolanaCommandResponse> ExecuteCommandAsync(SolanaCommandRequest request);

        /// <summary>
        /// Get balance for an address
        /// </summary>
        Task<SolanaCommandResponse> GetBalanceAsync(BalanceRequest request);

        /// <summary>
        /// Transfer SOL between accounts
        /// </summary>
        Task<SolanaCommandResponse> TransferAsync(TransferRequest request);

        /// <summary>
        /// Request airdrop from faucet
        /// </summary>
        Task<SolanaCommandResponse> AirdropAsync(AirdropRequest request);

        /// <summary>
        /// Get account information
        /// </summary>
        Task<SolanaCommandResponse> GetAccountAsync(AccountRequest request);

        /// <summary>
        /// Get transaction history
        /// </summary>
        Task<SolanaCommandResponse> GetTransactionHistoryAsync(TransactionHistoryRequest request);

        /// <summary>
        /// Get block information
        /// </summary>
        Task<SolanaCommandResponse> GetBlockAsync(BlockRequest request);

        /// <summary>
        /// Get current epoch information
        /// </summary>
        Task<SolanaCommandResponse> GetEpochInfoAsync(EpochInfoRequest request);

        /// <summary>
        /// Get validators information
        /// </summary>
        Task<SolanaCommandResponse> GetValidatorsAsync(ValidatorsRequest request);

        /// <summary>
        /// Get stake account information
        /// </summary>
        Task<SolanaCommandResponse> GetStakeAccountAsync(StakeAccountRequest request);

        /// <summary>
        /// Create a stake account
        /// </summary>
        Task<SolanaCommandResponse> CreateStakeAccountAsync(CreateStakeAccountRequest request);

        /// <summary>
        /// Delegate stake to a vote account
        /// </summary>
        Task<SolanaCommandResponse> DelegateStakeAsync(DelegateStakeRequest request);

        /// <summary>
        /// Get vote account information
        /// </summary>
        Task<SolanaCommandResponse> GetVoteAccountAsync(VoteAccountRequest request);

        /// <summary>
        /// Get cluster supply information
        /// </summary>
        Task<SolanaCommandResponse> GetSupplyAsync(SupplyRequest request);

        /// <summary>
        /// Get inflation information
        /// </summary>
        Task<SolanaCommandResponse> GetInflationAsync(InflationRequest request);

        /// <summary>
        /// Get largest accounts
        /// </summary>
        Task<SolanaCommandResponse> GetLargestAccountsAsync(LargestAccountsRequest request);

        /// <summary>
        /// Confirm a transaction
        /// </summary>
        Task<SolanaCommandResponse> ConfirmTransactionAsync(ConfirmRequest request);

        /// <summary>
        /// Get recent prioritization fees
        /// </summary>
        Task<SolanaCommandResponse> GetPrioritizationFeesAsync(PrioritizationFeesRequest request);

        /// <summary>
        /// Get current block height
        /// </summary>
        Task<SolanaCommandResponse> GetBlockHeightAsync(SolanaGlobalFlags flags);

        /// <summary>
        /// Get current slot
        /// </summary>
        Task<SolanaCommandResponse> GetSlotAsync(SolanaGlobalFlags flags);

        /// <summary>
        /// Get cluster version
        /// </summary>
        Task<SolanaCommandResponse> GetClusterVersionAsync(SolanaGlobalFlags flags);

        /// <summary>
        /// Get genesis hash
        /// </summary>
        Task<SolanaCommandResponse> GetGenesisHashAsync(SolanaGlobalFlags flags);

        /// <summary>
        /// Get transaction count
        /// </summary>
        Task<SolanaCommandResponse> GetTransactionCountAsync(SolanaGlobalFlags flags);
    }
}
