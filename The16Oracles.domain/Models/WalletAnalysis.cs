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
}
