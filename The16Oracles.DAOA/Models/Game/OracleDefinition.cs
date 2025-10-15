namespace The16Oracles.DAOA.Models.Game;

public class OracleDefinition
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal SubscriptionCost { get; set; }
    public int PowerLevel { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = string.Empty;
}

public static class OracleRegistry
{
    public static readonly List<OracleDefinition> Oracles = new()
    {
        new OracleDefinition
        {
            Name = "macro-trends",
            DisplayName = "Macro Economic Trends",
            Description = "Analyzes global economic indicators and trends",
            SubscriptionCost = 15m,
            PowerLevel = 3,
            Category = "Market Analysis",
            ApiEndpoint = "/api/oracles/macro-trends"
        },
        new OracleDefinition
        {
            Name = "defi-liquidity",
            DisplayName = "DeFi Liquidity Movements",
            Description = "Tracks liquidity flows across DeFi protocols",
            SubscriptionCost = 20m,
            PowerLevel = 4,
            Category = "Market Analysis",
            ApiEndpoint = "/api/oracles/defi-liquidity"
        },
        new OracleDefinition
        {
            Name = "whale-behavior",
            DisplayName = "Crypto Whale Behavior",
            Description = "Monitors large wallet movements and whale activities",
            SubscriptionCost = 30m,
            PowerLevel = 5,
            Category = "Market Analysis",
            ApiEndpoint = "/api/oracles/crypto-whale-behavior"
        },
        new OracleDefinition
        {
            Name = "nft-sentiment",
            DisplayName = "NFT Market Sentiment",
            Description = "Analyzes NFT market trends and sentiment",
            SubscriptionCost = 10m,
            PowerLevel = 2,
            Category = "Market Analysis",
            ApiEndpoint = "/api/oracles/nft-sentiment"
        },
        new OracleDefinition
        {
            Name = "black-swan",
            DisplayName = "Black Swan Detection",
            Description = "Identifies rare, high-impact market events",
            SubscriptionCost = 50m,
            PowerLevel = 8,
            Category = "Risk Detection",
            ApiEndpoint = "/api/oracles/black-swan"
        },
        new OracleDefinition
        {
            Name = "regulatory-risk",
            DisplayName = "Regulatory Risk Forecasting",
            Description = "Predicts regulatory changes and compliance risks",
            SubscriptionCost = 25m,
            PowerLevel = 4,
            Category = "Risk Detection",
            ApiEndpoint = "/api/oracles/regulatory-risk"
        },
        new OracleDefinition
        {
            Name = "airdrop-opportunities",
            DisplayName = "Airdrop Launch Opportunities",
            Description = "Discovers upcoming airdrops and launch opportunities",
            SubscriptionCost = 15m,
            PowerLevel = 3,
            Category = "Opportunities",
            ApiEndpoint = "/api/oracles/airdrop-opportunities"
        },
        new OracleDefinition
        {
            Name = "emerging-market-surge",
            DisplayName = "Emerging Market Capital Surge",
            Description = "Detects capital surges in emerging markets",
            SubscriptionCost = 35m,
            PowerLevel = 6,
            Category = "Opportunities",
            ApiEndpoint = "/api/oracles/emerging-market-surge"
        },
        new OracleDefinition
        {
            Name = "l2-activity",
            DisplayName = "L2 Activity Monitoring",
            Description = "Tracks Layer 2 network activities and growth",
            SubscriptionCost = 20m,
            PowerLevel = 4,
            Category = "Technical Metrics",
            ApiEndpoint = "/api/oracles/l2-activity"
        },
        new OracleDefinition
        {
            Name = "chain-interoperability",
            DisplayName = "Chain Interoperability Metrics",
            Description = "Measures cross-chain interaction and bridges",
            SubscriptionCost = 25m,
            PowerLevel = 5,
            Category = "Technical Metrics",
            ApiEndpoint = "/api/oracles/chain-interoperability"
        },
        new OracleDefinition
        {
            Name = "node-validator-profits",
            DisplayName = "Node Validator Profits",
            Description = "Analyzes validator profitability and network health",
            SubscriptionCost = 15m,
            PowerLevel = 3,
            Category = "Technical Metrics",
            ApiEndpoint = "/api/oracles/node-validator-profits"
        },
        new OracleDefinition
        {
            Name = "ai-narrative-trends",
            DisplayName = "AI Narrative Trend Detection",
            Description = "Identifies emerging AI and tech narratives",
            SubscriptionCost = 40m,
            PowerLevel = 7,
            Category = "Trends",
            ApiEndpoint = "/api/oracles/ai-narrative-trends"
        },
        new OracleDefinition
        {
            Name = "tokenomics-supply",
            DisplayName = "Tokenomics Supply Curves",
            Description = "Analyzes token supply dynamics and economics",
            SubscriptionCost = 20m,
            PowerLevel = 4,
            Category = "Trends",
            ApiEndpoint = "/api/oracles/tokenomics-supply"
        },
        new OracleDefinition
        {
            Name = "tech-adoption",
            DisplayName = "Tech Adoption Curves",
            Description = "Tracks technology adoption and innovation rates",
            SubscriptionCost = 25m,
            PowerLevel = 5,
            Category = "Trends",
            ApiEndpoint = "/api/oracles/tech-adoption"
        }
    };
}
