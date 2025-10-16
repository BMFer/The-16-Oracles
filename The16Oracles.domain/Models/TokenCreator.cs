namespace The16Oracles.domain.Models
{
    /// <summary>
    /// Information about a token's creator and mint authority
    /// </summary>
    public class TokenCreatorInfo
    {
        public string TokenMintAddress { get; set; } = string.Empty;
        public string? MintAuthority { get; set; }
        public string? FreezeAuthority { get; set; }
        public int Decimals { get; set; }
        public decimal Supply { get; set; }
        public bool IsInitialized { get; set; }
        public DateTime RetrievedAt { get; set; }
        public string? TokenName { get; set; }
        public string? TokenSymbol { get; set; }
    }

    /// <summary>
    /// Creator information for a token account
    /// </summary>
    public class TokenAccountCreatorInfo
    {
        public string AccountAddress { get; set; } = string.Empty;
        public string TokenMintAddress { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public TokenCreatorInfo? CreatorInfo { get; set; }
    }

    /// <summary>
    /// Request to get token creators from account list
    /// </summary>
    public class GetTokenCreatorsRequest
    {
        public List<string> AccountAddresses { get; set; } = new();
        public string Network { get; set; } = "devnet";
        public bool IncludeZeroBalances { get; set; } = false;
        public bool FetchSupplyInfo { get; set; } = true;
    }

    /// <summary>
    /// Result of token creator analysis
    /// </summary>
    public class TokenCreatorsResult
    {
        public List<TokenAccountCreatorInfo> Accounts { get; set; } = new();
        public Dictionary<string, TokenCreatorInfo> UniqueCreators { get; set; } = new();
        public DateTime AnalyzedAt { get; set; }
        public TokenCreatorStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// Statistics about token creator analysis
    /// </summary>
    public class TokenCreatorStatistics
    {
        public int TotalAccountsAnalyzed { get; set; }
        public int TotalUniqueTokens { get; set; }
        public int TotalUniqueCreators { get; set; }
        public int AccountsWithNoAuthority { get; set; }
        public int AccountsWithSameAuthority { get; set; }
        public TimeSpan AnalysisDuration { get; set; }
    }

    /// <summary>
    /// Response from token creator analysis
    /// </summary>
    public class GetTokenCreatorsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenCreatorsResult? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Grouped token accounts by creator
    /// </summary>
    public class TokensByCreator
    {
        public string CreatorAddress { get; set; } = string.Empty;
        public List<TokenAccountCreatorInfo> Tokens { get; set; } = new();
        public int TotalTokenCount { get; set; }
        public decimal TotalValueHeld { get; set; }
    }

    /// <summary>
    /// Request to group tokens by creator
    /// </summary>
    public class GroupTokensByCreatorRequest
    {
        public List<string> AccountAddresses { get; set; } = new();
        public string Network { get; set; } = "devnet";
        public bool IncludeZeroBalances { get; set; } = false;
        public int MinimumTokenCount { get; set; } = 1;
    }

    /// <summary>
    /// Response from grouping tokens by creator
    /// </summary>
    public class GroupTokensByCreatorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<TokensByCreator> Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
