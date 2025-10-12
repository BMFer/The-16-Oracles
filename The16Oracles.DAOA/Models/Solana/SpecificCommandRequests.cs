namespace The16Oracles.DAOA.Models.Solana
{
    /// <summary>
    /// Request for balance command
    /// </summary>
    public class BalanceRequest
    {
        public string? Address { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for transfer command
    /// </summary>
    public class TransferRequest
    {
        public string RecipientAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for airdrop command
    /// </summary>
    public class AirdropRequest
    {
        public decimal Amount { get; set; }
        public string? Address { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for account command
    /// </summary>
    public class AccountRequest
    {
        public string Address { get; set; } = string.Empty;
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for transaction history
    /// </summary>
    public class TransactionHistoryRequest
    {
        public string Address { get; set; } = string.Empty;
        public int? Limit { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for block information
    /// </summary>
    public class BlockRequest
    {
        public long Slot { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for epoch information
    /// </summary>
    public class EpochInfoRequest
    {
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for validators information
    /// </summary>
    public class ValidatorsRequest
    {
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for stake account operations
    /// </summary>
    public class StakeAccountRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for creating stake account
    /// </summary>
    public class CreateStakeAccountRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for delegating stake
    /// </summary>
    public class DelegateStakeRequest
    {
        public string StakeAccount { get; set; } = string.Empty;
        public string VoteAccount { get; set; } = string.Empty;
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for vote account operations
    /// </summary>
    public class VoteAccountRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for supply information
    /// </summary>
    public class SupplyRequest
    {
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for inflation information
    /// </summary>
    public class InflationRequest
    {
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for largest accounts
    /// </summary>
    public class LargestAccountsRequest
    {
        public int? Limit { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for transaction logs
    /// </summary>
    public class LogsRequest
    {
        public string? Address { get; set; }
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for confirming a transaction
    /// </summary>
    public class ConfirmRequest
    {
        public string Signature { get; set; } = string.Empty;
        public SolanaGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for recent prioritization fees
    /// </summary>
    public class PrioritizationFeesRequest
    {
        public SolanaGlobalFlags Flags { get; set; } = new();
    }
}
