# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

### Build the solution
```bash
dotnet build The16Oracles.sln
```

### Run the Web API (DAOA)
```bash
dotnet run --project The16Oracles.DAOA/The16Oracles.DAOA.csproj
```
Access Swagger UI at: `https://localhost:5001/swagger` (or http://localhost:5000/swagger)

### Run the Discord Bot Console
```bash
dotnet run --project The16Oracles.console/The16Oracles.console.csproj
```
Requires `config.json` in the console project directory with Discord bot tokens and configuration.

### Run all tests
```bash
dotnet test
```

### Run specific test project
```bash
dotnet test The16Oracles.domain.nunit/The16Oracles.domain.nunit.csproj
```

### Run tests with coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run a single test
```bash
dotnet test --filter "FullyQualifiedName~The16Oracles.domain.nunit.Models.ConfigTests"
```

## Project Architecture

### Solution Structure
This is a multi-project .NET solution with clean architecture:

- **The16Oracles.DAOA** - ASP.NET Core Web API exposing 16 crypto oracle endpoints + Solana CLI wrapper + Oracle Wars game API
- **The16Oracles.domain** - Shared domain layer with models and services (Discord bot functionality + Oracle Wars commands)
- **The16Oracles.console** - Console application for running Discord bots with Oracle Wars game integration (uses dependency injection)
- **The16Oracles.domain.nunit** - NUnit test project for domain layer
- **The16Oracles.DAOA.nunit** - NUnit test project for DAOA Web API
- **The16Oracles.www.Server** - ASP.NET Core backend with Solana trading bot system
- **The16Oracles.www.Server.nunit** - NUnit test project for trading bot system
- **the16oracles.www.client** - Angular 17 frontend application

### Target Frameworks
- DAOA, domain, console: **.NET 8.0**
- domain.nunit: **.NET 9.0** (uses latest C# features)

### Oracle Pattern (DAOA Project)

All 16 oracles implement the `IAIOracle` interface:

```csharp
public interface IAIOracle
{
    string Name { get; }
    Task<OracleResult> EvaluateAsync(DataBundle bundle);
}
```

**Oracle Registration Pattern** (see `The16Oracles.DAOA/Program.cs`):
1. Register HttpClient for the oracle: `builder.Services.AddHttpClient<OracleName>();`
2. Register oracle as singleton: `builder.Services.AddSingleton<IAIOracle, OracleName>();`
3. Create minimal API endpoint: `app.MapGet("/api/oracles/{name}", async (OracleName oracle) => ...)`

Each oracle is responsible for:
- Fetching external data from crypto/blockchain APIs via injected HttpClient
- Processing data into metrics
- Returning standardized `OracleResult` with confidence score, metrics, and timestamp

**The 16 Oracles** (by category):
- **Market Analysis**: MacroEconomicTrends, DeFiLiquidityMovements, CryptoWhaleBehavior, NFTMarketSentiment
- **Risk Detection**: BlackSwanDetection, SecurityRugRiskDetection, RegulatoryRiskForecasting
- **Opportunities**: AirdropLaunchOpportunities, EmergingMarketCapitalSurge
- **Technical Metrics**: L2ActivityMonitoring, ChainInteroperabilityMetrics, NodeValidatorProfits
- **Trends**: AiNarrativeTrendDetection, TokenomicsSupplyCurves, TechAdoptionCurves, StablecoinFlowTracking

### Solana CLI API Wrapper (DAOA Project)

The DAOA also includes a RESTful wrapper for Solana CLI commands:

**Service Pattern** (see `The16Oracles.DAOA/Services/SolanaService.cs`):
1. Register service: `builder.Services.AddSingleton<ISolanaService, SolanaService>();`
2. Create minimal API endpoints for each Solana command
3. Execute CLI commands via Process and return standardized responses

**Key Features**:
- 25+ endpoints covering all major Solana CLI operations
- Support for all networks (mainnet-beta, devnet, testnet, localhost)
- Standardized request/response models with global flags support
- Process execution with output/error handling
- JSON parsing for structured data responses

**Endpoint Categories**:
- Account & Balance Management (balance, address, transfer)
- Airdrop & Faucet operations
- Transaction management (history, confirmation, count)
- Block & Slot information
- Epoch information
- Cluster information (version, supply, inflation, validators)
- Stake account management
- Vote account management
- Generic command execution endpoint

**Solana Endpoints**: All under `/api/solana/` with tag "Solana CLI":
- `/api/solana/balance` - Get account balance
- `/api/solana/transfer` - Transfer SOL
- `/api/solana/airdrop` - Request airdrop
- `/api/solana/epoch-info` - Get epoch information
- `/api/solana/validators` - Get validator information
- etc. (See `SolanaAPI.md` for complete documentation)

### Oracle Wars Game API (DAOA Project)

The DAOA also includes a complete Discord bot game called **Oracle Wars**:

**Service Pattern** (see `The16Oracles.DAOA/Services/OracleGameService.cs`):
1. Register service: `builder.Services.AddSingleton<IOracleGameService, OracleGameService>();`
2. Create minimal API endpoints for game operations
3. Integrate with existing oracles for battle calculations

**Key Features**:
- 9 game API endpoints under `/api/game/`
- Player management with SOL balance and statistics
- 14 playable oracles with varying costs (10-50 SOL) and power levels (2-8)
- Battle system using live oracle confidence scores
- Global leaderboard and ranking system
- Daily bonus rewards (2 SOL per subscribed oracle)
- In-memory game state using ConcurrentDictionary

**Game Endpoints**: All under `/api/game/` with tag "Oracle Wars Game":
- `/api/game/player/create` - Register new player (POST)
- `/api/game/player/{userId}` - Get player profile (GET)
- `/api/game/oracle/subscribe` - Subscribe to oracle (POST)
- `/api/game/oracle/unsubscribe` - Unsubscribe from oracle (DELETE)
- `/api/game/oracles` - List all oracles (GET)
- `/api/game/battle/create` - Create battle (POST)
- `/api/game/battle/{id}/execute` - Execute battle (POST)
- `/api/game/leaderboard` - Get rankings (GET)
- `/api/game/daily-bonus/{userId}` - Claim daily bonus (POST)

**Battle Calculation**:
```csharp
Player Score = Σ(Oracle Confidence Score × Oracle Power Level)
Winner = Player with higher total score (or random if tie)
```

See `ORACLE_WARS_GAME.md` for complete API documentation.

### Discord Bot Architecture (Console + Domain)

The console application loads bot configurations from `config.json` which can contain multiple Discord bot instances. Uses Microsoft.Extensions.DependencyInjection for service registration.

**Key Services**:
- `BotService` - Manages Discord bot instances (from domain layer)
- `DataModel` - Data access layer (from domain layer)
- `DiscordBot` - Individual bot instance with DSharpPlus integration
- `OracleWarsApiService` - HTTP client for Oracle Wars game API communication

**Configuration Pattern**: Load `config.json` → deserialize to `Config` model → extract specific Discord config by ID → create bot instance via DI

**Oracle Wars Discord Integration**:
The Discord bot automatically registers Oracle Wars commands when initialized:
- 8 slash commands (`/ow-*`) - Modern Discord UI with autocomplete
- 9 prefix commands (`!ow-*`) - Traditional command-line style
- Commands: register, profile, oracles, subscribe, battle, leaderboard, daily bonus, help
- Rich Discord embeds with colors, fields, and user mentions
- Auto-executed battles with narrative generation
- Error handling with user-friendly messages

See `DISCORD_ORACLE_WARS.md` for complete Discord bot integration documentation.

## Configuration Requirements

### DAOA API Keys (appsettings.json)

The oracles require various third-party API keys configured in `The16Oracles.DAOA/appsettings.json`:

- **FRED:ApiKey** - Federal Reserve Economic Data (MacroEconomicTrendsOracle)
- **NFTMarketSentiment:OpenSeaApiKey** - OpenSea API (NFTMarketSentimentOracle)
- **Etherscan:ApiKey** - Ethereum blockchain data
- **WhaleBehavior:ApiKey** - Whale Alert API
- **NarrativeTrend:NewsApiKey** - News API
- **NarrativeTrend:TwitterBearerToken** - Twitter/X API
- **L2Activity:Arbiscan:ApiKey** - Arbitrum chain data
- **L2Activity:OptimisticEtherscan:ApiKey** - Optimism chain data
- **RugRisk:Covalent:ApiKey** - Covalent blockchain API

Replace placeholder values like `<YOUR_FRED_API_KEY>` with actual API keys before running.

### Discord Bot Configuration (config.json)

The console project requires `config.json` in the project directory:

```json
{
  "Discords": [
    {
      "Id": 1,
      "Name": "Bot Name",
      "Token": "YOUR_DISCORD_BOT_TOKEN",
      "CommandPrefix": "!",
      "WelcomeChannelId": 123456789,
      "AssetsChannelId": 123456789,
      "OracleWarsApiUrl": "https://localhost:5001"
    }
  ]
}
```

**New Fields for Oracle Wars**:
- `CommandPrefix`: Prefix for traditional commands (e.g., "!" for `!ow-register`)
- `OracleWarsApiUrl`: URL of the DAOA API for game functionality

See `DiscordBot.md` for comprehensive Discord bot setup and features.
See `QUICK_START_ORACLE_WARS.md` for 5-minute Oracle Wars setup guide.

## API Design Patterns

### Minimal APIs
The DAOA uses ASP.NET Core minimal APIs (not controllers). All oracle endpoints follow this pattern:

```csharp
app.MapGet("/api/oracles/{oracle-name}", async (OracleClassName oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetOracle-name")
.WithTags("DAOA Oracles")
.WithOpenApi();
```

### Oracle Endpoint URLs
All oracle endpoints are under `/api/oracles/` with kebab-case naming:
- `/api/oracles/macro-trends`
- `/api/oracles/defi-liquidity`
- `/api/oracles/whale-behavior`
- `/api/oracles/nft-sentiment`
- etc.

### Solana API Endpoints
All Solana CLI wrapper endpoints are under `/api/solana/`:
- GET endpoints for simple queries (e.g., `/api/solana/cluster-version?url=devnet`)
- POST endpoints for operations requiring parameters (e.g., `/api/solana/balance`, `/api/solana/transfer`)
- Generic execution endpoint: `/api/solana/execute` for advanced custom commands

### Oracle Wars Game Endpoints
All game endpoints are under `/api/game/` with Swagger tag "Oracle Wars Game":
- Player management: `/api/game/player/*`
- Oracle operations: `/api/game/oracle/*`
- Battle system: `/api/game/battle/*`
- Leaderboard: `/api/game/leaderboard`
- Daily rewards: `/api/game/daily-bonus/{userId}`

### DataBundle Pattern
Most oracles currently accept an empty `DataBundle` object. This is designed for future expansion where oracles might receive contextual data or parameters.

## Testing Conventions

### Test Structure
Tests mirror the domain structure:
- `The16Oracles.domain.nunit/Models/` - tests for domain models
- `The16Oracles.domain.nunit/Services/` - tests for domain services

### NUnit Configuration
- Uses NUnit 4.2.2 with latest analyzers
- Code coverage via coverlet.collector
- Global using for `NUnit.Framework` (configured in .csproj)

## Discord Bot Features

The domain layer includes a sophisticated Discord bot system with:
- **AI-powered welcome messages** using OpenAI integration
- **NFT showcase functionality** pulling from Discord asset channels
- **War game mechanics** with AI-generated battle narratives
- **Traditional prefix commands** and **slash commands**
- **DSharpPlus 4.5.1** with CommandsNext, Interactivity, and SlashCommands modules
- **Oracle Wars Game Integration**:
  - 17 Discord commands (8 slash + 9 prefix) for gameplay
  - Rich embeds with battle results and leaderboards
  - Real-time communication with DAOA game API
  - Player profiles, oracle subscriptions, and PvP battles
  - Daily rewards and global rankings

See `DiscordBot.md` for detailed documentation on bot setup, commands, and customization.
See `DISCORD_ORACLE_WARS.md` for Oracle Wars Discord integration guide.

## Important Notes

### Mixed Target Frameworks
The test project targets .NET 9.0 while other projects target .NET 8.0. This is intentional to use latest test framework features and C# language capabilities in tests.

### Oracle Independence
Each oracle operates independently - they don't share state or communicate with each other. This allows parallel execution and independent failure handling.

### Configuration-Driven Design
Both the DAOA API (via appsettings.json) and Discord bots (via config.json) are configuration-driven. No hardcoded tokens or API keys should be committed to the repository.

### External Dependencies
- Most oracles require real-time external API access. If APIs are unavailable or rate-limited, oracles will throw exceptions that need to be handled by the calling code.
- The Solana CLI wrapper requires **Solana CLI to be installed** on the server where DAOA runs. Install from: https://docs.solana.com/cli/install-solana-cli-tools

## Related Documentation

### Oracle and API Documentation
- **SolanaAPI.md** - Complete documentation for the Solana CLI Web API wrapper with request/response examples
- **SPL-TOKEN-API.md** - Complete documentation for the SPL Token CLI Web API wrapper
- **README.md** - Project overview and solution structure

### Discord Bot Documentation
- **DiscordBot.md** - Comprehensive guide for Discord bot setup, commands, and customization

### Oracle Wars Game Documentation
- **QUICK_START_ORACLE_WARS.md** - 5-minute setup guide for Oracle Wars game
- **ORACLE_WARS_GAME.md** - Complete API reference for Oracle Wars game endpoints
- **DISCORD_ORACLE_WARS.md** - Discord bot integration guide for Oracle Wars
- **ORACLE_WARS_SUMMARY.md** - Complete summary of Oracle Wars implementation

### Trading Bot Documentation
- **TRADEBOT_README.md** - Documentation for the Solana trading bot system (in The16Oracles.www.Server/)
