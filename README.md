# The-16-Oracles

● Project Summary

  The 16 Oracles is a multi-faceted .NET solution focused on cryptocurrency oracle systems with the following components:

  Core Projects

  1. The16Oracles.DAOA (ASP.NET Core Web API)
    - Decentralized Autonomous Oracle Application exposing 16 specialized crypto oracles via REST API
    - Each oracle analyzes specific aspects of crypto markets and blockchain data:
      - Market Analysis: Macro trends, DeFi liquidity, whale behavior, NFT sentiment
      - Risk Detection: Black swan events, rug pull detection, regulatory risks
      - Opportunities: Airdrop launches, emerging market surges
      - Technical Metrics: L2 activity, chain interoperability, node validator profits
      - Trends: AI narratives, tokenomics, tech adoption curves, stablecoin flows
    - **Solana CLI Web API Wrapper**:
      - 25+ RESTful endpoints wrapping Solana CLI commands
      - Account & balance management (balance, address, transfer)
      - Airdrop and faucet operations
      - Transaction management and confirmation
      - Block, slot, and epoch information
      - Cluster information (version, supply, inflation, validators)
      - Stake and vote account management
      - Support for all Solana networks (mainnet, devnet, testnet, localhost)
      - Generic command execution for advanced usage
      - Documented in SolanaAPI.md
    - **SPL Token CLI Web API Wrapper**:
      - 23+ RESTful endpoints wrapping SPL Token CLI commands
      - Account management (accounts, address, balance, create-account, close, gc)
      - Token operations (create-token, mint, burn, transfer, supply, close-mint)
      - Token delegation (approve, revoke)
      - Token authority management (authorize)
      - Freeze/thaw operations
      - Native SOL wrapping/unwrapping
      - Display token information
      - Support for Token-2022 program extensions
      - Generic command execution for advanced usage
      - Documented in SPL-TOKEN-API.md
    - Built with .NET minimal APIs and Swagger/OpenAPI documentation
  2. The16Oracles.domain
    - Domain layer containing shared models, services, and business logic
    - Includes Discord bot services and data models
  3. The16Oracles.console
    - Console application for testing and running Discord bots
    - Uses dependency injection to manage bot configurations
    - Loads settings from config.json for multiple Discord bot instances
  4. The16Oracles.www.Server (ASP.NET Core 8.0 Backend)
    - Backend API server for the web application
    - Built with ASP.NET Core 8.0 with SPA proxy support
    - Integrates with Angular client application
    - Includes Swashbuckle for API documentation
    - **Solana Trading Bot System**:
      - Multi-stablecoin cascade trading strategy
      - Automated trading across configurable stablecoin pairs (USDC, USDT, etc.)
      - Profitability ranking system for optimal trade execution order
      - Cascade execution: results from top-ranked bot feed into next bot
      - Jupiter Aggregator v6 integration for best swap rates
      - Comprehensive risk management (trade limits, daily caps, price impact checks)
      - Real-time balance and status monitoring
      - RESTful API for trade execution and pair management
  5. the16oracles.www.client (Angular 17 Frontend)
    - Modern Angular 17 single-page application
    - Client-side web interface for The 16 Oracles platform
    - Features TypeScript, RxJS, and Jasmine testing framework
    - Integrated with ASP.NET Core backend via SPA proxy
    - Configured with HTTPS and cross-platform development support
  6. The16Oracles.domain.nunit
    - NUnit test project for domain layer testing
    - Built with .NET 9.0 and NUnit 4.2.2
    - Includes code coverage collection via coverlet
    - Ensures reliability and quality of domain logic and services
  7. The16Oracles.DAOA.nunit
    - NUnit test project for DAOA Web API testing
    - Built with .NET 9.0 and NUnit 4.2.2
    - Comprehensive test coverage (127+ tests):
      - Oracle implementations and API endpoints
      - Solana CLI service (81 tests)
      - SPL Token CLI service (46 tests)
    - Tests all blockchain integration endpoints
    - Includes code coverage collection via coverlet
  8. The16Oracles.www.Server.nunit
    - NUnit test project for trading bot and web server testing
    - Built with .NET 8.0, NUnit 3.14, and Moq 4.20
    - Comprehensive test coverage (40+ tests) for:
      - TradingBotController endpoints (16 tests)
      - TradingBotOrchestrator service (15 tests)
      - ProfitabilityAnalyzer service (13 tests)
    - Tests cascade trading logic, risk management, and API endpoints
    - Includes code coverage collection via coverlet

  Additional Features

- **Automated Trading**: Multi-stablecoin cascade trading bot on Solana with Jupiter Aggregator integration (documented in The16Oracles.www.Server/TRADEBOT_README.md)
- **Solana Blockchain Integration**:
  - Solana CLI Web API: 25+ endpoints for blockchain operations (documented in SolanaAPI.md)
  - SPL Token CLI Web API: 23+ endpoints for token operations (documented in SPL-TOKEN-API.md)
  - Full support for account management, token operations, delegation, freeze/thaw, and native SOL wrapping
- **Discord Bot Integration**: AI-powered NFT community bot with OpenAI integration for welcome messages, NFT showcases, and war game mechanics (documented in DiscordBot.md)
- **Architecture**: Clean separation with domain logic, console runner, web API layers, automated trading services, and comprehensive blockchain integration
- **Testing**: Comprehensive unit test coverage across all projects with 206+ total tests:
  - The16Oracles.domain.nunit: 24 tests
  - The16Oracles.DAOA.nunit: 127 tests (81 Solana CLI, 46 SPL Token CLI)
  - The16Oracles.www.Server.nunit: 39 tests (trading bot)
  - Additional: 16+ oracle and model tests
- **Copyright**: © 2025 Jerrame Hertz, All Rights Reserved

  This is a comprehensive crypto analytics, automated trading, blockchain operations, and community engagement platform combining oracle data feeds, full Solana and SPL Token CLI integration, intelligent trading bots, and Discord community features.
