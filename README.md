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
    - **Oracle Wars Discord Game**:
      - Complete PvP strategy game integrated with Discord bots
      - 9 RESTful game API endpoints under /api/game/
      - Player management with SOL balance and win/loss tracking
      - 14 playable oracles with varying costs (10-50 SOL) and power levels (2-8)
      - Battle system using live oracle data for score calculations
      - Global leaderboard and ranking system
      - Daily bonus rewards (2 SOL per subscribed oracle)
      - In-memory game state with ConcurrentDictionary storage
      - Documented in ORACLE_WARS_GAME.md
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
    - **Oracle Wars Discord Bot Integration**:
      - OracleWarsApiService: HTTP client for DAOA game API communication
      - 8 slash commands (/ow-*) for modern Discord UI
      - 9 prefix commands (!ow-*) for traditional command usage
      - Rich Discord embeds with colors, fields, and formatting
      - Commands: register, profile, oracles, subscribe, battle, leaderboard, daily bonus
      - Auto-executed battles with narrative generation
      - User-friendly error handling and validation
      - Documented in DISCORD_ORACLE_WARS.md
    - **Wallet Relationship Analyzer**:
      - Pattern matching analysis to identify relationships between Solana wallets
      - 6 detection algorithms: shared tokens, balance similarity, SOL similarity, temporal proximity, activity correlation, cluster identification
      - Detects relationship types: LikelyRelated, PossiblyRelated, SharedTokens, MirrorTrading, SuspiciousActivity
      - Use cases: fraud detection, airdrop sybil prevention, whale tracking, governance vote analysis
      - Integrates with DAOA Solana/SPL Token endpoints for wallet data
      - Comprehensive test coverage (19 unit tests)
      - Documented in WALLET_RELATIONSHIP_ANALYZER.md
    - **Token Supply Concentration Analyzer**:
      - Comprehensive token supply analysis and suspicious holder detection for Solana SPL tokens
      - Supply concentration metrics: Gini coefficient, Herfindahl-Hirschman Index, top holder percentages
      - 6-level risk assessment (VeryLow to Critical) with composite concentration scores
      - 9 suspicious holder detection flags: NewWallet, LowActivity, LargeConcentration, RelatedToOtherHolders, etc.
      - Cluster identification for related suspicious holders using graph traversal
      - Use cases: rug pull detection, fair launch verification, sybil attack detection, token health monitoring
      - Integrates with TokenSupplyAnalyzer and WalletRelationshipAnalyzer services
      - 30 comprehensive unit tests (all passed)
      - Documented in TOKEN_SUPPLY_ANALYZER.md
    - **Token Creator Analyzer**:
      - Retrieve token creator addresses (mint authorities) from token account lists
      - Get mint authority, freeze authority, supply, and decimals for any token
      - Group tokens by creator to identify common issuers
      - Verify token legitimacy by checking if mint/freeze authorities are renounced
      - Use cases: portfolio analysis, common creator detection, token legitimacy verification, ecosystem auditing
      - Integrates with DAOA SPL Token API endpoints
      - 25 comprehensive unit tests (all passed)
      - Documented in TOKEN_CREATOR_ANALYZER.md
  3. The16Oracles.console
    - Console application for testing and running Discord bots
    - Uses dependency injection to manage bot configurations
    - Loads settings from config.json for multiple Discord bot instances
    - Automatically registers Oracle Wars commands (slash and prefix)
    - Supports OracleWarsApiUrl configuration for API endpoint
    - Example configuration provided in config.example.json
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
    - Comprehensive test coverage (98+ tests):
      - WalletRelationshipAnalyzer: 19 tests
      - TokenSupplyAnalyzer: 30 tests
      - TokenCreatorAnalyzer: 25 tests
      - Original domain tests: 24 tests
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

- **Oracle Wars Game**: Complete Discord bot game where players battle using oracle subscriptions
  - 17 Discord commands (8 slash + 9 prefix) for gameplay
  - Player profiles with SOL balance and statistics
  - 14 unique oracles across 5 categories (Market Analysis, Risk Detection, Opportunities, Technical Metrics, Trends)
  - PvP battles with live oracle data scoring
  - Global leaderboard and daily reward system
  - Quick start guide in QUICK_START_ORACLE_WARS.md
  - Complete documentation: ORACLE_WARS_GAME.md, DISCORD_ORACLE_WARS.md, ORACLE_WARS_SUMMARY.md
- **Automated Trading**: Multi-stablecoin cascade trading bot on Solana with Jupiter Aggregator integration (documented in The16Oracles.www.Server/TRADEBOT_README.md)
- **Advanced Wallet & Token Analysis Suite**: Three powerful analyzers for Solana blockchain intelligence
  - **Wallet Relationship Analyzer**: Pattern matching to identify relationships between wallets
    - 6 detection algorithms analyzing shared tokens, balances, temporal patterns, and activity
    - Identifies fraud, sybil attacks, whale clusters, and coordinated governance voting
    - 19 unit tests, documented in WALLET_RELATIONSHIP_ANALYZER.md
  - **Token Supply Concentration Analyzer**: Detect supply concentration and suspicious holders
    - Gini coefficient, Herfindahl-Hirschman Index, 6-level risk assessment (VeryLow to Critical)
    - 9 suspicious holder detection flags with cluster identification
    - Use cases: rug pull detection, fair launch verification, sybil attack detection
    - 30 unit tests, documented in TOKEN_SUPPLY_ANALYZER.md
  - **Token Creator Analyzer**: Retrieve and analyze token creator addresses (mint authorities)
    - Get mint/freeze authorities, verify if renounced, group tokens by creator
    - Use cases: portfolio analysis, legitimacy verification, ecosystem auditing
    - 25 unit tests, documented in TOKEN_CREATOR_ANALYZER.md
  - All analyzers integrate with DAOA endpoints for real-time blockchain data
- **Solana Blockchain Integration**:
  - Solana CLI Web API: 25+ endpoints for blockchain operations (documented in SolanaAPI.md)
  - SPL Token CLI Web API: 23+ endpoints for token operations (documented in SPL-TOKEN-API.md)
  - Full support for account management, token operations, delegation, freeze/thaw, and native SOL wrapping
- **Discord Bot Integration**:
  - AI-powered NFT community bot with OpenAI integration for welcome messages, NFT showcases, and war game mechanics (documented in DiscordBot.md)
  - Oracle Wars game integration with DSharpPlus 4.5.1 for interactive gameplay
- **Architecture**: Clean separation with domain logic, console runner, web API layers, automated trading services, Discord game integration, and comprehensive blockchain integration
- **Testing**: Comprehensive unit test coverage across all projects with 280+ total tests:
  - The16Oracles.domain.nunit: 98 tests
    - WalletRelationshipAnalyzer: 19 tests
    - TokenSupplyAnalyzer: 30 tests
    - TokenCreatorAnalyzer: 25 tests
    - Original domain tests: 24 tests
  - The16Oracles.DAOA.nunit: 127 tests (81 Solana CLI, 46 SPL Token CLI)
  - The16Oracles.www.Server.nunit: 39 tests (trading bot)
  - Additional: 16+ oracle and model tests
  - Oracle Wars game: Manual testing via Discord and Swagger UI
  - All analyzer tests pass with 100% success rate
- **Copyright**: © 2025 Jerrame Hertz, All Rights Reserved

  This is a comprehensive crypto analytics, automated trading, blockchain operations, Discord gaming, and advanced token analysis platform combining oracle data feeds, full Solana and SPL Token CLI integration, intelligent trading bots, interactive Discord games, wallet relationship detection, supply concentration analysis, token creator verification, and community features.

  Quick Start Guides

  - **Oracle Wars Game**: See QUICK_START_ORACLE_WARS.md for 5-minute setup
  - **Trading Bot**: See The16Oracles.www.Server/TRADEBOT_README.md for configuration
  - **Solana API**: See SolanaAPI.md and SPL-TOKEN-API.md for endpoint documentation
  - **Discord Bot**: See DiscordBot.md for community bot setup
  - **Analysis Suite**:
    - **Wallet Relationships**: WALLET_RELATIONSHIP_ANALYZER.md - Detect wallet relationships and clusters
    - **Supply Concentration**: TOKEN_SUPPLY_ANALYZER.md - Analyze token distribution and detect suspicious holders
    - **Token Creators**: TOKEN_CREATOR_ANALYZER.md - Get creator addresses and verify token legitimacy
