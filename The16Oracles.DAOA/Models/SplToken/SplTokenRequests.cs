namespace The16Oracles.DAOA.Models.SplToken
{
    // Account Management Requests

    /// <summary>
    /// Request for accounts command - List all token accounts by owner
    /// </summary>
    public class TokenAccountsRequest
    {
        public string? TokenMintAddress { get; set; }
        public string? Owner { get; set; }
        public bool AddressesOnly { get; set; }
        public bool Delegated { get; set; }
        public bool ExternallyCloseable { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for address command - Get wallet address
    /// </summary>
    public class TokenAddressRequest
    {
        public string? Owner { get; set; }
        public string? Token { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for balance command - Get token account balance
    /// </summary>
    public class TokenBalanceRequest
    {
        public string? TokenMintAddress { get; set; }
        public string? Address { get; set; }
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for create-account command - Create a new token account
    /// </summary>
    public class CreateTokenAccountRequest
    {
        public string TokenMintAddress { get; set; } = string.Empty;
        public string? AccountKeypair { get; set; }
        public string? Owner { get; set; }
        public bool Immutable { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for close command - Close a token account
    /// </summary>
    public class CloseTokenAccountRequest
    {
        public string? TokenMintAddress { get; set; }
        public string? Address { get; set; }
        public string? Owner { get; set; }
        public string? CloseAuthority { get; set; }
        public string? Recipient { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for gc command - Cleanup unnecessary token accounts
    /// </summary>
    public class GarbageCollectRequest
    {
        public bool CloseEmptyAssociatedAccounts { get; set; }
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Token Operations Requests

    /// <summary>
    /// Request for create-token command - Create a new token
    /// </summary>
    public class CreateTokenRequest
    {
        public string? TokenKeypair { get; set; }
        public string? MintAuthority { get; set; }
        public int? Decimals { get; set; }
        public bool EnableFreeze { get; set; }
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for mint command - Mint new tokens
    /// </summary>
    public class MintTokensRequest
    {
        public string TokenAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? RecipientAddress { get; set; }
        public string? MintAuthority { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for burn command - Burn tokens from an account
    /// </summary>
    public class BurnTokensRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for transfer command - Transfer tokens between accounts
    /// </summary>
    public class TransferTokensRequest
    {
        public string TokenAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string RecipientAddress { get; set; } = string.Empty;
        public string? From { get; set; }
        public string? Owner { get; set; }
        public bool Fund { get; set; }
        public bool AllowUnfundedRecipient { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for supply command - Get token supply
    /// </summary>
    public class TokenSupplyRequest
    {
        public string TokenAddress { get; set; } = string.Empty;
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for close-mint command - Close a token mint
    /// </summary>
    public class CloseMintRequest
    {
        public string TokenAddress { get; set; } = string.Empty;
        public string? CloseAuthority { get; set; }
        public string? Recipient { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Token Delegation Requests

    /// <summary>
    /// Request for approve command - Approve a delegate for a token account
    /// </summary>
    public class ApproveRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DelegateAddress { get; set; } = string.Empty;
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for revoke command - Revoke a delegate's authority
    /// </summary>
    public class RevokeRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Token Authority Requests

    /// <summary>
    /// Request for authorize command - Authorize a new signing keypair
    /// </summary>
    public class AuthorizeRequest
    {
        public string Address { get; set; } = string.Empty;
        public string AuthorityType { get; set; } = string.Empty; // mint, freeze, owner, close
        public string NewAuthority { get; set; } = string.Empty;
        public string? CurrentAuthority { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Freeze/Thaw Requests

    /// <summary>
    /// Request for freeze command - Freeze a token account
    /// </summary>
    public class FreezeAccountRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public string? FreezeAuthority { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for thaw command - Thaw a token account
    /// </summary>
    public class ThawAccountRequest
    {
        public string AccountAddress { get; set; } = string.Empty;
        public string? FreezeAuthority { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Native SOL Wrapping Requests

    /// <summary>
    /// Request for wrap command - Wrap native SOL in a SOL token account
    /// </summary>
    public class WrapRequest
    {
        public decimal Amount { get; set; }
        public string? WalletKeypair { get; set; }
        public bool CreateAux { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for unwrap command - Unwrap a SOL token account
    /// </summary>
    public class UnwrapRequest
    {
        public string? Address { get; set; }
        public string? WalletKeypair { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    /// <summary>
    /// Request for sync-native command - Sync a native SOL token account
    /// </summary>
    public class SyncNativeRequest
    {
        public string? Address { get; set; }
        public string? Owner { get; set; }
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Display Request

    /// <summary>
    /// Request for display command - Query details of an SPL Token mint, account, or multisig
    /// </summary>
    public class DisplayRequest
    {
        public string Address { get; set; } = string.Empty;
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }

    // Generic Command Request

    /// <summary>
    /// Generic request for executing any SPL Token command
    /// </summary>
    public class SplTokenCommandRequest
    {
        public string Command { get; set; } = string.Empty;
        public Dictionary<string, string> Arguments { get; set; } = new();
        public SplTokenGlobalFlags Flags { get; set; } = new();
    }
}
