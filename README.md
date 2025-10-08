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

  Additional Features

- Discord Bot Integration: AI-powered NFT community bot with OpenAI integration for welcome messages, NFT showcases, and war game mechanics (documented in
  DiscordBot.md)
- Architecture: Clean separation with domain logic, console runner, and web API layers
- Copyright: © 2025 Jerrame Hertz, All Rights Reserved

  This is a comprehensive crypto analytics and community engagement platform combining oracle data feeds with Discord community features.
