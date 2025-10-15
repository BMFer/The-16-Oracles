# Quick Start Guide - Oracle Wars Discord Game

Get Oracle Wars running on Discord in 5 minutes!

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Discord Bot with token
- Terminal/Command Prompt

## Step 1: Start the Game API (DAOA)

Open a terminal and run:

```bash
cd The16Oracles.DAOA
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

✅ **Keep this running** - the Discord bot needs it!

## Step 2: Configure Discord Bot

### 2a. Get Discord Bot Token

1. Go to [Discord Developer Portal](https://discord.com/developers/applications)
2. Create a new application
3. Go to "Bot" section
4. Click "Reset Token" and copy your bot token
5. Save it as an environment variable:

**Windows:**
```cmd
setx DISCORD_BOT_TOKEN "your-bot-token-here"
```

**Linux/Mac:**
```bash
export DISCORD_BOT_TOKEN="your-bot-token-here"
```

### 2b. Invite Bot to Server

1. In Developer Portal, go to "OAuth2" → "URL Generator"
2. Select scopes:
   - ✅ `bot`
   - ✅ `applications.commands`
3. Select bot permissions:
   - ✅ Send Messages
   - ✅ Embed Links
   - ✅ Read Message History
   - ✅ Use Slash Commands
4. Copy the generated URL and open in browser
5. Select your server and authorize

### 2c. Create config.json

Create `The16Oracles.console/config.json`:

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
      "Token": "DISCORD_BOT_TOKEN",
      "CommandPrefix": "!",
      "WelcomeChannelId": "123456789",
      "AssetsChannelId": "123456789",
      "LaunchpadUrl": "https://your-project.com",
      "OracleWarsApiUrl": "https://localhost:5001",
      "Oracles": []
    }
  ]
}
```

**Note:** `Token` value should match your environment variable name.

## Step 3: Start Discord Bot

Open a **new terminal** (keep DAOA running!) and run:

```bash
cd The16Oracles.console
dotnet run
```

You should see:
```
Oracle Wars Bot
Bot is ready!
The Oracle Wars Bot bot is loaded and ready!
```

✅ Your bot should now be online in Discord!

## Step 4: Test in Discord

In any channel where the bot has access:

### Register
```
/ow-register
```
or
```
!ow-register
```

You should get a response:
```
Oracle Wars - Registration
Welcome to Oracle Wars, YourUsername! You start with 100 SOL.

Balance: 100 SOL
Wins: 0
Losses: 0
```

### View Oracles
```
/ow-oracles
```

You'll see all 14 available oracles grouped by category.

### Subscribe to an Oracle
```
/ow-subscribe oracle:whale-behavior
```
or
```
!ow-subscribe whale-behavior
```

### Battle Another Player

First, have another user register. Then:
```
/ow-battle opponent:@OtherUser wager:10
```
or
```
!ow-battle @OtherUser 10
```

### View Leaderboard
```
/ow-leaderboard
```

### Claim Daily Bonus
```
/ow-daily
```

## Complete Command Reference

### Slash Commands (Recommended)
- `/ow-register` - Create profile
- `/ow-profile` - View stats
- `/ow-oracles` - List oracles
- `/ow-subscribe <oracle>` - Subscribe
- `/ow-battle <@user> <wager>` - Battle
- `/ow-leaderboard [limit]` - Rankings
- `/ow-daily` - Daily bonus

### Prefix Commands (Alternative)
- `!ow-help` - Show help
- `!ow-register` - Create profile
- `!ow-profile` - View stats
- `!ow-oracles` - List oracles
- `!ow-subscribe <name>` - Subscribe
- `!ow-battle @user <wager>` - Battle
- `!ow-leaderboard [limit]` - Rankings
- `!ow-daily` - Daily bonus

## Troubleshooting

### Slash commands don't appear
- Wait 1 hour for Discord to cache them
- OR kick the bot and re-invite with the OAuth URL
- Make sure bot has `applications.commands` scope

### Bot doesn't respond
- Check both terminals are running (DAOA + Bot)
- Verify bot has "Send Messages" permission
- Check bot is online in Discord member list

### "Player not found"
- Run `/ow-register` first
- Each user must register individually

### "API request failed"
- Ensure DAOA is running on `https://localhost:5001`
- Check firewall isn't blocking localhost
- Try `http://localhost:5000` in config if HTTPS fails

### "Insufficient balance"
- Check balance with `/ow-profile`
- Claim daily bonus with `/ow-daily`
- Win battles to earn SOL

## Game Tips

1. **Start Cheap:** Subscribe to `nft-sentiment` (10 SOL) first
2. **Build Power:** Save up for `black-swan` (50 SOL, Power 8)
3. **Daily Grind:** Subscribe to 5 oracles = 10 SOL/day bonus
4. **Strategic Battles:** Challenge players with fewer oracles
5. **Diversify:** Mix high and low power oracles

## Oracle Quick Reference

| Name | Cost | Power | Good For |
|------|------|-------|----------|
| `nft-sentiment` | 10 SOL | 2 | Beginners |
| `macro-trends` | 15 SOL | 3 | Balanced |
| `whale-behavior` | 30 SOL | 5 | Mid-tier |
| `ai-narrative-trends` | 40 SOL | 7 | Advanced |
| `black-swan` | 50 SOL | 8 | Max Power |

## Next Steps

- Read `ORACLE_WARS_GAME.md` for detailed API documentation
- Read `DISCORD_ORACLE_WARS.md` for integration details
- Customize bot embed colors and messages
- Add more Discord servers
- Deploy to production hosting

## Development Commands

### Rebuild Everything
```bash
dotnet build The16Oracles.sln
```

### Run Tests
```bash
dotnet test
```

### View API Docs
While DAOA is running, visit:
```
https://localhost:5001/swagger
```

## Production Deployment

For production use:

1. **Deploy DAOA API** to a server (Azure, AWS, DigitalOcean, etc.)
2. **Update config.json** `OracleWarsApiUrl` to production URL
3. **Deploy Discord bot** to hosting (same or different server)
4. **Use database** instead of in-memory storage (add Entity Framework)
5. **Set up monitoring** and logging

## Support

If you encounter issues:

1. Check both terminals for error messages
2. Test API at `https://localhost:5001/swagger`
3. Verify Discord bot permissions
4. Review documentation in `ORACLE_WARS_GAME.md`

## Architecture Overview

```
┌─────────────────┐
│  Discord User   │
└────────┬────────┘
         │ /ow-battle @user 10
         ▼
┌─────────────────────────────┐
│   Discord Bot (domain)      │
│  - Slash Commands           │
│  - Prefix Commands          │
│  - Embed Formatting         │
└────────┬────────────────────┘
         │ HTTP Request
         ▼
┌─────────────────────────────┐
│  OracleWarsApiService       │
│  - HTTP Client              │
│  - JSON Serialization       │
└────────┬────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│  DAOA Web API               │
│  - REST Endpoints           │
│  - Swagger Docs             │
└────────┬────────────────────┘
         │
         ▼
┌─────────────────────────────┐
│  OracleGameService          │
│  - Game Logic               │
│  - Battle Calculations      │
│  - Oracle Integration       │
└─────────────────────────────┘
```

---

**Enjoy Oracle Wars!** ⚔️ May the oracles be ever in your favor!
