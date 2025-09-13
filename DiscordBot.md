# Discord NFT Bot Documentation

A comprehensive Discord bot built with DSharpPlus that integrates OpenAI for AI-generated content, NFT showcasing, and war game mechanics.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Commands](#commands)
- [Architecture](#architecture)
- [API Integration](#api-integration)
- [Customization](#customization)
- [Troubleshooting](#troubleshooting)

## Features

### Core Functionality
- **AI-Powered Welcome Messages** - Generates unique, lore-rich welcome messages for new members
- **NFT Asset Management** - Automatically pulls images from designated Discord channels
- **Interactive Commands** - Both traditional prefix and modern slash commands
- **War Game Integration** - Immersive game mechanics with AI-generated narratives
- **Professional Embeds** - Branded messages with sale links and copyright footers

### AI Integration
- **OpenAI GPT Integration** - Dynamic content generation for welcomes and game lore
- **Contextual Responses** - AI adapts to user names and battle contexts
- **Fallback System** - Graceful degradation when AI services are unavailable

### Asset Management
- **Channel Integration** - Reads images directly from Discord asset channels
- **Random Selection** - Automatically selects random assets for showcases
- **Multiple Format Support** - Handles various image formats and attachments

## Prerequisites

### System Requirements
- .NET 6.0 or higher
- Visual Studio 2022 or VS Code
- Discord Developer Account
- OpenAI API Account

### NuGet Dependencies
```xml
<PackageReference Include="DSharpPlus" Version="4.4.0" />
<PackageReference Include="DSharpPlus.CommandsNext" Version="4.4.0" />
<PackageReference Include="DSharpPlus.SlashCommands" Version="4.4.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

## Installation

### Step 1: Clone and Setup
1. Create a new C# Console Application
2. Install required NuGet packages
3. Copy the bot code into your project
4. Build the solution

### Step 2: Discord Bot Setup
1. Visit the [Discord Developer Portal](https://discord.com/developers/applications)
2. Create a new application
3. Navigate to the "Bot" section
4. Create a bot and copy the token
5. Enable necessary intents:
   - Server Members Intent
   - Message Content Intent

### Step 3: Invite Bot to Server
Use this URL template (replace `YOUR_CLIENT_ID`):
```
https://discord.com/api/oauth2/authorize?client_id=YOUR_CLIENT_ID&permissions=8&scope=bot%20applications.commands
```

## Configuration

### config.json Structure
Create a `config.json` file in your project root:

```json
{
  "DiscordToken": "YOUR_DISCORD_BOT_TOKEN",
  "OpenAIApiKey": "sk-your-openai-api-key",
  "CommandPrefix": "!",
  "WelcomeChannelId": 123456789012345678,
  "AssetsChannelId": 123456789012345678,
  "LaunchpadUrl": "https://your-nft-launchpad.com",
  "DevIconUrl": "https://your-website.com/dev-icon.png"
}
```

### Configuration Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `DiscordToken` | string | Your Discord bot token |
| `OpenAIApiKey` | string | Your OpenAI API key (starts with 'sk-') |
| `CommandPrefix` | string | Prefix for traditional commands (default: "!") |
| `WelcomeChannelId` | ulong | Channel ID where welcome messages are sent |
| `AssetsChannelId` | ulong | Channel ID containing NFT images |
| `LaunchpadUrl` | string | URL to your NFT marketplace/launchpad |
| `DevIconUrl` | string | URL to your developer/brand icon |

### Getting Channel IDs
1. Enable Developer Mode in Discord (User Settings > Advanced)
2. Right-click on the desired channel
3. Select "Copy ID"

## Commands

### Traditional Commands (Prefix: !)

#### NFT Commands
| Command | Description | Usage |
|---------|-------------|--------|
| `!nft showcase` | Display a random NFT from collection | `!nft showcase` |
| `!nft collection` | Show collection overview | `!nft collection` |

#### Game Commands
| Command | Description | Usage |
|---------|-------------|--------|
| `!game warriors` | List available warriors | `!game warriors` |
| `!game battle [type]` | Start a battle with AI narrative | `!game battle arena` |

### Slash Commands

#### /nft-gallery
Interactive NFT browser with dropdown selection
- **Usage**: `/nft-gallery`
- **Features**: 
  - Dropdown menu with up to 20 NFTs
  - Real-time asset loading from channels
  - Rich embed displays

#### /warrior-select
Warrior selection interface for battles
- **Usage**: `/warrior-select`
- **Features**: 
  - Multi-select dropdown (up to 3 warriors)
  - Warrior stats and descriptions
  - Battle formation setup

## Architecture

### Core Classes

#### `Bot` Class
Main bot instance handling Discord connection and event management.

**Key Methods:**
- `RunAsync()` - Initializes and starts the bot
- `OnClientReady()` - Handles bot ready event
- `OnMemberJoined()` - Processes new member welcomes

#### `OpenAIService` Class
Manages all AI integration and content generation.

**Key Methods:**
- `GenerateWelcomeMessageAsync(string username)` - Creates personalized welcome messages
- `GenerateGameLoreAsync(string context)` - Generates battle narratives and lore

#### `AssetManager` Class
Handles NFT asset retrieval and management from Discord channels.

**Key Methods:**
- `GetRandomAssetAsync()` - Retrieves random asset from channel
- `GetNFTAssetsAsync()` - Gets all assets from channel for listings

#### Command Modules
- `NFTCommands` - Traditional prefix commands for NFT operations
- `GameCommands` - War game related commands
- `SlashNFTCommands` - Modern slash command implementations

### Data Flow

```
Discord Event → Bot Event Handler → Service Layer → Discord Response
     ↓              ↓                    ↓              ↑
New Member → OnMemberJoined → OpenAI + Assets → Welcome Message
   Join                          Service         
```

## API Integration

### OpenAI Integration

#### Welcome Message Generation
```csharp
var request = new {
    model = "gpt-3.5-turbo",
    messages = new[] {
        new { 
            role = "system", 
            content = "You are a mystical war game narrator..." 
        },
        new { 
            role = "user", 
            content = $"Welcome {username} to our NFT war game community." 
        }
    },
    max_tokens = 150
};
```

#### Battle Lore Generation
The system uses contextual prompts to generate unique battle narratives based on:
- User display names
- Battle types
- Current game state

### Discord API Usage

#### Embed Creation
```csharp
var embed = new DiscordEmbedBuilder() {
    Title = "Welcome Message",
    Description = "AI-generated content",
    Color = DiscordColor.Gold,
    ImageUrl = "Asset from channel",
    Footer = new DiscordEmbedBuilder.EmbedFooter {
        Text = "© Copyright",
        IconUrl = "Developer icon"
    }
};
```

## Customization

### Adding New Commands

#### Traditional Commands
1. Create a new command class inheriting from `BaseCommandModule`
2. Add `[Command("name")]` attribute to methods
3. Register the command module in `Bot.RunAsync()`

#### Slash Commands
1. Create methods in `SlashNFTCommands` class
2. Add `[SlashCommand("name", "description")]` attribute
3. Use `InteractionContext` for responses

### Modifying AI Prompts

#### System Prompts
Located in `OpenAIService` methods:
- **Welcome Messages**: "You are a mystical war game narrator..."
- **Battle Lore**: "You are an epic fantasy war game lore master..."

#### Customization Tips
- Adjust `max_tokens` for longer/shorter responses
- Modify system prompts to match your game's theme
- Add context variables for more personalized content

### Asset Management Customization

#### Supported File Types
Currently configured for images. Modify `AssetManager.GetNFTAssetsAsync()` to support additional formats:

```csharp
if (attachment.MediaType?.StartsWith("image/") == true)
{
    // Add support for other media types
}
```

### Styling and Branding

#### Embed Colors
Customize embed colors in command methods:
- `DiscordColor.Gold` - Welcome messages
- `DiscordColor.Purple` - NFT showcases  
- `DiscordColor.Red` - War game content

#### Footer Customization
Modify footer content in embed builders to match your branding.

## Troubleshooting

### Common Issues

#### Bot Not Responding
**Symptoms**: Commands not executing, no responses
**Solutions**:
1. Verify bot token in `config.json`
2. Check bot permissions in Discord server
3. Ensure Message Content Intent is enabled
4. Verify command prefixes match configuration

#### AI Integration Failures
**Symptoms**: Generic fallback messages appearing
**Solutions**:
1. Verify OpenAI API key validity
2. Check API quota and billing status
3. Monitor console for HTTP errors
4. Test API key with direct curl request

#### Asset Loading Issues
**Symptoms**: "No NFT assets found" messages
**Solutions**:
1. Verify `AssetsChannelId` is correct
2. Ensure bot has read permissions for assets channel
3. Check that images exist in the specified channel
4. Verify image formats are supported

#### Welcome Messages Not Sending
**Symptoms**: New members not receiving welcomes
**Solutions**:
1. Verify `WelcomeChannelId` configuration
2. Check Server Members Intent is enabled
3. Ensure bot has send message permissions in welcome channel
4. Monitor console for event handler errors

### Debugging Tips

#### Enable Detailed Logging
The bot uses Microsoft.Extensions.Logging. Increase log level for debugging:

```csharp
MinimumLogLevel = LogLevel.Debug
```

#### Console Output Monitoring
Watch console output for:
- API request failures
- Permission errors
- Configuration loading issues
- Asset retrieval problems

### Performance Optimization

#### Asset Caching
Consider implementing caching for frequently accessed assets:
- Cache asset lists in memory
- Implement periodic cache refresh
- Store popular assets locally

#### Rate Limiting
Be aware of API rate limits:
- Discord: 5 requests per 5 seconds per bot
- OpenAI: Depends on your plan tier

### Security Considerations

#### Token Management
- Never commit tokens to version control
- Use environment variables in production
- Rotate tokens periodically
- Implement proper access controls

#### Input Validation
The bot includes basic input validation, but consider adding:
- Command parameter sanitization
- User permission checks
- Rate limiting for expensive operations

## Support and Contributing

### Getting Help
- Check the troubleshooting section first
- Review Discord and OpenAI API documentation
- Monitor bot console output for error details

### Contributing
When modifying the bot:
1. Test thoroughly in a development server
2. Follow C# coding conventions
3. Update documentation for new features
4. Consider backward compatibility

### Version History
- **v1.0** - Initial release with core functionality
- Features: AI welcomes, NFT showcases, war game commands
- Integrations: OpenAI, Discord asset channels

---

*This documentation covers the essential aspects of the Discord NFT Bot. For specific implementation details, refer to the source code and inline comments.*