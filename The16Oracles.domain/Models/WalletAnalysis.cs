namespace The16Oracles.domain.Models
{
    /// <summary>
    /// Represents a Solana wallet profile with token holdings
    /// </summary>
    public class WalletProfile
    {
        public string Address { get; set; } = string.Empty;
        public decimal SolBalance { get; set; }
        public List<TokenHolding> TokenHoldings { get; set; } = new();
        public DateTime LastUpdated { get; set; }
        public WalletMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a token holding in a wallet
    /// </summary>
    public class TokenHolding
    {
        public string TokenMintAddress { get; set; } = string.Empty;
        public string AccountAddress { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int Decimals { get; set; }
        public string? TokenName { get; set; }
        public string? TokenSymbol { get; set; }
        public DateTime? LastTransferDate { get; set; }
    }

    /// <summary>
    /// Metadata about wallet activity and characteristics
    /// </summary>
    public class WalletMetadata
    {
        public int TotalTransactions { get; set; }
        public DateTime? FirstTransactionDate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public int UniqueTokenCount { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents a relationship between two wallets
    /// </summary>
    public class WalletRelationship
    {
        public string Wallet1Address { get; set; } = string.Empty;
        public string Wallet2Address { get; set; } = string.Empty;
        public decimal RelationshipScore { get; set; }
        public RelationshipType Type { get; set; }
        public List<RelationshipEvidence> Evidence { get; set; } = new();
        public DateTime AnalyzedAt { get; set; }
    }

    /// <summary>
    /// Types of relationships that can be detected
    /// </summary>
    public enum RelationshipType
    {
        None,
        LikelyRelated,           // High probability of being related
        PossiblyRelated,         // Moderate probability
        SharedTokens,            // Share multiple tokens
        FrequentTransfers,       // Frequent direct transfers
        MirrorTrading,           // Similar trading patterns
        CommonAuthority,         // Share same authority/owner
        SuspiciousActivity,      // Patterns suggesting coordination
        ClusterMember            // Part of a wallet cluster
    }

    /// <summary>
    /// Evidence supporting a relationship between wallets
    /// </summary>
    public class RelationshipEvidence
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Confidence { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();
    }

    /// <summary>
    /// Pattern matching configuration
    /// </summary>
    public class AnalysisConfiguration
    {
        public decimal MinimumRelationshipScore { get; set; } = 0.3m;
        public int MinimumSharedTokens { get; set; } = 2;
        public decimal TokenBalanceSimilarityThreshold { get; set; } = 0.8m;
        public int MinimumTransactionsForAnalysis { get; set; } = 5;
        public bool IncludeInactiveWallets { get; set; } = false;
        public int MaxWalletsToAnalyze { get; set; } = 100;
    }

    /// <summary>
    /// Result of analyzing multiple wallets
    /// </summary>
    public class WalletClusterAnalysis
    {
        public List<WalletProfile> Wallets { get; set; } = new();
        public List<WalletRelationship> Relationships { get; set; } = new();
        public List<WalletCluster> Clusters { get; set; } = new();
        public DateTime AnalyzedAt { get; set; }
        public AnalysisStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// A cluster of related wallets
    /// </summary>
    public class WalletCluster
    {
        public string ClusterId { get; set; } = string.Empty;
        public List<string> WalletAddresses { get; set; } = new();
        public RelationshipType PrimaryRelationType { get; set; }
        public decimal AverageRelationshipScore { get; set; }
        public List<string> SharedTokens { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Statistics about the analysis
    /// </summary>
    public class AnalysisStatistics
    {
        public int TotalWalletsAnalyzed { get; set; }
        public int TotalRelationshipsFound { get; set; }
        public int TotalClustersFound { get; set; }
        public decimal AverageTokensPerWallet { get; set; }
        public decimal HighestRelationshipScore { get; set; }
        public TimeSpan AnalysisDuration { get; set; }
    }

    /// <summary>
    /// Request to analyze wallet relationships
    /// </summary>
    public class AnalyzeWalletsRequest
    {
        public List<string> WalletAddresses { get; set; } = new();
        public AnalysisConfiguration Configuration { get; set; } = new();
        public string? Network { get; set; } = "devnet";
    }

    /// <summary>
    /// Response from wallet analysis
    /// </summary>
    public class AnalyzeWalletsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public WalletClusterAnalysis? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    // ==================== SUPPLY CONCENTRATION MODELS ====================

    /// <summary>
    /// Represents token supply and holder information
    /// </summary>
    public class TokenSupplyInfo
    {
        public string TokenMintAddress { get; set; } = string.Empty;
        public string? TokenSymbol { get; set; }
        public string? TokenName { get; set; }
        public decimal TotalSupply { get; set; }
        public int Decimals { get; set; }
        public DateTime RetrievedAt { get; set; }
    }

    /// <summary>
    /// Represents a holder of a specific token
    /// </summary>
    public class TokenHolder
    {
        public string WalletAddress { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal PercentageOfSupply { get; set; }
        public int Rank { get; set; }
        public WalletProfile? WalletProfile { get; set; }
    }

    /// <summary>
    /// Supply concentration metrics for a token
    /// </summary>
    public class SupplyConcentration
    {
        public string TokenMintAddress { get; set; } = string.Empty;
        public decimal TotalSupply { get; set; }
        public int TotalHolders { get; set; }

        // Top holder percentages
        public decimal Top1HolderPercentage { get; set; }
        public decimal Top5HoldersPercentage { get; set; }
        public decimal Top10HoldersPercentage { get; set; }
        public decimal Top20HoldersPercentage { get; set; }

        // Concentration scores
        public decimal GiniCoefficient { get; set; }
        public decimal HerfindahlIndex { get; set; }
        public decimal ConcentrationScore { get; set; } // 0-100, higher = more concentrated

        // Risk levels
        public ConcentrationRisk RiskLevel { get; set; }
        public string RiskDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Concentration risk levels
    /// </summary>
    public enum ConcentrationRisk
    {
        VeryLow,    // Well distributed
        Low,        // Moderately distributed
        Medium,     // Some concentration
        High,       // Significant concentration
        VeryHigh,   // Extreme concentration
        Critical    // Dangerous concentration (potential rug risk)
    }

    /// <summary>
    /// Flags indicating suspicious holder behavior
    /// </summary>
    [Flags]
    public enum SuspiciousFlags
    {
        None = 0,
        NewWallet = 1 << 0,              // Wallet created very recently
        LowActivity = 1 << 1,            // Very few transactions
        LargeConcentration = 1 << 2,     // Holds large % of supply
        RelatedToOtherHolders = 1 << 3,  // Related to other large holders
        SimilarBalances = 1 << 4,        // Similar balance to other wallets
        SimilarCreationTime = 1 << 5,    // Created around same time as others
        OnlyThisToken = 1 << 6,          // Only holds this token
        NoSolBalance = 1 << 7,           // Has no SOL for fees
        RapidAccumulation = 1 << 8       // Accumulated quickly
    }

    /// <summary>
    /// Suspicious holder information
    /// </summary>
    public class SuspiciousHolder
    {
        public string WalletAddress { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal PercentageOfSupply { get; set; }
        public SuspiciousFlags Flags { get; set; }
        public decimal SuspiciousScore { get; set; } // 0-100
        public List<string> Reasons { get; set; } = new();
        public WalletProfile? WalletProfile { get; set; }
        public List<string>? RelatedWallets { get; set; }
    }

    /// <summary>
    /// Request to analyze token supply concentration
    /// </summary>
    public class AnalyzeTokenSupplyRequest
    {
        public string TokenMintAddress { get; set; } = string.Empty;
        public string Network { get; set; } = "devnet";
        public int MaxHoldersToAnalyze { get; set; } = 100;
        public decimal MinimumHoldingPercentage { get; set; } = 0.01m; // 0.01% minimum
        public bool AnalyzeSuspiciousHolders { get; set; } = true;
        public bool IncludeRelationshipAnalysis { get; set; } = true;
        public AnalysisConfiguration? WalletAnalysisConfig { get; set; }
    }

    /// <summary>
    /// Result of token supply concentration analysis
    /// </summary>
    public class TokenSupplyAnalysisResult
    {
        public TokenSupplyInfo SupplyInfo { get; set; } = new();
        public SupplyConcentration Concentration { get; set; } = new();
        public List<TokenHolder> TopHolders { get; set; } = new();
        public List<SuspiciousHolder> SuspiciousHolders { get; set; } = new();
        public List<WalletCluster> SuspiciousClusters { get; set; } = new();
        public DateTime AnalyzedAt { get; set; }
        public SupplyAnalysisStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// Statistics about supply analysis
    /// </summary>
    public class SupplyAnalysisStatistics
    {
        public int TotalHoldersAnalyzed { get; set; }
        public int SuspiciousHoldersFound { get; set; }
        public int SuspiciousClustersFound { get; set; }
        public decimal TotalSupplyAnalyzed { get; set; }
        public decimal PercentageAnalyzed { get; set; }
        public TimeSpan AnalysisDuration { get; set; }
    }

    /// <summary>
    /// Response from token supply analysis
    /// </summary>
    public class AnalyzeTokenSupplyResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TokenSupplyAnalysisResult? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
