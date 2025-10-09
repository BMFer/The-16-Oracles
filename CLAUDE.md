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

- **The16Oracles.DAOA** - ASP.NET Core Web API exposing 16 crypto oracle endpoints
- **The16Oracles.domain** - Shared domain layer with models and services (Discord bot functionality)
- **The16Oracles.console** - Console application for running Discord bots (uses dependency injection)
- **The16Oracles.domain.nunit** - NUnit test project for domain layer

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

### Discord Bot Architecture (Console + Domain)

The console application loads bot configurations from `config.json` which can contain multiple Discord bot instances. Uses Microsoft.Extensions.DependencyInjection for service registration.

**Key Services**:
- `BotService` - Manages Discord bot instances (from domain layer)
- `DataModel` - Data access layer (from domain layer)
- `DiscordBot` - Individual bot instance with DSharpPlus integration

**Configuration Pattern**: Load `config.json` → deserialize to `Config` model → extract specific Discord config by ID → create bot instance via DI

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
      "WelcomeChannelId": 123456789,
      "AssetsChannelId": 123456789
    }
  ]
}
```

See `DiscordBot.md` for comprehensive Discord bot setup and features.

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

See `DiscordBot.md` for detailed documentation on bot setup, commands, and customization.

## Important Notes

### Mixed Target Frameworks
The test project targets .NET 9.0 while other projects target .NET 8.0. This is intentional to use latest test framework features and C# language capabilities in tests.

### Oracle Independence
Each oracle operates independently - they don't share state or communicate with each other. This allows parallel execution and independent failure handling.

### Configuration-Driven Design
Both the DAOA API (via appsettings.json) and Discord bots (via config.json) are configuration-driven. No hardcoded tokens or API keys should be committed to the repository.

### External Dependencies
Most oracles require real-time external API access. If APIs are unavailable or rate-limited, oracles will throw exceptions that need to be handled by the calling code.
