# Oracle Wars - Discord Bot Game

**Oracle Wars** is a crypto-themed strategy game built into The16Oracles.DAOA Web API. Players can compete using oracle subscriptions, battle other players, and climb the leaderboard.

## Game Overview

Oracle Wars is a competitive game where players:
- Create profiles with starting SOL balance
- Subscribe to powerful crypto oracles for advantages
- Battle other players to earn SOL
- Claim daily bonuses based on oracle subscriptions
- Compete on the global leaderboard

## Game Mechanics

### Starting Resources
- Every new player starts with **100 SOL**
- Use SOL to subscribe to oracles and wager in battles

### Oracle Subscriptions
Each of the 14 available oracles has:
- **Subscription Cost**: 10-50 SOL (one-time payment)
- **Power Level**: 2-8 (affects battle calculations)
- **Category**: Market Analysis, Risk Detection, Opportunities, Technical Metrics, or Trends

### Battle System
1. Challenge another player with a SOL wager (minimum 1 SOL)
2. Both players must have sufficient balance
3. Each subscribed oracle contributes to battle score (live oracle data × power level)
4. Winner takes the wager from the loser
5. Battle history and statistics are tracked

### Daily Bonuses
- Claim once per 24 hours
- Earn **2 SOL per subscribed oracle**
- Encourages oracle diversity and daily engagement

## API Endpoints

All endpoints are under `/api/game/` with the tag **"Oracle Wars Game"**.

### Player Management

#### Create Player
```http
POST /api/game/player/create
Content-Type: application/json

{
  "discordUserId": "123456789",
  "username": "CryptoWarrior"
}
```

#### Get Player Profile
```http
GET /api/game/player/{discordUserId}
```

**Response:**
```json
{
  "success": true,
  "message": "Player profile retrieved",
  "data": {
    "discordUserId": "123456789",
    "username": "CryptoWarrior",
    "solBalance": 100,
    "wins": 0,
    "losses": 0,
    "subscribedOracles": [],
    "createdAt": "2025-10-15T00:00:00Z",
    "lastActive": "2025-10-15T00:00:00Z"
  }
}
```

### Oracle Management

#### Get Available Oracles
```http
GET /api/game/oracles
```

**Response includes all 14 oracles:**
```json
{
  "success": true,
  "message": "Available oracles",
  "data": [
    {
      "name": "macro-trends",
      "displayName": "Macro Economic Trends",
      "description": "Analyzes global economic indicators and trends",
      "subscriptionCost": 15,
      "powerLevel": 3,
      "category": "Market Analysis",
      "apiEndpoint": "/api/oracles/macro-trends"
    }
    // ... 13 more oracles
  ]
}
```

#### Subscribe to Oracle
```http
POST /api/game/oracle/subscribe
Content-Type: application/json

{
  "discordUserId": "123456789",
  "oracleName": "whale-behavior"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Successfully subscribed to Crypto Whale Behavior! Cost: 30 SOL. Remaining balance: 70 SOL",
  "data": {
    "discordUserId": "123456789",
    "username": "CryptoWarrior",
    "solBalance": 70,
    "subscribedOracles": ["whale-behavior"]
  }
}
```

#### Unsubscribe from Oracle
```http
DELETE /api/game/oracle/unsubscribe?discordUserId=123456789&oracleName=whale-behavior
```

### Battle System

#### Create Battle
```http
POST /api/game/battle/create
Content-Type: application/json

{
  "challengerUserId": "123456789",
  "opponentUserId": "987654321",
  "wager": 10
}
```

**Response:**
```json
{
  "success": true,
  "message": "Battle created! CryptoWarrior vs OracleKing for 10 SOL",
  "data": {
    "battleId": "abc123...",
    "challenger": "123456789",
    "opponent": "987654321",
    "wager": 10,
    "status": "Active"
  }
}
```

#### Execute Battle
```http
POST /api/game/battle/{battleId}/execute
```

**Response:**
```json
{
  "success": true,
  "message": "CryptoWarrior wins 10 SOL!",
  "data": {
    "battleId": "abc123...",
    "challenger": "123456789",
    "opponent": "987654321",
    "wager": 10,
    "winner": "123456789",
    "status": "Completed",
    "challengerTotalScore": 45.67,
    "opponentTotalScore": 38.21,
    "challengerOracleScores": {
      "whale-behavior": 15.2,
      "black-swan": 30.47
    },
    "opponentOracleScores": {
      "defi-liquidity": 18.1,
      "nft-sentiment": 20.11
    },
    "battleNarrative": "⚔️ Epic Oracle Battle!\n\nCryptoWarrior (Score: 45.67) challenged OracleKing (Score: 38.21)\nWager: 10 SOL\n\nThe oracles have spoken! CryptoWarrior emerges victorious!"
  }
}
```

### Leaderboard & Bonuses

#### Get Leaderboard
```http
GET /api/game/leaderboard?limit=10
```

**Response:**
```json
{
  "success": true,
  "message": "Leaderboard retrieved",
  "data": [
    {
      "rank": 1,
      "username": "OracleMaster",
      "wins": 25,
      "losses": 3,
      "solBalance": 450,
      "winRate": 89.29
    },
    {
      "rank": 2,
      "username": "CryptoWarrior",
      "wins": 18,
      "losses": 7,
      "solBalance": 320,
      "winRate": 72.0
    }
    // ... more entries
  ]
}
```

#### Claim Daily Bonus
```http
POST /api/game/daily-bonus/{discordUserId}
```

**Response:**
```json
{
  "success": true,
  "message": "Daily bonus claimed! Earned 8 SOL from 4 subscribed oracle(s). New balance: 108 SOL",
  "data": {
    "discordUserId": "123456789",
    "username": "CryptoWarrior",
    "solBalance": 108
  }
}
```

## Available Oracles

| Oracle Name | Display Name | Cost | Power | Category |
|-------------|--------------|------|-------|----------|
| `macro-trends` | Macro Economic Trends | 15 SOL | 3 | Market Analysis |
| `defi-liquidity` | DeFi Liquidity Movements | 20 SOL | 4 | Market Analysis |
| `whale-behavior` | Crypto Whale Behavior | 30 SOL | 5 | Market Analysis |
| `nft-sentiment` | NFT Market Sentiment | 10 SOL | 2 | Market Analysis |
| `black-swan` | Black Swan Detection | 50 SOL | 8 | Risk Detection |
| `regulatory-risk` | Regulatory Risk Forecasting | 25 SOL | 4 | Risk Detection |
| `airdrop-opportunities` | Airdrop Launch Opportunities | 15 SOL | 3 | Opportunities |
| `emerging-market-surge` | Emerging Market Capital Surge | 35 SOL | 6 | Opportunities |
| `l2-activity` | L2 Activity Monitoring | 20 SOL | 4 | Technical Metrics |
| `chain-interoperability` | Chain Interoperability Metrics | 25 SOL | 5 | Technical Metrics |
| `node-validator-profits` | Node Validator Profits | 15 SOL | 3 | Technical Metrics |
| `ai-narrative-trends` | AI Narrative Trend Detection | 40 SOL | 7 | Trends |
| `tokenomics-supply` | Tokenomics Supply Curves | 20 SOL | 4 | Trends |
| `tech-adoption` | Tech Adoption Curves | 25 SOL | 5 | Trends |

## Strategy Tips

1. **Balanced Portfolio**: Subscribe to a mix of high and low power oracles
2. **High-Risk, High-Reward**: Black Swan Detection (50 SOL, power 8) is the most powerful
3. **Budget Friendly**: NFT Market Sentiment (10 SOL) is great for new players
4. **Daily Grind**: More subscriptions = more daily bonus (2 SOL each)
5. **Smart Wagering**: Start with small wagers to test your oracle strength

## Discord Bot Integration

This API is designed to be consumed by a Discord bot. Example bot commands:

- `/oracle-wars register` - Create player profile
- `/oracle-wars profile` - View your stats
- `/oracle-wars oracles` - List available oracles
- `/oracle-wars subscribe <oracle-name>` - Subscribe to an oracle
- `/oracle-wars battle <@opponent> <wager>` - Challenge a player
- `/oracle-wars leaderboard` - View top players
- `/oracle-wars daily` - Claim daily bonus

## Running the Game

### Start the DAOA API
```bash
dotnet run --project The16Oracles.DAOA/The16Oracles.DAOA.csproj
```

Access Swagger UI at `https://localhost:5001/swagger` to test all game endpoints.

### Swagger Tags
All game endpoints are grouped under the **"Oracle Wars Game"** tag in Swagger for easy discovery.

## Technical Architecture

### In-Memory Storage
- Player data and battles are stored in-memory using `ConcurrentDictionary`
- Data persists only during application runtime
- For production, integrate with a database (Entity Framework, MongoDB, etc.)

### Oracle Integration
- Battle calculations use **live oracle data** from the 16 crypto oracles
- Each oracle's `EvaluateAsync()` method returns a confidence score (0-1)
- Final battle score = Σ(oracle confidence × oracle power level)
- Fallback to simulated scores if oracle API calls fail

### Service Pattern
- `IOracleGameService` - Main game service interface
- `OracleGameService` - Singleton service implementation
- Registered in `Program.cs` with dependency injection

## Future Enhancements

1. **Persistence**: Add database storage for player data
2. **Tournaments**: Multi-player bracket tournaments
3. **Power-ups**: Temporary boost items purchasable with SOL
4. **Achievements**: Unlock badges and titles
5. **Oracle Rentals**: Temporary oracle access for reduced cost
6. **Guild System**: Team-based battles and shared resources
7. **Real SOL Integration**: Connect to actual Solana wallets (via SPL Token API)

## Error Handling

All endpoints return standardized `GameResponse<T>` objects:

```json
{
  "success": false,
  "message": "Insufficient balance. Need 50 SOL, have 30 SOL",
  "data": null
}
```

Common errors:
- **Player not found**: Create profile first
- **Insufficient balance**: Earn more SOL through battles
- **Oracle already subscribed**: Each oracle can only be subscribed once
- **Cannot battle yourself**: Choose a different opponent
- **Battle not active**: Battle already completed or cancelled

## Support & Feedback

For issues or feature requests, please refer to the main project documentation.

---

**Have fun battling in Oracle Wars!** May the oracles be ever in your favor!
