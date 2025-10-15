using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using The16Oracles.domain.Commands;
using The16Oracles.domain.Models;

namespace The16Oracles.domain.Services
{
    public class DiscordBot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension? SlashCommands { get; private set; }

        private readonly Discord _config;

        public DiscordBot(Discord configuration)
        {
            _config = configuration;

            var discordConfiguration = new DiscordConfiguration()
            {
                Token = Environment.GetEnvironmentVariable(_config.Token),
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                Intents = DiscordIntents.All
            };

            Client = new DiscordClient(discordConfiguration);

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { _config.CommandPrefix },
                EnableDms = false,
                EnableMentionPrefix = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            // Register Oracle Wars prefix commands
            Commands.RegisterCommands<OracleWarsPrefixCommands>();

            // Register Oracle Wars slash commands
            SlashCommands = Client.UseSlashCommands();
            SlashCommands.RegisterCommands<OracleWarsSlashCommands>();

            Client.Ready += OnClientReady;

            Client.ConnectAsync();
            Task.Delay(-1);
        }

        public string Name => _config.Name;

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation("Bot is ready!");
            return Task.CompletedTask;
        }

    }
}
