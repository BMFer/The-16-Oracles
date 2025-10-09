# Solana Trading Bot - Setup & Usage Guide

## Overview

This trading bot executes swaps between SOL and a custom SPL token using Jupiter Aggregator v6. The bot is built with proper security, risk management, and defensive architecture.

## Architecture

### Services

1. **JupiterApiService** - Interfaces with Jupiter Aggregator v6 API
   - Gets swap quotes
   - Retrieves swap transactions

2. **SolanaTransactionService** - Handles Solana blockchain operations
   - Executes transactions
   - Queries balances (SOL and tokens)
   - Manages wallet operations

3. **RiskManagementService** - Enforces trading limits
   - Per-trade notional limits
   - Daily volume limits
   - Automatic daily counter resets

4. **TradingBotService** - Main orchestrator
   - Coordinates all services
   - Executes complete trade flow
   - Provides status information

### API Endpoints

- `GET /api/TradingBot/status` - Get bot status, balances, and daily volume
- `POST /api/TradingBot/trade` - Execute a trade (direction + amount)
- `POST /api/TradingBot/swap/sol-to-token` - Convenience endpoint for SOL → Token
- `POST /api/TradingBot/swap/token-to-sol` - Convenience endpoint for Token → SOL

## Configuration

### Required Settings

Edit `appsettings.json` or use User Secrets:

```json
{
  "TradeBot": {
    "Solana": {
      "RpcUrl": "https://api.mainnet-beta.solana.com",
      "BotPrivateKey": "YOUR_BASE58_PRIVATE_KEY_HERE",
      "TokenMint": "YOUR_TOKEN_MINT_ADDRESS",
      "SolMint": "So11111111111111111111111111111111111111112"
    },
    "RiskManagement": {
      "MaxTradeNotionalSol": 50,
      "MaxDailyNotionalSol": 500,
      "SlippageBps": 30,
      "MinBalanceSol": 0.1
    },
    "Trading": {
      "JupiterApiUrl": "https://quote-api.jup.ag/v6",
      "MaxRetries": 3,
      "TimeoutSeconds": 30,
      "Enabled": false
    }
  }
}
```

### Security Best Practices

**NEVER commit private keys to source control!**

Use one of these methods:

#### Option 1: User Secrets (Recommended for Development)

```bash
cd The16Oracles.www.Server
dotnet user-secrets init
dotnet user-secrets set "TradeBot:Solana:BotPrivateKey" "YOUR_PRIVATE_KEY"
dotnet user-secrets set "TradeBot:Solana:TokenMint" "YOUR_TOKEN_MINT"
```

#### Option 2: Environment Variables (Production)

```bash
export TradeBot__Solana__BotPrivateKey="YOUR_PRIVATE_KEY"
export TradeBot__Solana__TokenMint="YOUR_TOKEN_MINT"
```

#### Option 3: Azure Key Vault / AWS Secrets Manager (Production)

Configure your cloud provider's secret manager and reference secrets in configuration.

### Enable Trading

Set `TradeBot:Trading:Enabled` to `true` when ready to trade live.

## Installation

### 1. Install Dependencies

The required NuGet packages are already added:

- Solnet.Wallet (v6.8.0)
- Solnet.Rpc (v6.8.0)
- Solnet.Programs (v6.8.0)

Restore packages:

```bash
dotnet restore
```

### 2. Configure Secrets

```bash
dotnet user-secrets set "TradeBot:Solana:BotPrivateKey" "YOUR_BASE58_PRIVATE_KEY"
dotnet user-secrets set "TradeBot:Solana:TokenMint" "YOUR_SPL_TOKEN_MINT_ADDRESS"
dotnet user-secrets set "TradeBot:Trading:Enabled" "true"
```

### 3. Fund Your Wallet

Ensure your bot wallet has:

- Sufficient SOL for transactions (gas fees + trading)
- Tokens if you plan to sell tokens for SOL

### 4. Run the Server

```bash
dotnet run
```

Access Swagger UI at: `https://localhost:5001/swagger`

## Usage Examples

### Get Bot Status

```bash
curl -X GET https://localhost:5001/api/TradingBot/status
```

Response:

```json
{
  "isRunning": true,
  "isEnabled": true,
  "currentSolBalance": 10.5,
  "currentTokenBalance": 1000000,
  "dailyVolumeSol": 150.0,
  "tradesExecutedToday": 5,
  "lastTradeAt": "2025-10-08T12:34:56Z"
}
```

### Execute SOL → Token Trade

```bash
curl -X POST https://localhost:5001/api/TradingBot/trade \
  -H "Content-Type: application/json" \
  -d '{
    "direction": "SolToToken",
    "amountSol": 1.0
  }'
```

Response:

```json
{
  "success": true,
  "transactionSignature": "3xH...abc",
  "details": {
    "inputMint": "So11111111111111111111111111111111111111112",
    "outputMint": "YourTokenMint...",
    "inputAmount": 1.0,
    "outputAmount": 125000,
    "priceImpactPct": 0.15,
    "executedAt": "2025-10-08T12:35:00Z"
  }
}
```

## Risk Controls

### Automatic Protections

1. **Per-Trade Limit**: Maximum 50 SOL per trade (configurable)
2. **Daily Volume Limit**: Maximum 500 SOL traded per day (configurable)
3. **Price Impact Check**: Rejects trades with >1% price impact
4. **Minimum Balance**: Ensures 0.1 SOL minimum balance (configurable)
5. **Slippage Protection**: 0.3% default slippage tolerance (30 bps)

### Daily Reset

Daily counters automatically reset at UTC midnight.

## Monitoring

### Logs

The bot logs all operations:

- Trade executions
- Risk check violations
- Balance queries
- Jupiter API calls
- Transaction confirmations

### Recommended Monitoring

1. Daily volume tracking
2. Failed transaction alerts
3. Balance threshold alerts
4. Price impact warnings

## Troubleshooting

### "Bot private key is not configured"

Set the private key using user secrets or environment variables.

### "Trading bot is disabled"

Set `TradeBot:Trading:Enabled` to `true` in configuration.

### "Insufficient SOL balance"

Fund your wallet with more SOL. The bot requires `MinBalanceSol` + trade amount + gas fees.

### "Risk check failed: exceeds daily volume limit"

Wait until UTC midnight for daily counters to reset, or increase `MaxDailyNotionalSol`.

### "Price impact too high"

Reduce trade size or wait for better market liquidity.

### Transaction confirmation timeout

- Check RPC endpoint is responsive
- Consider using a premium RPC provider (Helius, QuickNode, Triton)
- Increase `TimeoutSeconds` in configuration

## Production Recommendations

1. **Use Premium RPC**: Mainnet-beta public RPC has rate limits
   - Helius: <https://helius.xyz>
   - QuickNode: <https://quicknode.com>
   - Triton: <https://triton.one>

2. **Implement Additional Monitoring**:
   - Application Performance Monitoring (APM)
   - Error tracking (Sentry, Application Insights)
   - Transaction success rate metrics

3. **Set Conservative Limits Initially**:
   - Start with low `MaxTradeNotionalSol` (1-5 SOL)
   - Start with low `MaxDailyNotionalSol` (10-50 SOL)
   - Gradually increase after testing

4. **Regular Security Audits**:
   - Rotate wallet keys periodically
   - Review transaction logs
   - Monitor for unusual patterns

5. **Backup & Recovery**:
   - Securely backup private keys
   - Document recovery procedures
   - Test disaster recovery plan

## Removed from Original Implementation

The following issues from the client-side code were fixed:

1. ✅ Moved from Angular client to ASP.NET Core backend
2. ✅ Proper secrets management (no hardcoded keys)
3. ✅ Implemented complete risk management (not TODO)
4. ✅ Fixed Jupiter v6 API implementation (correct response structure)
5. ✅ Added balance verification before trades
6. ✅ Proper error handling and logging
7. ✅ Transaction confirmation waiting logic
8. ✅ Removed immediate execution (trades via API only)
9. ✅ Used proper constants (no magic numbers)
10. ✅ Proper dependency injection and service architecture

## License

This trading bot is part of The16Oracles project.
