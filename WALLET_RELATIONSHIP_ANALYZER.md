# Wallet Relationship Analyzer

A comprehensive tool for analyzing relationships between Solana wallets using pattern matching of SOL balances, token holdings, transaction history, and temporal patterns.

## Overview

The **WalletRelationshipAnalyzer** is a service in `The16Oracles.domain` that leverages The16Oracles.DAOA Solana and SPL Token API endpoints to:
- Retrieve wallet profiles with SOL balances and token holdings
- Identify relationships between wallets using multiple pattern matching algorithms
- Cluster related wallets into groups
- Provide detailed evidence for each detected relationship

## Architecture

### Components

```
┌─────────────────────────────────────────────────────┐
│      WalletRelationshipAnalyzer Service            │
│                                                     │
│  ┌────────────────────────────────────────────┐   │
│  │  Pattern Matching Algorithms               │   │
│  │  - Shared Tokens Detection                 │   │
│  │  - Balance Similarity Analysis             │   │
│  │  - Temporal Proximity Detection            │   │
│  │  - Activity Correlation                    │   │
│  │  - Cluster Identification                  │   │
│  └─────────────────┬──────────────────────────┘   │
│                    │                                │
│  ┌─────────────────▼──────────────────────────┐   │
│  │  HTTP Client to DAOA Endpoints             │   │
│  │  - /api/solana/balance                     │   │
│  │  - /api/spl-token/accounts                 │   │
│  │  - /api/solana/transaction-history         │   │
│  └────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────┐
│          The16Oracles.DAOA Web API                  │
│                                                     │
│  ┌────────────────────────────────────────────┐   │
│  │  Solana CLI Service                        │   │
│  │  SPL Token CLI Service                     │   │
│  └────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

### Models

Located in `The16Oracles.domain/Models/WalletAnalysis.cs`:

- **WalletProfile** - Complete wallet information including SOL balance, token holdings, and metadata
- **TokenHolding** - Individual token account details
- **WalletMetadata** - Transaction history and activity metrics
- **WalletRelationship** - Relationship between two wallets with evidence
- **RelationshipEvidence** - Supporting data for detected relationships
- **WalletCluster** - Group of related wallets
- **AnalysisConfiguration** - Configurable thresholds and parameters

### Relationship Types

The analyzer can detect the following relationship types:

1. **LikelyRelated** (score ≥ 0.7) - High probability of being controlled by the same entity
2. **PossiblyRelated** (score ≥ 0.5) - Moderate probability of relationship
3. **SharedTokens** - Wallets holding multiple common tokens
4. **MirrorTrading** - Similar trading patterns and balances
5. **SuspiciousActivity** - Multiple indicators suggesting coordination
6. **ClusterMember** - Part of a larger group of related wallets

## Pattern Matching Algorithms

### 1. Shared Tokens Detection (Weight: 0.3)

Identifies wallets holding the same tokens.

```csharp
Score = Min(1.0, SharedTokenCount / 10) × 0.3
```

**Evidence Generated:**
- Number of shared tokens
- List of common token mint addresses

**Threshold:** Minimum 2 shared tokens (configurable)

### 2. Balance Similarity Analysis (Weight: 0.25)

Compares token balances for shared tokens.

```csharp
For each shared token:
  Similarity = 1 - (|Balance1 - Balance2| / ((Balance1 + Balance2) / 2))

Score = Average(Similarities) × 0.25
```

**Evidence Generated:**
- Overall similarity score
- Per-token balance comparison

**Threshold:** ≥ 80% similarity (configurable)

### 3. SOL Balance Similarity (Weight: 0.15)

Compares SOL balances between wallets.

```csharp
Similarity = 1 - (|SOL1 - SOL2| / ((SOL1 + SOL2) / 2))
Score = Similarity × 0.15 (if similarity > 80%)
```

**Evidence Generated:**
- Individual SOL balances
- Similarity percentage

### 4. Temporal Proximity Detection (Weight: 0.2)

Identifies wallets created around the same time.

```csharp
DaysDifference = |FirstTx1 - FirstTx2|
Score = ((7 - DaysDifference) / 7) × 0.2 (if < 7 days)
```

**Evidence Generated:**
- First transaction dates
- Days difference

**Window:** 7 days (hardcoded)

### 5. Activity Correlation (Weight: 0.1)

Compares transaction activity levels.

```csharp
Similarity = 1 - (|TxCount1 - TxCount2| / ((TxCount1 + TxCount2) / 2))
Score = Similarity × 0.1 (if similarity > 75%)
```

**Evidence Generated:**
- Transaction counts
- Activity similarity score

### 6. Cluster Identification

Groups wallets into clusters using graph traversal:

1. Start with each unprocessed wallet
2. Find all relationships above minimum score threshold
3. Recursively add connected wallets to cluster
4. Calculate cluster statistics:
   - Average relationship score
   - Primary relationship type (most common)
   - Shared tokens across all wallets

## Usage

### Basic Example

```csharp
using The16Oracles.domain.Services;
using The16Oracles.domain.Models;

// Initialize analyzer
var analyzer = new WalletRelationshipAnalyzer("https://localhost:5001");

// Prepare request
var request = new AnalyzeWalletsRequest
{
    WalletAddresses = new List<string>
    {
        "7EqQdEULxWcraVx3mXKFjc84LhCkMGZCkRuDpvcMwJeK",
        "CuieVDEDtLo7FypA9SbLM9saXFdb1dsshEkyErMqkRQq",
        "FwYdNECxCmvJeUXm7JZgWQ3q5KkLJnqB4v1qJfnZQQ6P"
    },
    Configuration = new AnalysisConfiguration
    {
        MinimumRelationshipScore = 0.3m,
        MinimumSharedTokens = 2,
        TokenBalanceSimilarityThreshold = 0.8m,
        IncludeInactiveWallets = false,
        MaxWalletsToAnalyze = 100
    },
    Network = "devnet"
};

// Analyze wallets
var response = await analyzer.AnalyzeWalletsAsync(request);

if (response.Success)
{
    Console.WriteLine($"Analyzed {response.Data.Statistics.TotalWalletsAnalyzed} wallets");
    Console.WriteLine($"Found {response.Data.Statistics.TotalRelationshipsFound} relationships");
    Console.WriteLine($"Identified {response.Data.Statistics.TotalClustersFound} clusters");

    foreach (var relationship in response.Data.Relationships)
    {
        Console.WriteLine($"\n{relationship.Wallet1Address} <-> {relationship.Wallet2Address}");
        Console.WriteLine($"Score: {relationship.RelationshipScore:P1}");
        Console.WriteLine($"Type: {relationship.Type}");
        Console.WriteLine($"Evidence: {relationship.Evidence.Count} pieces");

        foreach (var evidence in relationship.Evidence)
        {
            Console.WriteLine($"  - {evidence.Type}: {evidence.Description}");
        }
    }
}
else
{
    Console.WriteLine($"Analysis failed: {response.Message}");
}
```

### Advanced Configuration

```csharp
var config = new AnalysisConfiguration
{
    // Minimum score to consider a relationship (0-1)
    MinimumRelationshipScore = 0.4m,

    // Minimum number of shared tokens required
    MinimumSharedTokens = 3,

    // Threshold for balance similarity (0-1)
    TokenBalanceSimilarityThreshold = 0.85m,

    // Minimum transactions to consider wallet active
    MinimumTransactionsForAnalysis = 10,

    // Include wallets with no recent activity
    IncludeInactiveWallets = true,

    // Maximum wallets to analyze in one request
    MaxWalletsToAnalyze = 50
};
```

### Retrieve Individual Wallet Profile

```csharp
var profile = await analyzer.GetWalletProfileAsync(
    "7EqQdEULxWcraVx3mXKFjc84LhCkMGZCkRuDpvcMwJeK",
    network: "mainnet-beta"
);

if (profile != null)
{
    Console.WriteLine($"Address: {profile.Address}");
    Console.WriteLine($"SOL Balance: {profile.SolBalance}");
    Console.WriteLine($"Token Holdings: {profile.TokenHoldings.Count}");
    Console.WriteLine($"Total Transactions: {profile.Metadata.TotalTransactions}");
    Console.WriteLine($"Is Active: {profile.Metadata.IsActive}");

    foreach (var holding in profile.TokenHoldings)
    {
        Console.WriteLine($"  Token: {holding.TokenMintAddress}");
        Console.WriteLine($"  Balance: {holding.Balance} (decimals: {holding.Decimals})");
    }
}
```

### Analyze Specific Wallet Pairs

```csharp
var wallets = new List<WalletProfile>
{
    await analyzer.GetWalletProfileAsync(wallet1Address, "devnet"),
    await analyzer.GetWalletProfileAsync(wallet2Address, "devnet")
};

var config = new AnalysisConfiguration { MinimumRelationshipScore = 0.1m };
var relationships = await analyzer.FindRelationshipsAsync(wallets, config);

foreach (var rel in relationships)
{
    Console.WriteLine($"Relationship Score: {rel.RelationshipScore:P1}");
    Console.WriteLine($"Type: {rel.Type}");
}
```

## API Response Structure

### AnalyzeWalletsResponse

```json
{
  "success": true,
  "message": "Successfully analyzed 3 wallets, found 2 relationships and 1 clusters",
  "data": {
    "wallets": [...],
    "relationships": [
      {
        "wallet1Address": "7EqQd...",
        "wallet2Address": "CuieV...",
        "relationshipScore": 0.65,
        "type": "SharedTokens",
        "evidence": [
          {
            "type": "SharedTokens",
            "description": "Both wallets hold 5 common tokens",
            "confidence": 0.3,
            "details": {
              "SharedTokenCount": 5,
              "SharedTokens": ["token1", "token2", ...]
            }
          },
          {
            "type": "SimilarBalances",
            "description": "Token balances are 85% similar",
            "confidence": 0.25,
            "details": {
              "SimilarityScore": 0.85
            }
          }
        ],
        "analyzedAt": "2025-10-15T12:00:00Z"
      }
    ],
    "clusters": [
      {
        "clusterId": "cluster-1",
        "walletAddresses": ["7EqQd...", "CuieV...", "FwYdN..."],
        "primaryRelationType": "SharedTokens",
        "averageRelationshipScore": 0.68,
        "sharedTokens": ["token1", "token2"],
        "description": "Cluster of 3 wallets with SharedTokens relationship pattern"
      }
    ],
    "analyzedAt": "2025-10-15T12:00:00Z",
    "statistics": {
      "totalWalletsAnalyzed": 3,
      "totalRelationshipsFound": 2,
      "totalClustersFound": 1,
      "averageTokensPerWallet": 8.5,
      "highestRelationshipScore": 0.75,
      "analysisDuration": "00:00:12.3456789"
    }
  },
  "errors": [],
  "warnings": ["Wallet X is inactive and was excluded from analysis"]
}
```

## Testing

### Unit Tests

Located in `The16Oracles.domain.nunit/Services/WalletRelationshipAnalyzerTests.cs`:

**Test Coverage:**
- ✅ 19 tests covering all major functionality
- ✅ Model initialization and validation
- ✅ Shared token detection
- ✅ Balance similarity calculation
- ✅ SOL balance similarity
- ✅ Activity correlation
- ✅ Temporal proximity detection
- ✅ Multiple wallet pair analysis
- ✅ Relationship type classification
- ✅ Request/response models

**Run Tests:**
```bash
dotnet test --filter "FullyQualifiedName~WalletRelationshipAnalyzerTests"
```

### Manual Testing

1. **Start DAOA API:**
   ```bash
   cd The16Oracles.DAOA
   dotnet run
   ```

2. **Create Test Program:**
   ```csharp
   // In a test console app or script
   var analyzer = new WalletRelationshipAnalyzer("https://localhost:5001");

   var request = new AnalyzeWalletsRequest
   {
       WalletAddresses = new List<string>
       {
           "your-wallet-address-1",
           "your-wallet-address-2"
       },
       Network = "devnet"
   };

   var result = await analyzer.AnalyzeWalletsAsync(request);
   Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
   ```

## Use Cases

### 1. Fraud Detection

Identify potentially fraudulent wallet clusters:

```csharp
var config = new AnalysisConfiguration
{
    MinimumRelationshipScore = 0.7m, // High threshold
    MinimumSharedTokens = 5,
    TokenBalanceSimilarityThreshold = 0.9m
};

var result = await analyzer.AnalyzeWalletsAsync(new AnalyzeWalletsRequest
{
    WalletAddresses = suspiciousWallets,
    Configuration = config
});

// Flag clusters with SuspiciousActivity type
var suspiciousClusters = result.Data.Clusters
    .Where(c => c.PrimaryRelationType == RelationshipType.SuspiciousActivity)
    .ToList();
```

### 2. Airdrop Sybil Detection

Prevent multiple airdrop claims from related wallets:

```csharp
var airdropParticipants = GetAirdropWallets();

var result = await analyzer.AnalyzeWalletsAsync(new AnalyzeWalletsRequest
{
    WalletAddresses = airdropParticipants,
    Configuration = new AnalysisConfiguration
    {
        MinimumRelationshipScore = 0.5m,
        MinimumSharedTokens = 3
    }
});

// Identify and exclude related wallets
var sybilClusters = result.Data.Clusters
    .Where(c => c.WalletAddresses.Count > 1)
    .ToList();

foreach (var cluster in sybilClusters)
{
    // Keep only first wallet from each cluster
    var walletsToExclude = cluster.WalletAddresses.Skip(1);
    ExcludeFromAirdrop(walletsToExclude);
}
```

### 3. Whale Tracking

Monitor related whale wallets:

```csharp
var whaleWallets = GetLargeBalanceWallets(minBalance: 10000);

var result = await analyzer.AnalyzeWalletsAsync(new AnalyzeWalletsRequest
{
    WalletAddresses = whaleWallets,
    Configuration = new AnalysisConfiguration
    {
        MinimumRelationshipScore = 0.3m,
        IncludeInactiveWallets = true
    }
});

// Track whale clusters for market intelligence
foreach (var cluster in result.Data.Clusters)
{
    var totalBalance = cluster.WalletAddresses
        .Sum(addr => GetWalletBalance(addr));

    Console.WriteLine($"Whale cluster {cluster.ClusterId}: {totalBalance} SOL");
    MonitorCluster(cluster);
}
```

### 4. Governance Vote Analysis

Detect coordinated voting patterns:

```csharp
var voters = GetProposalVoters(proposalId);

var result = await analyzer.AnalyzeWalletsAsync(new AnalyzeWalletsRequest
{
    WalletAddresses = voters,
    Configuration = new AnalysisConfiguration
    {
        MinimumRelationshipScore = 0.6m,
        MinimumSharedTokens = 2
    }
});

// Identify voting cartels
var coordinatedVoters = result.Data.Relationships
    .Where(r => r.Type == RelationshipType.LikelyRelated ||
                r.Type == RelationshipType.SuspiciousActivity)
    .SelectMany(r => new[] { r.Wallet1Address, r.Wallet2Address })
    .Distinct()
    .ToList();
```

## Performance Considerations

### Optimization Tips

1. **Batch Size:** Keep wallet count under 100 per request
   ```csharp
   var batches = wallets.Chunk(100);
   foreach (var batch in batches)
   {
       await analyzer.AnalyzeWalletsAsync(new AnalyzeWalletsRequest
       {
           WalletAddresses = batch.ToList()
       });
   }
   ```

2. **Network Selection:** Use `devnet` for testing to avoid rate limits

3. **Configuration Tuning:** Adjust thresholds to reduce false positives
   ```csharp
   // Stricter = fewer results but higher confidence
   MinimumRelationshipScore = 0.7m,
   MinimumSharedTokens = 5
   ```

4. **Inactive Wallets:** Exclude inactive wallets to improve speed
   ```csharp
   IncludeInactiveWallets = false
   ```

### Complexity

- **Time Complexity:** O(n²) for pairwise comparisons where n = number of wallets
- **Space Complexity:** O(n × t) where t = average tokens per wallet
- **Typical Duration:** 2-30 seconds for 10-100 wallets (depends on network latency)

## Limitations

1. **API Dependency:** Requires DAOA endpoints to be running
2. **Network Latency:** Performance depends on Solana network response times
3. **Transaction History:** Limited to last 100 transactions per wallet
4. **Token Metadata:** Token names/symbols not always available
5. **Historical Data:** No access to historical balance snapshots

## Future Enhancements

### Planned Features

- [ ] **Direct Transfer Detection** - Analyze transaction history for direct transfers between wallets
- [ ] **Time Series Analysis** - Track balance changes over time
- [ ] **Machine Learning** - ML model for relationship prediction
- [ ] **Graph Visualization** - Export wallet relationship graphs
- [ ] **Real-time Monitoring** - Subscribe to wallet changes
- [ ] **Custom Scoring Weights** - User-configurable algorithm weights
- [ ] **Database Caching** - Cache wallet profiles for faster repeat analysis
- [ ] **Parallel Processing** - Multi-threaded wallet fetching
- [ ] **Token Metadata Enrichment** - Fetch token names and symbols
- [ ] **Pagination Support** - Handle large wallet sets efficiently

### Contribution Ideas

- Add NFT holding similarity detection
- Implement staking position correlation
- Create Discord bot integration
- Build web dashboard for visualization
- Add export to JSON/CSV formats

## Troubleshooting

### Common Issues

**Problem:** "No wallet addresses provided"
```csharp
// Solution: Ensure WalletAddresses list is populated
request.WalletAddresses = new List<string> { "wallet1", "wallet2" };
```

**Problem:** "API request failed: 500"
```csharp
// Solution: Verify DAOA is running
// In terminal: cd The16Oracles.DAOA && dotnet run
```

**Problem:** "Need at least 2 valid wallets"
```csharp
// Solution: Check wallet addresses are valid and accessible
// Set IncludeInactiveWallets = true if needed
```

**Problem:** No relationships found
```csharp
// Solution: Lower the minimum score threshold
config.MinimumRelationshipScore = 0.1m;
config.MinimumSharedTokens = 1;
```

**Problem:** Timeout errors
```csharp
// Solution: Reduce batch size or increase timeout
var analyzer = new WalletRelationshipAnalyzer("https://localhost:5001");
analyzer._httpClient.Timeout = TimeSpan.FromMinutes(5);
```

## Integration Examples

### Discord Bot Integration

```csharp
[Command("analyze-wallets")]
public async Task AnalyzeWalletsCommand(CommandContext ctx, params string[] addresses)
{
    var analyzer = new WalletRelationshipAnalyzer("https://localhost:5001");

    var response = await analyzer.AnalyzeWalletsAsync(new AnalyzeWalletsRequest
    {
        WalletAddresses = addresses.ToList()
    });

    if (response.Success)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Wallet Relationship Analysis")
            .WithDescription($"Found {response.Data.Relationships.Count} relationships")
            .WithColor(DiscordColor.Green);

        foreach (var rel in response.Data.Relationships.Take(5))
        {
            embed.AddField(
                $"{rel.Wallet1Address.Substring(0, 8)}... <-> {rel.Wallet2Address.Substring(0, 8)}...",
                $"Type: {rel.Type}, Score: {rel.RelationshipScore:P0}"
            );
        }

        await ctx.RespondAsync(embed: embed);
    }
}
```

### Web API Endpoint

```csharp
app.MapPost("/api/wallets/analyze", async (
    IWalletRelationshipAnalyzer analyzer,
    AnalyzeWalletsRequest request) =>
{
    var result = await analyzer.AnalyzeWalletsAsync(request);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
})
.WithName("AnalyzeWallets")
.WithTags("Wallet Analysis")
.WithOpenApi();
```

## License & Credits

© 2025 The16Oracles Project
Part of The 16 Oracles comprehensive crypto analytics platform.

---

**For more information:**
- See `The16Oracles.domain/Services/WalletRelationshipAnalyzer.cs` for implementation
- See `The16Oracles.domain/Models/WalletAnalysis.cs` for data models
- See `The16Oracles.domain.nunit/Services/WalletRelationshipAnalyzerTests.cs` for test examples
