using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using The16Oracles.domain.Services;

namespace The16Oracles.domain.Commands
{
    public class OracleWarsPrefixCommands : BaseCommandModule
    {
        private readonly IOracleWarsApiService _apiService;

        public OracleWarsPrefixCommands()
        {
            // TODO: Inject from DI container
            _apiService = new OracleWarsApiService();
        }

        [Command("ow-register")]
        [Description("Register for Oracle Wars and receive 100 SOL")]
        public async Task Register(CommandContext ctx)
        {
            var userId = ctx.User.Id.ToString();
            var username = ctx.User.Username;

            var response = await _apiService.CreatePlayerAsync(userId, username);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Oracle Wars - Registration")
                .WithDescription(response.Message)
                .WithColor(response.Success ? DiscordColor.Green : DiscordColor.Red);

            if (response.Success && response.Data != null)
            {
                embed.AddField("Balance", $"{response.Data.SolBalance} SOL", true);
                embed.AddField("Wins", response.Data.Wins.ToString(), true);
                embed.AddField("Losses", response.Data.Losses.ToString(), true);
            }

            await ctx.RespondAsync(embed);
        }

        [Command("ow-profile")]
        [Aliases("ow-stats", "ow-me")]
        [Description("View your Oracle Wars profile")]
        public async Task Profile(CommandContext ctx)
        {
            var userId = ctx.User.Id.ToString();
            var response = await _apiService.GetPlayerAsync(userId);

            if (!response.Success || response.Data == null)
            {
                await ctx.RespondAsync($"❌ {response.Message}");
                return;
            }

            var player = response.Data;
            var winRate = player.Wins + player.Losses > 0
                ? (double)player.Wins / (player.Wins + player.Losses) * 100
                : 0;

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"⚔️ {player.Username}'s Profile")
                .WithColor(DiscordColor.Gold)
                .AddField("💰 SOL Balance", $"{player.SolBalance} SOL", true)
                .AddField("🏆 Wins", player.Wins.ToString(), true)
                .AddField("💀 Losses", player.Losses.ToString(), true)
                .AddField("📊 Win Rate", $"{winRate:F1}%", true)
                .AddField("🔮 Subscribed Oracles", player.SubscribedOracles.Count.ToString(), true)
                .WithThumbnail(ctx.User.AvatarUrl)
                .WithTimestamp(DateTime.UtcNow);

            if (player.SubscribedOracles.Count > 0)
            {
                embed.AddField("📜 Oracle List", string.Join(", ", player.SubscribedOracles));
            }

            await ctx.RespondAsync(embed);
        }

        [Command("ow-oracles")]
        [Aliases("ow-list")]
        [Description("View all available oracles")]
        public async Task Oracles(CommandContext ctx)
        {
            var response = await _apiService.GetAvailableOraclesAsync();

            if (!response.Success || response.Data == null)
            {
                await ctx.RespondAsync($"❌ {response.Message}");
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🔮 Available Oracles")
                .WithDescription("Subscribe to oracles to gain power in battles!\nUse `!ow-subscribe <oracle-name>` to subscribe")
                .WithColor(DiscordColor.Purple);

            var groupedOracles = response.Data.GroupBy(o => o.Category);

            foreach (var category in groupedOracles)
            {
                var oracleList = string.Join("\n", category.Select(o =>
                    $"**{o.Name}** - {o.SubscriptionCost} SOL (Power: {o.PowerLevel})"));

                embed.AddField($"📂 {category.Key}", oracleList);
            }

            await ctx.RespondAsync(embed);
        }

        [Command("ow-subscribe")]
        [Description("Subscribe to an oracle")]
        public async Task Subscribe(CommandContext ctx, [Description("Oracle name")] string oracleName)
        {
            var userId = ctx.User.Id.ToString();
            var response = await _apiService.SubscribeToOracleAsync(userId, oracleName);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🔮 Oracle Subscription")
                .WithDescription(response.Message)
                .WithColor(response.Success ? DiscordColor.Green : DiscordColor.Red);

            if (response.Success && response.Data != null)
            {
                embed.AddField("💰 Remaining Balance", $"{response.Data.SolBalance} SOL");
                embed.AddField("📜 Total Subscriptions", response.Data.SubscribedOracles.Count.ToString());
            }

            await ctx.RespondAsync(embed);
        }

        [Command("ow-unsubscribe")]
        [Description("Unsubscribe from an oracle")]
        public async Task Unsubscribe(CommandContext ctx, [Description("Oracle name")] string oracleName)
        {
            var userId = ctx.User.Id.ToString();
            var response = await _apiService.UnsubscribeFromOracleAsync(userId, oracleName);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🔮 Oracle Unsubscription")
                .WithDescription(response.Message)
                .WithColor(response.Success ? DiscordColor.Green : DiscordColor.Red);

            await ctx.RespondAsync(embed);
        }

        [Command("ow-battle")]
        [Description("Challenge another player to battle")]
        public async Task Battle(
            CommandContext ctx,
            [Description("Player to challenge")] DiscordUser opponent,
            [Description("SOL amount to wager")] decimal wager)
        {
            if (opponent.Id == ctx.User.Id)
            {
                await ctx.RespondAsync("❌ You cannot battle yourself!");
                return;
            }

            var challengerId = ctx.User.Id.ToString();
            var opponentId = opponent.Id.ToString();

            var battleResponse = await _apiService.CreateBattleAsync(challengerId, opponentId, wager);

            if (!battleResponse.Success || battleResponse.Data == null)
            {
                await ctx.RespondAsync($"❌ {battleResponse.Message}");
                return;
            }

            var battleId = battleResponse.Data.BattleId;

            // Auto-execute the battle
            var executeResponse = await _apiService.ExecuteBattleAsync(battleId);

            if (!executeResponse.Success || executeResponse.Data == null)
            {
                await ctx.RespondAsync($"❌ {executeResponse.Message}");
                return;
            }

            var battle = executeResponse.Data;

            var embed = new DiscordEmbedBuilder()
                .WithTitle("⚔️ Battle Results!")
                .WithDescription(battle.BattleNarrative ?? "The battle has concluded!")
                .WithColor(DiscordColor.Gold)
                .AddField("🏆 Winner", $"<@{battle.Winner}>", true)
                .AddField("💰 Winnings", $"{battle.Wager} SOL", true)
                .AddField("📊 Challenger Score", battle.ChallengerTotalScore.ToString("F2"), true)
                .AddField("📊 Opponent Score", battle.OpponentTotalScore.ToString("F2"), true)
                .WithTimestamp(DateTime.UtcNow);

            await ctx.RespondAsync(embed);
        }

        [Command("ow-leaderboard")]
        [Aliases("ow-top", "ow-lb")]
        [Description("View the top players")]
        public async Task Leaderboard(CommandContext ctx, [Description("Number of players")] int limit = 10)
        {
            var response = await _apiService.GetLeaderboardAsync(limit);

            if (!response.Success || response.Data == null)
            {
                await ctx.RespondAsync($"❌ {response.Message}");
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🏆 Oracle Wars Leaderboard")
                .WithColor(DiscordColor.Gold);

            var leaderboard = string.Join("\n", response.Data.Select(entry =>
                $"**#{entry.Rank}** {entry.Username} - {entry.Wins}W/{entry.Losses}L ({entry.WinRate:F1}%) - {entry.SolBalance} SOL"));

            embed.WithDescription(leaderboard.Length > 0 ? leaderboard : "No players yet!");

            await ctx.RespondAsync(embed);
        }

        [Command("ow-daily")]
        [Aliases("ow-bonus")]
        [Description("Claim your daily bonus")]
        public async Task DailyBonus(CommandContext ctx)
        {
            var userId = ctx.User.Id.ToString();
            var response = await _apiService.ClaimDailyBonusAsync(userId);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🎁 Daily Bonus")
                .WithDescription(response.Message)
                .WithColor(response.Success ? DiscordColor.Green : DiscordColor.Red);

            if (response.Success && response.Data != null)
            {
                embed.AddField("💰 New Balance", $"{response.Data.SolBalance} SOL");
            }

            await ctx.RespondAsync(embed);
        }

        [Command("ow-help")]
        [Description("Show Oracle Wars help")]
        public async Task Help(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("⚔️ Oracle Wars - Help")
                .WithDescription("A competitive crypto-themed strategy game where you battle using oracle subscriptions!")
                .WithColor(DiscordColor.Blurple)
                .AddField("🎮 Getting Started",
                    "`!ow-register` - Create your profile and receive 100 SOL\n" +
                    "`!ow-profile` - View your stats and oracle subscriptions")
                .AddField("🔮 Oracle Management",
                    "`!ow-oracles` - List all available oracles\n" +
                    "`!ow-subscribe <name>` - Subscribe to an oracle\n" +
                    "`!ow-unsubscribe <name>` - Unsubscribe from an oracle")
                .AddField("⚔️ Battling",
                    "`!ow-battle @user <wager>` - Challenge a player (wager in SOL)\n" +
                    "Example: `!ow-battle @Alice 10`")
                .AddField("📊 Tracking",
                    "`!ow-leaderboard [limit]` - View top players\n" +
                    "`!ow-daily` - Claim daily bonus (2 SOL per oracle)")
                .AddField("💡 Tips",
                    "• Subscribe to more oracles for higher battle power\n" +
                    "• Higher power oracles cost more but give better advantages\n" +
                    "• Claim your daily bonus to earn passive SOL\n" +
                    "• Win battles to climb the leaderboard!")
                .WithFooter("May the oracles be ever in your favor!");

            await ctx.RespondAsync(embed);
        }
    }
}
