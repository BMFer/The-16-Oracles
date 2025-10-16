# Token Creator Analyzer

A service for retrieving token creator addresses (mint authorities) from a list of token account addresses on Solana.

## Overview

The **TokenCreatorAnalyzer** service in `The16Oracles.domain` uses The16Oracles.DAOA SPL Token API endpoints to:
- Get token account details from account addresses
- Retrieve token mint information including creator/mint authority
- Group tokens by their creators
- Provide statistics about token ownership and creation

## Key Features

### Token Creator Retrieval
- Get mint authority (creator) for any token
- Retrieve freeze authority information
- Access token supply and decimals
- Check token initialization status

### Account Analysis
- Analyze multiple token accounts in one request
- Filter zero-balance accounts
- Track unique tokens and creators
- Generate comprehensive statistics

### Grouping & Organization
- Group tokens by creator address
- Count tokens per creator
- Calculate total value held per creator
- Filter by minimum token count

## Architecture

```
┌─────────────────────────────────────────────────────┐
│         TokenCreatorAnalyzer Service                │
│                                                     │
│  ┌────────────────────────────────────────────┐   │
│  │  Core Functions                            │   │
│  │  - GetTokenCreatorsAsync()                 │   │
│  │  - GetTokenCreatorInfoAsync()              │   │
│  │  - GetAccountCreatorInfoAsync()            │   │
│  │  - GroupTokensByCreatorAsync()             │   │
│  └────────────────┬───────────────────────────┘   │
│                   │                                 │
│  ┌────────────────▼───────────────────────────┐   │
│  │  DAOA SPL Token API                        │   │
│  │  - POST /api/spl-token/display             │   │
│  │    (for both accounts and mints)           │   │
│  └────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## Models

Located in `The16Oracles.domain/Models/TokenCreator.cs`:

### Core Models
- **TokenCreatorInfo**: Token mint authority and metadata
- **TokenAccountCreatorInfo**: Account with linked creator info
- **TokensByCreator**: Grouped tokens by creator address

### Request/Response Models
- **GetTokenCreatorsRequest**: Configure token creator analysis
- **GetTokenCreatorsResponse**: Results with statistics
- **GroupTokensByCreatorRequest**: Configure grouping parameters
- **GroupTokensByCreatorResponse**: Grouped results

### Statistics
- **TokenCreatorStatistics**: Analysis metrics and counts

## Usage

### Basic Example - Get Creators from Account List

```csharp
using The16Oracles.domain.Services;
using The16Oracles.domain.Models;

// Initialize analyzer
var analyzer = new TokenCreatorAnalyzer("https://localhost:5001");

// Prepare request with token account addresses
var request = new GetTokenCreatorsRequest
{
    AccountAddresses = new List<string>
    {
        "TokenAccount1Address...",
        "TokenAccount2Address...",
        "TokenAccount3Address..."
    },
    Network = "devnet",
    IncludeZeroBalances = false,
    FetchSupplyInfo = true
};

// Analyze accounts and get creators
var response = await analyzer.GetTokenCreatorsAsync(request);

if (response.Success && response.Data != null)
{
    Console.WriteLine($"Analyzed {response.Data.Statistics.TotalAccountsAnalyzed} accounts");
    Console.WriteLine($"Found {response.Data.Statistics.TotalUniqueTokens} unique tokens");
    Console.WriteLine($"Found {response.Data.Statistics.TotalUniqueCreators} unique creators");
    Console.WriteLine();

    // Display account information
    foreach (var account in response.Data.Accounts)
    {
        Console.WriteLine($"Account: {account.AccountAddress}");
        Console.WriteLine($"  Token Mint: {account.TokenMintAddress}");
        Console.WriteLine($"  Owner: {account.Owner}");
        Console.WriteLine($"  Balance: {account.Balance}");

        if (account.CreatorInfo != null)
        {
            Console.WriteLine($"  Creator (Mint Authority): {account.CreatorInfo.MintAuthority}");
            Console.WriteLine($"  Freeze Authority: {account.CreatorInfo.FreezeAuthority}");
            Console.WriteLine($"  Supply: {account.CreatorInfo.Supply}");
            Console.WriteLine($"  Decimals: {account.CreatorInfo.Decimals}");
        }
        Console.WriteLine();
    }

    // Display unique creators
    Console.WriteLine("=== UNIQUE CREATORS ===");
    foreach (var creator in response.Data.UniqueCreators)
    {
        Console.WriteLine($"Creator: {creator.Key}");
        Console.WriteLine($"  Token: {creator.Value.TokenMintAddress}");
        Console.WriteLine($"  Supply: {creator.Value.Supply}");
        Console.WriteLine();
    }
}
else
{
    Console.WriteLine($"Analysis failed: {response.Message}");
}
```

### Get Token Creator Info Directly

```csharp
// Get creator info for a specific token mint
var creatorInfo = await analyzer.GetTokenCreatorInfoAsync(
    "TokenMintAddress...",
    "mainnet-beta");

if (creatorInfo != null)
{
    Console.WriteLine($"Token Mint: {creatorInfo.TokenMintAddress}");
    Console.WriteLine($"Mint Authority (Creator): {creatorInfo.MintAuthority}");
    Console.WriteLine($"Freeze Authority: {creatorInfo.FreezeAuthority}");
    Console.WriteLine($"Total Supply: {creatorInfo.Supply}");
    Console.WriteLine($"Decimals: {creatorInfo.Decimals}");
    Console.WriteLine($"Is Initialized: {creatorInfo.IsInitialized}");
    Console.WriteLine($"Retrieved At: {creatorInfo.RetrievedAt}");
}
```

### Group Tokens by Creator

```csharp
// Group token accounts by their creators
var groupRequest = new GroupTokensByCreatorRequest
{
    AccountAddresses = new List<string>
    {
        "Account1...",
        "Account2...",
        "Account3...",
        "Account4...",
        "Account5..."
    },
    Network = "devnet",
    IncludeZeroBalances = false,
    MinimumTokenCount = 2  // Only show creators with 2+ tokens
};

var groupResponse = await analyzer.GroupTokensByCreatorAsync(groupRequest);

if (groupResponse.Success)
{
    Console.WriteLine($"Found {groupResponse.Data.Count} creators");
    Console.WriteLine();

    foreach (var group in groupResponse.Data)
    {
        Console.WriteLine($"Creator: {group.CreatorAddress}");
        Console.WriteLine($"  Total Tokens: {group.TotalTokenCount}");
        Console.WriteLine($"  Total Value Held: {group.TotalValueHeld}");
        Console.WriteLine("  Tokens:");

        foreach (var token in group.Tokens)
        {
            Console.WriteLine($"    - {token.TokenMintAddress.Substring(0, 8)}...");
            Console.WriteLine($"      Account: {token.AccountAddress.Substring(0, 8)}...");
            Console.WriteLine($"      Balance: {token.Balance}");
        }
        Console.WriteLine();
    }
}
```

### Get Account Creator Info for Multiple Accounts

```csharp
// Get creator info for a list of accounts
var accounts = await analyzer.GetAccountCreatorInfoAsync(
    new List<string>
    {
        "Account1...",
        "Account2...",
        "Account3..."
    },
    "devnet");

foreach (var account in accounts)
{
    Console.WriteLine($"Account: {account.AccountAddress}");
    Console.WriteLine($"  Token: {account.TokenMintAddress}");
    Console.WriteLine($"  Balance: {account.Balance}");

    if (account.CreatorInfo != null)
    {
        Console.WriteLine($"  Creator: {account.CreatorInfo.MintAuthority}");
    }
}
```

## Use Cases

### 1. Portfolio Analysis - Identify Token Creators

```csharp
// Analyze a wallet's token portfolio to see who created each token
var analyzer = new TokenCreatorAnalyzer("https://localhost:5001");

var portfolioAccounts = new List<string>
{
    // User's token accounts from wallet
    "TokenAccount1...",
    "TokenAccount2...",
    "TokenAccount3..."
};

var response = await analyzer.GetTokenCreatorsAsync(new GetTokenCreatorsRequest
{
    AccountAddresses = portfolioAccounts,
    Network = "mainnet-beta",
    IncludeZeroBalances = false
});

if (response.Success)
{
    Console.WriteLine("Portfolio Creator Analysis:");
    foreach (var account in response.Data.Accounts)
    {
        var creator = account.CreatorInfo?.MintAuthority ?? "Unknown";
        Console.WriteLine($"{account.TokenMintAddress}: Created by {creator}");
    }
}
```

### 2. Detect Common Token Creators

```csharp
// Find if multiple tokens in a list were created by the same entity
var response = await analyzer.GroupTokensByCreatorAsync(new GroupTokensByCreatorRequest
{
    AccountAddresses = tokenAccountList,
    Network = "mainnet-beta",
    MinimumTokenCount = 2
});

foreach (var group in response.Data.Where(g => g.TotalTokenCount > 1))
{
    Console.WriteLine($"Creator {group.CreatorAddress} created {group.TotalTokenCount} tokens:");
    foreach (var token in group.Tokens)
    {
        Console.WriteLine($"  - {token.TokenMintAddress}");
    }
}
```

### 3. Verify Token Legitimacy

```csharp
// Check if a token's mint authority is renounced (set to null)
var creatorInfo = await analyzer.GetTokenCreatorInfoAsync(
    "SuspiciousTokenMint...",
    "mainnet-beta");

if (creatorInfo != null)
{
    if (creatorInfo.MintAuthority == null)
    {
        Console.WriteLine("Token mint authority is renounced - no one can mint more tokens");
    }
    else
    {
        Console.WriteLine($"Token can still be minted by: {creatorInfo.MintAuthority}");
        Console.WriteLine("WARNING: Creator can inflate supply!");
    }

    if (creatorInfo.FreezeAuthority == null)
    {
        Console.WriteLine("Freeze authority is renounced - tokens cannot be frozen");
    }
    else
    {
        Console.WriteLine($"WARNING: Tokens can be frozen by: {creatorInfo.FreezeAuthority}");
    }
}
```

### 4. Track Token Issuance by Creator

```csharp
// Monitor all tokens created by a specific address
var allAccounts = GetAllTokenAccounts(); // Your method to get accounts

var response = await analyzer.GetTokenCreatorsAsync(new GetTokenCreatorsRequest
{
    AccountAddresses = allAccounts,
    Network = "mainnet-beta"
});

var targetCreator = "TargetCreatorAddress...";
var tokensBy Creator = response.Data.Accounts
    .Where(a => a.CreatorInfo?.MintAuthority == targetCreator)
    .ToList();

Console.WriteLine($"Found {tokensByCreator.Count} tokens created by {targetCreator}");
```

### 5. Audit Token Ecosystem

```csharp
// Audit multiple token accounts to understand the ecosystem
var request = new GetTokenCreatorsRequest
{
    AccountAddresses = ecosystemTokenAccounts,
    Network = "mainnet-beta"
};

var response = await analyzer.GetTokenCreatorsAsync(request);

if (response.Success)
{
    var stats = response.Data.Statistics;

    Console.WriteLine("=== ECOSYSTEM AUDIT ===");
    Console.WriteLine($"Total Accounts: {stats.TotalAccountsAnalyzed}");
    Console.WriteLine($"Unique Tokens: {stats.TotalUniqueTokens}");
    Console.WriteLine($"Unique Creators: {stats.TotalUniqueCreators}");
    Console.WriteLine($"Accounts with No Authority: {stats.AccountsWithNoAuthority}");
    Console.WriteLine($"Accounts with Same Authority: {stats.AccountsWithSameAuthority}");
    Console.WriteLine($"Analysis Duration: {stats.AnalysisDuration.TotalSeconds:F2}s");
}
```

## Configuration Options

### GetTokenCreatorsRequest

```csharp
var request = new GetTokenCreatorsRequest
{
    // List of token account addresses to analyze
    AccountAddresses = new List<string> { "account1", "account2" },

    // Solana network: devnet, testnet, mainnet-beta, or localhost
    Network = "mainnet-beta",

    // Include accounts with zero balance in results
    IncludeZeroBalances = false,

    // Fetch supply information for each token mint
    FetchSupplyInfo = true
};
```

### GroupTokensByCreatorRequest

```csharp
var request = new GroupTokensByCreatorRequest
{
    // List of token account addresses
    AccountAddresses = new List<string> { "account1", "account2" },

    // Solana network
    Network = "mainnet-beta",

    // Include zero-balance accounts
    IncludeZeroBalances = false,

    // Minimum number of tokens a creator must have to be included
    MinimumTokenCount = 1
};
```

## Response Structure

### GetTokenCreatorsResponse

```json
{
  "success": true,
  "message": "Successfully analyzed 5 accounts, found 3 unique creators",
  "data": {
    "accounts": [
      {
        "accountAddress": "Account1Address...",
        "tokenMintAddress": "TokenMint1...",
        "owner": "OwnerAddress...",
        "balance": 100.5,
        "creatorInfo": {
          "tokenMintAddress": "TokenMint1...",
          "mintAuthority": "CreatorAddress1...",
          "freezeAuthority": null,
          "decimals": 9,
          "supply": 1000000,
          "isInitialized": true,
          "retrievedAt": "2025-10-15T12:00:00Z"
        }
      }
    ],
    "uniqueCreators": {
      "CreatorAddress1...": {
        "tokenMintAddress": "TokenMint1...",
        "mintAuthority": "CreatorAddress1...",
        "supply": 1000000
      }
    },
    "analyzedAt": "2025-10-15T12:00:00Z",
    "statistics": {
      "totalAccountsAnalyzed": 5,
      "totalUniqueTokens": 4,
      "totalUniqueCreators": 3,
      "accountsWithNoAuthority": 1,
      "accountsWithSameAuthority": 2,
      "analysisDuration": "00:00:03.1234567"
    }
  },
  "errors": [],
  "warnings": []
}
```

### GroupTokensByCreatorResponse

```json
{
  "success": true,
  "message": "Found 2 creators",
  "data": [
    {
      "creatorAddress": "Creator1Address...",
      "tokens": [
        {
          "accountAddress": "Account1...",
          "tokenMintAddress": "Mint1...",
          "balance": 100
        },
        {
          "accountAddress": "Account2...",
          "tokenMintAddress": "Mint2...",
          "balance": 200
        }
      ],
      "totalTokenCount": 2,
      "totalValueHeld": 300
    }
  ],
  "errors": [],
  "warnings": []
}
```

## API Integration

The analyzer uses The16Oracles.DAOA endpoint:

- `POST /api/spl-token/display` - Display token mint or account details

Ensure DAOA API is running:

```bash
cd The16Oracles.DAOA
dotnet run
```

Access Swagger UI: `https://localhost:5001/swagger`

## Testing

### Run Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~TokenCreatorAnalyzerTests"
```

### Test Coverage

25 comprehensive tests covering:
- Model initialization and validation
- Request/response structures
- Token creator information retrieval
- Account creator linking
- Grouping logic
- Filtering operations
- Statistics calculations
- Edge cases

**All 25 tests passed successfully!**

## Important Notes

### Null Mint Authority

When `MintAuthority` is `null`, it means:
- The mint authority has been **renounced**
- No one can mint additional tokens
- The supply is **fixed** and cannot increase
- This is common for decentralized/community tokens

### Null Freeze Authority

When `FreezeAuthority` is `null`, it means:
- The freeze authority has been **renounced**
- Token accounts cannot be frozen
- Users have full control of their tokens
- This is a sign of a more decentralized token

### Account vs Mint Address

- **Account Address**: A token account that holds a specific amount of tokens
- **Mint Address**: The token definition itself (the "blueprint" for the token)
- One mint can have many token accounts
- The creator is associated with the mint, not the account

## Performance Considerations

- **Processing Time**: ~0.5-1 second per account (depends on network)
- **Batch Size**: Recommended 10-50 accounts per request
- **Network Latency**: Mainnet queries are slower than devnet
- **Concurrent Requests**: Service uses single HttpClient for efficiency

## Error Handling

The service handles errors gracefully:
- Invalid account addresses are skipped with warnings
- Network errors are caught and logged
- Failed accounts don't stop the entire analysis
- Detailed error messages in response

## Limitations

1. **Requires DAOA Running**: The DAOA API must be active
2. **Network Dependent**: Requires Solana RPC access
3. **Account Validity**: Only works with valid token accounts
4. **No Historical Data**: Shows current state only

## Future Enhancements

- Cache token creator information for faster repeat queries
- Support for batch account retrieval
- Historical creator change tracking
- Integration with token metadata services
- Creator reputation scoring
- Token family tree visualization

## Source Code

- **Service**: `The16Oracles.domain/Services/TokenCreatorAnalyzer.cs`
- **Models**: `The16Oracles.domain/Models/TokenCreator.cs`
- **Tests**: `The16Oracles.domain.nunit/Services/TokenCreatorAnalyzerTests.cs`

## Related Documentation

- **SPL-TOKEN-API.md** - SPL Token API reference
- **WALLET_RELATIONSHIP_ANALYZER.md** - Wallet relationship detection
- **TOKEN_SUPPLY_ANALYZER.md** - Token supply concentration analysis

## License & Credits

© 2025 The16Oracles Project
Part of The 16 Oracles comprehensive crypto analytics platform.

---

**Quick Start Example:**

```csharp
var analyzer = new TokenCreatorAnalyzer("https://localhost:5001");

var response = await analyzer.GetTokenCreatorsAsync(new GetTokenCreatorsRequest
{
    AccountAddresses = new List<string> { "YourTokenAccount..." },
    Network = "mainnet-beta"
});

if (response.Success)
{
    foreach (var account in response.Data.Accounts)
    {
        Console.WriteLine($"Token: {account.TokenMintAddress}");
        Console.WriteLine($"Creator: {account.CreatorInfo?.MintAuthority ?? "Renounced"}");
    }
}
```
