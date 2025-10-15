# Oracle Wars - Discord Bot Integration

This guide explains how to integrate the Oracle Wars game with your Discord bot using The16Oracles.domain.

## Overview

The Discord bot integration provides both **slash commands** (`/ow-*`) and **prefix commands** (`!ow-*`) for playing Oracle Wars directly in Discord.

## Architecture

```
Discord Bot (The16Oracles.domain)
    ↓
OracleWarsApiService (HTTP Client)
    ↓
DAOA Web API (The16Oracles.DAOA)
    ↓
Oracle Wars Game Service
```

## Setup Instructions

### 1. Start the DAOA API

First, ensure the Oracle Wars API is running:

```bash
cd The16Oracles.DAOA
dotnet run
```

The API should be accessible at `https://localhost:5001` (or `http://localhost:5000`)

### 2. Configure Discord Bot

Update your `config.json` in The16Oracles.console directory:

```json
{
  "SolutionName": "The16Oracles",
  "SolutionDisplayName": "The 16 Oracles",
  "ProjectVersion": "1.0.0",
  "Developer": "Your Name",
  "Discords": [
    {
      "Id": 1,
      "Name": "Oracle Wars Bot",
      "Token": "YOUR_BOT_TOKEN_ENV_VAR",
      "CommandPrefix": "!",
      "WelcomeChannelId": "123456789",
      "AssetsChannelId": "123456789",
      "LaunchpadUrl": "https://your-launchpad.com",
      "OracleWarsApiUrl": "https://localhost:5001",
      "Oracles": []
    }
  ]
}
```

**Important:**
- `Token` should be the **environment variable name** containing your Discord bot token
- `OracleWarsApiUrl` points to your DAOA API instance
- Set the bot token as an environment variable before running

### 3. Run the Discord Bot

```bash
cd The16Oracles.console
dotnet run
```

The bot will automatically register Oracle Wars commands and connect to Discord.

## Available Commands

### Slash Commands (Modern Discord UI)

| Command | Description | Parameters |
|---------|-------------|------------|
| `/ow-register` | Create your Oracle Wars profile | None |
| `/ow-profile` | View your stats and balance | None |
| `/ow-oracles` | List all available oracles | None |
| `/ow-subscribe` | Subscribe to an oracle | `oracle` (string) |
| `/ow-battle` | Challenge another player | `opponent` (user), `wager` (number) |
| `/ow-execute-battle` | Execute a created battle | `battle-id` (string) |
| `/ow-leaderboard` | View top players | `limit` (number, default: 10) |
| `/ow-daily` | Claim your daily bonus | None |

### Prefix Commands (Traditional)

| Command | Aliases | Description | Usage |
|---------|---------|-------------|-------|
| `!ow-register` | - | Create your profile | `!ow-register` |
| `!ow-profile` | `!ow-stats`, `!ow-me` | View your stats | `!ow-profile` |
| `!ow-oracles` | `!ow-list` | List available oracles | `!ow-oracles` |
| `!ow-subscribe` | - | Subscribe to oracle | `!ow-subscribe whale-behavior` |
| `!ow-unsubscribe` | - | Unsubscribe from oracle | `!ow-unsubscribe whale-behavior` |
| `!ow-battle` | - | Challenge player (auto-executes) | `!ow-battle @user 10` |
| `!ow-leaderboard` | `!ow-top`, `!ow-lb` | View leaderboard | `!ow-leaderboard 20` |
| `!ow-daily` | `!ow-bonus` | Claim daily bonus | `!ow-daily` |
| `!ow-help` | - | Show help message | `!ow-help` |

## Command Examples

### Getting Started

1. **Register for the game:**
   ```
   /ow-register
   ```
   or
   ```
   !ow-register
   ```

2. **Check your profile:**
   ```
   /ow-profile
   ```

3. **View available oracles:**
   ```
   /ow-oracles
   ```

### Playing the Game

1. **Subscribe to an oracle:**
   ```
   /ow-subscribe oracle:whale-behavior
   ```
   or
   ```
   !ow-subscribe whale-behavior
   ```

2. **Battle another player:**
   ```
   /ow-battle opponent:@Alice wager:10
   ```
   or
   ```
   !ow-battle @Alice 10
   ```

3. **Claim daily bonus:**
   ```
   /ow-daily
   ```

4. **Check leaderboard:**
   ```
   /ow-leaderboard limit:20
   ```

## Discord Embed Features

The bot uses rich Discord embeds with:

- **Color-coded responses** (Green for success, Red for errors, Gold for battles)
- **Field layouts** for organized information
- **User avatars** in profile displays
- **Timestamps** for battle results
- **Formatted leaderboards** with rankings and stats
- **Battle narratives** with emoji decorations

### Example Profile Embed

```
⚔️ CryptoWarrior's Profile

💰 SOL Balance: 150 SOL
🏆 Wins: 12
💀 Losses: 3
📊 Win Rate: 80.0%
🔮 Subscribed Oracles: 4

📜 Oracle List: whale-behavior, black-swan, defi-liquidity, nft-sentiment
```

### Example Battle Result

```
⚔️ Battle Results!

Epic Oracle Battle!

CryptoWarrior (Score: 45.67) challenged OracleKing (Score: 38.21)
Wager: 10 SOL

The oracles have spoken! CryptoWarrior emerges victorious!

🏆 Winner: @CryptoWarrior
💰 Winnings: 10 SOL
📊 Challenger Score: 45.67
📊 Opponent Score: 38.21
```

## Oracle List (Quick Reference)

### Market Analysis
- `macro-trends` - 15 SOL (Power: 3)
- `defi-liquidity` - 20 SOL (Power: 4)
- `whale-behavior` - 30 SOL (Power: 5)
- `nft-sentiment` - 10 SOL (Power: 2)

### Risk Detection
- `black-swan` - 50 SOL (Power: 8) ⭐ Most Powerful
- `regulatory-risk` - 25 SOL (Power: 4)

### Opportunities
- `airdrop-opportunities` - 15 SOL (Power: 3)
- `emerging-market-surge` - 35 SOL (Power: 6)

### Technical Metrics
- `l2-activity` - 20 SOL (Power: 4)
- `chain-interoperability` - 25 SOL (Power: 5)
- `node-validator-profits` - 15 SOL (Power: 3)

### Trends
- `ai-narrative-trends` - 40 SOL (Power: 7)
- `tokenomics-supply` - 20 SOL (Power: 4)
- `tech-adoption` - 25 SOL (Power: 5)

## Technical Details

### Files Created

**The16Oracles.domain:**
- `Models/OracleWars.cs` - DTOs for API communication
- `Services/OracleWarsApiService.cs` - HTTP client for DAOA API
- `Commands/OracleWarsSlashCommands.cs` - Slash command handlers
- `Commands/OracleWarsPrefixCommands.cs` - Prefix command handlers
- `Services/DiscordBot.cs` (modified) - Command registration

**Configuration:**
- `Models/Discord.cs` (modified) - Added `OracleWarsApiUrl` property

### API Communication Flow

1. **User executes Discord command** (e.g., `/ow-battle`)
2. **Command handler** extracts parameters
3. **OracleWarsApiService** makes HTTP request to DAOA API
4. **DAOA API** processes game logic
5. **Response returned** as JSON
6. **Discord embed created** with formatted data
7. **Embed sent** to Discord channel

### Error Handling

All commands include error handling:
- ❌ Red embeds for errors
- ✅ Green embeds for success
- 🟠 Orange embeds for pending actions

Common errors handled:
- Player not found (prompt to register)
- Insufficient balance
- Invalid oracle names
- Battle not found
- API connection failures

## Bot Permissions Required

Your Discord bot needs these permissions:
- `applications.commands` - For slash commands
- `Send Messages` - For responses
- `Embed Links` - For rich embeds
- `Read Message History` - For command processing
- `Use Slash Commands` - For slash command registration

**Permission Integer:** `274877908992`

## Development Tips

### Changing API URL

To point to a production API instead of localhost:

```json
{
  "OracleWarsApiUrl": "https://api.your-domain.com"
}
```

### SSL Certificate Issues (Development)

If using HTTPS with self-signed certificates, you may need to add to `OracleWarsApiService.cs`:

```csharp
public OracleWarsApiService(string baseUrl = "https://localhost:5001")
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    _httpClient = new HttpClient(handler);
    _baseUrl = baseUrl;
}
```

### Adding Custom Commands

To add new commands:

1. Add method to `OracleWarsSlashCommands.cs` or `OracleWarsPrefixCommands.cs`
2. Use `[SlashCommand]` or `[Command]` attribute
3. Call `_apiService` methods
4. Build Discord embed response
5. Rebuild and restart bot

Example:
```csharp
[SlashCommand("ow-custom", "My custom command")]
public async Task CustomCommand(InteractionContext ctx)
{
    await ctx.DeferAsync();
    // Your logic here
    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
}
```

## Troubleshooting

### Bot doesn't respond to slash commands
- Ensure bot has `applications.commands` scope when invited
- Wait 1 hour for Discord to cache commands, or kick/re-invite bot
- Check bot has permission to send messages in channel

### "API request failed: 404"
- Verify DAOA API is running on configured URL
- Check `OracleWarsApiUrl` in config.json
- Test API endpoints in browser: `https://localhost:5001/swagger`

### "Player not found"
- User needs to run `/ow-register` first
- Each user must register individually

### Commands show but give errors
- Check DAOA API logs for errors
- Verify API is accessible from bot server
- Check network/firewall settings

## Production Deployment

### Recommended Setup

1. **Deploy DAOA API** to a production server (Azure, AWS, etc.)
2. **Update config.json** with production API URL
3. **Deploy Discord bot** to hosting service
4. **Set environment variables** for Discord token
5. **Monitor logs** for errors

### Security Considerations

- ✅ Never commit Discord tokens to git
- ✅ Use HTTPS for API communication
- ✅ Validate all user input
- ✅ Rate limit expensive operations
- ✅ Use environment variables for secrets

### Scalability

For multiple Discord servers:
- DAOA API supports multiple concurrent bots
- Player data is keyed by Discord User ID (global)
- Consider adding database persistence for production
- Use load balancer for API if traffic is high

## Next Steps

1. **Test all commands** in your Discord server
2. **Customize embed colors/formatting** to match server theme
3. **Add admin commands** for moderation
4. **Implement persistence** (database) for production
5. **Add analytics** to track game usage
6. **Create tournaments** and events

## Support

For issues or questions:
- Check API Swagger docs: `https://localhost:5001/swagger`
- Review `ORACLE_WARS_GAME.md` for API details
- Test endpoints directly before debugging bot

---

**Enjoy Oracle Wars on Discord!** ⚔️🔮
