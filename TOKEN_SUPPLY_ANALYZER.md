# Token Supply Concentration Analyzer

A comprehensive tool for analyzing token supply concentration and identifying suspicious holders on Solana SPL tokens.

## Overview

The **TokenSupplyAnalyzer** service in `The16Oracles.domain` leverages DAOA Solana and SPL Token API endpoints to:
- Analyze token supply concentration and distribution
- Calculate concentration risk metrics (Gini coefficient, Herfindahl index)
- Identify suspicious holders using multiple detection algorithms
- Detect clusters of related suspicious wallets
- Provide comprehensive risk assessments for tokens

## Key Features

### Supply Concentration Analysis
- **Top Holder Percentages**: Track top 1, 5, 10, and 20 holders
- **Gini Coefficient**: Measure wealth inequality (0 = perfect equality, 1 = perfect inequality)
- **Herfindahl-Hirschman Index**: Sum of squared market shares
- **Concentration Score**: 0-100 composite risk score
- **Risk Levels**: VeryLow, Low, Medium, High, VeryHigh, Critical

### Suspicious Holder Detection
Identifies holders with suspicious patterns using 9 different flags:
- **NewWallet**: Created within last 7 days
- **LowActivity**: Fewer than 10 transactions
- **LargeConcentration**: Holds >5% of supply
- **RelatedToOtherHolders**: Connected to other large holders
- **SimilarBalances**: Similar token amounts to other wallets
- **SimilarCreationTime**: Created around same time as others
- **OnlyThisToken**: No token diversification
- **NoSolBalance**: Insufficient SOL for transaction fees
- **RapidAccumulation**: Fast token accumulation

### Cluster Identification
- Uses graph traversal to identify groups of related suspicious holders
- Integrates with `WalletRelationshipAnalyzer` for relationship detection
- Calculates aggregate metrics for each cluster

## Architecture

```
┌─────────────────────────────────────────────────────┐
│         TokenSupplyAnalyzer Service                 │
│                                                     │
│  ┌────────────────────────────────────────────┐   │
│  │  Core Analysis Functions                   │   │
│  │  - GetTokenSupplyAsync()                   │   │
│  │  - GetHoldersFromWalletsAsync()            │   │
│  │  - CalculateConcentrationAsync()           │   │
│  │  - IdentifySuspiciousHoldersAsync()        │   │
│  │  - IdentifySuspiciousClustersAsync()       │   │
│  └────────────────┬───────────────────────────┘   │
│                   │                                 │
│  ┌────────────────▼───────────────────────────┐   │
│  │  DAOA API Endpoints                        │   │
│  │  - /api/spl-token/supply                   │   │
│  │  - /api/spl-token/accounts                 │   │
│  │  - /api/solana/balance                     │   │
│  └────────────────┬───────────────────────────┘   │
│                   │                                 │
│  ┌────────────────▼───────────────────────────┐   │
│  │  WalletRelationshipAnalyzer Integration    │   │
│  │  - GetWalletProfileAsync()                 │   │
│  │  - FindRelationshipsAsync()                │   │
│  └────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## Models

Located in `The16Oracles.domain/Models/WalletAnalysis.cs`:

### Request/Response Models
- **AnalyzeTokenSupplyRequest**: Configure analysis parameters
- **AnalyzeTokenSupplyResponse**: Contains results and errors/warnings
- **TokenSupplyAnalysisResult**: Complete analysis data

### Data Models
- **TokenSupplyInfo**: Token supply and metadata
- **TokenHolder**: Individual holder information with balance and rank
- **SupplyConcentration**: Concentration metrics and risk assessment
- **SuspiciousHolder**: Suspicious holder with flags and score
- **SupplyAnalysisStatistics**: Analysis summary statistics

### Enumerations
- **ConcentrationRisk**: VeryLow to Critical (6 levels)
- **SuspiciousFlags**: 9 combinable flags for suspicious behavior

## Usage

### Basic Example

```csharp
using The16Oracles.domain.Services;
using The16Oracles.domain.Models;

// Initialize analyzer
var analyzer = new TokenSupplyAnalyzer("https://localhost:5001");

// Prepare wallet addresses to analyze (you need to provide these)
var walletAddresses = new List<string>
{
    "Wallet1Address...",
    "Wallet2Address...",
    "Wallet3Address...",
    // ... more wallet addresses
};

// Get holders from wallet list
var tokenMint = "YourTokenMintAddressHere";
var holders = await analyzer.GetHoldersFromWalletsAsync(
    tokenMint,
    walletAddresses,
    "devnet");

// Build analysis request
var request = new AnalyzeTokenSupplyRequest
{
    TokenMintAddress = tokenMint,
    Network = "devnet",
    MaxHoldersToAnalyze = 100,
    MinimumHoldingPercentage = 0.01m, // 0.01% minimum
    AnalyzeSuspiciousHolders = true,
    IncludeRelationshipAnalysis = true
};

// Note: The analyzer requires wallet addresses to be provided
// In production, you would use an indexing service like Helius or QuickNode
// to enumerate all token holders

// Analyze token supply
var response = await analyzer.AnalyzeTokenSupplyAsync(request);

if (response.Success && response.Data != null)
{
    var data = response.Data;

    Console.WriteLine($"Token: {data.SupplyInfo.TokenMintAddress}");
    Console.WriteLine($"Total Supply: {data.SupplyInfo.TotalSupply:N0}");
    Console.WriteLine($"Total Holders Analyzed: {data.Statistics.TotalHoldersAnalyzed}");
    Console.WriteLine();

    // Concentration Analysis
    Console.WriteLine("=== CONCENTRATION ANALYSIS ===");
    Console.WriteLine($"Risk Level: {data.Concentration.RiskLevel}");
    Console.WriteLine($"Risk Description: {data.Concentration.RiskDescription}");
    Console.WriteLine($"Concentration Score: {data.Concentration.ConcentrationScore:F2}/100");
    Console.WriteLine($"Top 1 Holder: {data.Concentration.Top1HolderPercentage:F2}%");
    Console.WriteLine($"Top 5 Holders: {data.Concentration.Top5HoldersPercentage:F2}%");
    Console.WriteLine($"Top 10 Holders: {data.Concentration.Top10HoldersPercentage:F2}%");
    Console.WriteLine($"Gini Coefficient: {data.Concentration.GiniCoefficient:F4}");
    Console.WriteLine($"Herfindahl Index: {data.Concentration.HerfindahlIndex:F4}");
    Console.WriteLine();

    // Top Holders
    Console.WriteLine("=== TOP 10 HOLDERS ===");
    foreach (var holder in data.TopHolders.Take(10))
    {
        Console.WriteLine($"#{holder.Rank}: {holder.WalletAddress.Substring(0, 8)}...");
        Console.WriteLine($"   Balance: {holder.Balance:N2} ({holder.PercentageOfSupply:F2}%)");
    }
    Console.WriteLine();

    // Suspicious Holders
    if (data.SuspiciousHolders.Any())
    {
        Console.WriteLine($"=== SUSPICIOUS HOLDERS ({data.SuspiciousHolders.Count}) ===");
        foreach (var suspicious in data.SuspiciousHolders.Take(10))
        {
            Console.WriteLine($"Wallet: {suspicious.WalletAddress.Substring(0, 8)}...");
            Console.WriteLine($"Suspicious Score: {suspicious.SuspiciousScore}/100");
            Console.WriteLine($"Holding: {suspicious.PercentageOfSupply:F2}%");
            Console.WriteLine($"Flags: {suspicious.Flags}");
            Console.WriteLine("Reasons:");
            foreach (var reason in suspicious.Reasons)
            {
                Console.WriteLine($"  - {reason}");
            }
            if (suspicious.RelatedWallets?.Any() == true)
            {
                Console.WriteLine($"Related to {suspicious.RelatedWallets.Count} other holders");
            }
            Console.WriteLine();
        }
    }

    // Suspicious Clusters
    if (data.SuspiciousClusters.Any())
    {
        Console.WriteLine($"=== SUSPICIOUS CLUSTERS ({data.SuspiciousClusters.Count}) ===");
        foreach (var cluster in data.SuspiciousClusters)
        {
            Console.WriteLine($"Cluster: {cluster.ClusterId}");
            Console.WriteLine($"Wallets: {cluster.WalletAddresses.Count}");
            Console.WriteLine($"Avg Relationship Score: {cluster.AverageRelationshipScore:P1}");
            Console.WriteLine($"Description: {cluster.Description}");
            Console.WriteLine();
        }
    }
}
else
{
    Console.WriteLine($"Analysis failed: {response.Message}");
    foreach (var error in response.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
```

### Advanced Configuration

```csharp
var request = new AnalyzeTokenSupplyRequest
{
    TokenMintAddress = "YourTokenMint...",
    Network = "mainnet-beta",
    MaxHoldersToAnalyze = 200,
    MinimumHoldingPercentage = 0.1m, // Only analyze holders with >0.1%
    AnalyzeSuspiciousHolders = true,
    IncludeRelationshipAnalysis = true,

    // Configure wallet relationship analysis
    WalletAnalysisConfig = new AnalysisConfiguration
    {
        MinimumRelationshipScore = 0.5m,
        MinimumSharedTokens = 3,
        TokenBalanceSimilarityThreshold = 0.85m,
        IncludeInactiveWallets = false
    }
};
```

### Get Token Supply Only

```csharp
var supplyInfo = await analyzer.GetTokenSupplyAsync(
    "TokenMintAddress",
    "mainnet-beta");

if (supplyInfo != null)
{
    Console.WriteLine($"Total Supply: {supplyInfo.TotalSupply:N0}");
    Console.WriteLine($"Retrieved: {supplyInfo.RetrievedAt}");
}
```

### Calculate Concentration Metrics

```csharp
var holders = new List<TokenHolder>
{
    new() { Balance = 500000, PercentageOfSupply = 50 },
    new() { Balance = 300000, PercentageOfSupply = 30 },
    new() { Balance = 200000, PercentageOfSupply = 20 }
};

var concentration = await analyzer.CalculateConcentrationAsync(holders, 1000000);

Console.WriteLine($"Risk Level: {concentration.RiskLevel}");
Console.WriteLine($"Gini: {concentration.GiniCoefficient:F4}");
Console.WriteLine($"HHI: {concentration.HerfindahlIndex:F4}");
Console.WriteLine($"Score: {concentration.ConcentrationScore:F2}");
```

## Risk Assessment Guide

### Concentration Risk Levels

| Risk Level | Concentration Score | Description |
|------------|-------------------|-------------|
| **VeryLow** | 0-15 | Well-distributed supply, low risk |
| **Low** | 15-30 | Some concentration, monitor |
| **Medium** | 30-45 | Moderate concentration, caution advised |
| **High** | 45-60 | Significant concentration, high risk |
| **VeryHigh** | 60-80 | Highly concentrated, very high risk |
| **Critical** | 80-100 | Extreme concentration, potential rug pull risk |

### Suspicious Score Interpretation

| Score | Risk | Action |
|-------|------|--------|
| 0-20 | Low | Normal holder |
| 21-40 | Medium | Monitor activity |
| 41-60 | High | Investigate further |
| 61-80 | Very High | Red flag - exercise caution |
| 81-100 | Critical | Severe warning - potential scam |

### Concentration Metrics Explained

#### Gini Coefficient
- Measures inequality in distribution
- 0 = perfect equality (all holders have same amount)
- 1 = perfect inequality (one holder has everything)
- Typical healthy tokens: 0.3-0.6

#### Herfindahl-Hirschman Index (HHI)
- Sum of squared market shares
- Higher values = more concentrated
- < 0.15: Competitive/distributed
- 0.15-0.25: Moderately concentrated
- > 0.25: Highly concentrated

## Use Cases

### 1. Token Due Diligence

```csharp
// Before investing in a token, analyze concentration risk
var analyzer = new TokenSupplyAnalyzer("https://localhost:5001");

var request = new AnalyzeTokenSupplyRequest
{
    TokenMintAddress = newToken,
    Network = "mainnet-beta",
    AnalyzeSuspiciousHolders = true
};

var result = await analyzer.AnalyzeTokenSupplyAsync(request);

if (result.Data?.Concentration.RiskLevel == ConcentrationRisk.Critical)
{
    Console.WriteLine("WARNING: Extreme concentration - potential rug pull risk!");
}
```

### 2. Rug Pull Detection

```csharp
// Detect potential rug pull setup
var holders = await analyzer.GetHoldersFromWalletsAsync(tokenMint, walletList, "mainnet-beta");
var concentration = await analyzer.CalculateConcentrationAsync(holders, totalSupply);

bool potentialRugPull =
    concentration.Top1HolderPercentage > 40 ||
    concentration.Top5HoldersPercentage > 80 ||
    concentration.RiskLevel == ConcentrationRisk.Critical;

if (potentialRugPull)
{
    Console.WriteLine("HIGH RISK: Supply concentration suggests rug pull potential");
}
```

### 3. Fair Launch Verification

```csharp
// Verify fair token distribution
var response = await analyzer.AnalyzeTokenSupplyAsync(new AnalyzeTokenSupplyRequest
{
    TokenMintAddress = tokenMint,
    IncludeRelationshipAnalysis = true
});

bool fairDistribution =
    response.Data.Concentration.Top1HolderPercentage < 10 &&
    response.Data.SuspiciousClusters.Count == 0 &&
    response.Data.Concentration.RiskLevel <= ConcentrationRisk.Medium;

Console.WriteLine(fairDistribution
    ? "Fair distribution detected"
    : "Questionable distribution detected");
```

### 4. Sybil Attack Detection

```csharp
// Identify coordinated wallet clusters
var response = await analyzer.AnalyzeTokenSupplyAsync(request);

if (response.Data.SuspiciousClusters.Any())
{
    Console.WriteLine("Potential Sybil attack detected:");
    foreach (var cluster in response.Data.SuspiciousClusters)
    {
        var totalClusterHolding = response.Data.SuspiciousHolders
            .Where(h => cluster.WalletAddresses.Contains(h.WalletAddress))
            .Sum(h => h.PercentageOfSupply);

        Console.WriteLine($"Cluster {cluster.ClusterId}: {cluster.WalletAddresses.Count} wallets");
        Console.WriteLine($"Combined holding: {totalClusterHolding:F2}%");
    }
}
```

### 5. Token Health Monitoring

```csharp
// Monitor token health over time
public async Task MonitorTokenHealth(string tokenMint)
{
    var analyzer = new TokenSupplyAnalyzer("https://localhost:5001");

    while (true)
    {
        var result = await analyzer.AnalyzeTokenSupplyAsync(new AnalyzeTokenSupplyRequest
        {
            TokenMintAddress = tokenMint,
            Network = "mainnet-beta"
        });

        if (result.Success)
        {
            LogMetrics(result.Data.Concentration, result.Data.Statistics);

            if (result.Data.Concentration.RiskLevel >= ConcentrationRisk.High)
            {
                await SendAlert($"Token {tokenMint} risk increased to {result.Data.Concentration.RiskLevel}");
            }
        }

        await Task.Delay(TimeSpan.FromHours(1));
    }
}
```

## Important Notes

### Token Holder Enumeration

The current implementation requires wallet addresses to be provided via `GetHoldersFromWalletsAsync()`. In production environments, you should use:

1. **Helius API** - Comprehensive Solana indexer
2. **QuickNode** - Fast RPC with token holder endpoints
3. **Metaplex** - For NFT and token metadata
4. **Custom Indexer** - Scan chain programmatically

Example integration with external service:

```csharp
// Pseudo-code for integration with indexing service
public async Task<List<string>> GetAllTokenHolders(string tokenMint)
{
    // Call external indexing service
    var response = await heliusClient.GetTokenHolders(tokenMint);
    return response.Holders.Select(h => h.Address).ToList();
}

// Use in analyzer
var allHolders = await GetAllTokenHolders(tokenMint);
var holders = await analyzer.GetHoldersFromWalletsAsync(tokenMint, allHolders, "mainnet-beta");
```

### Performance Considerations

- **Batch Size**: Analyze up to 100 wallets per request for optimal performance
- **Network Latency**: Allow 2-5 minutes for comprehensive analysis
- **Rate Limits**: Respect DAOA API rate limits
- **Caching**: Consider caching wallet profiles for repeated analysis

### Accuracy

- Concentration metrics are as accurate as the wallet sample provided
- Relationship detection requires at least 2 wallets
- Historical data is limited to recent transactions
- Some holders may be exchanges or liquidity pools (normal concentration)

## Testing

### Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~TokenSupplyAnalyzerTests"
```

### Test Coverage

30 comprehensive tests covering:
- Model initialization and validation
- Suspicious flags combinations
- Concentration risk calculations
- Gini coefficient and HHI calculations
- Top holder percentage calculations
- Request/response validation
- Edge cases and error handling

## API Integration

The analyzer works with The16Oracles.DAOA endpoints:

- `POST /api/spl-token/supply` - Get token supply
- `POST /api/spl-token/accounts` - Get token accounts
- `POST /api/solana/balance` - Get SOL balance
- `POST /api/solana/transaction-history` - Get transaction history

Ensure DAOA API is running:

```bash
cd The16Oracles.DAOA
dotnet run
```

Access Swagger UI: `https://localhost:5001/swagger`

## Source Code

- **Service**: `The16Oracles.domain/Services/TokenSupplyAnalyzer.cs`
- **Models**: `The16Oracles.domain/Models/WalletAnalysis.cs`
- **Tests**: `The16Oracles.domain.nunit/Services/TokenSupplyAnalyzerTests.cs`

## Related Documentation

- **WALLET_RELATIONSHIP_ANALYZER.md** - Wallet relationship detection system
- **SPL-TOKEN-API.md** - SPL Token API reference
- **SolanaAPI.md** - Solana CLI API reference

## Future Enhancements

- Direct on-chain token holder enumeration
- Historical concentration tracking
- Machine learning for anomaly detection
- Real-time monitoring webhooks
- Integration with token metadata services
- Automated risk reports
- Discord/Telegram alert integration

## License & Credits

© 2025 The16Oracles Project
Part of The 16 Oracles comprehensive crypto analytics platform.

---

**For support and questions:**
- See `The16Oracles.domain/Services/TokenSupplyAnalyzer.cs` for implementation details
- See `TokenSupplyAnalyzerTests.cs` for usage examples
- Refer to WALLET_RELATIONSHIP_ANALYZER.md for relationship analysis features
