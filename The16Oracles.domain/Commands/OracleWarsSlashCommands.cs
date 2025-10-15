using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using The16Oracles.domain.Services;

namespace The16Oracles.domain.Commands
{
    public class OracleWarsSlashCommands : ApplicationCommandModule
    {
        private readonly IOracleWarsApiService _apiService;

        public OracleWarsSlashCommands()
        {
            // TODO: Inject from DI container
            _apiService = new OracleWarsApiService();
        }

        [SlashCommand("ow-register", "Register for Oracle Wars and receive 100 SOL")]
        public async Task RegisterCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

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

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ow-profile", "View your Oracle Wars profile")]
        public async Task ProfileCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            var userId = ctx.User.Id.ToString();
            var response = await _apiService.GetPlayerAsync(userId);

            if (!response.Success || response.Data == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ {response.Message}"));
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

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ow-oracles", "View all available oracles")]
        public async Task OraclesCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            var response = await _apiService.GetAvailableOraclesAsync();

            if (!response.Success || response.Data == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ {response.Message}"));
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🔮 Available Oracles")
                .WithDescription("Subscribe to oracles to gain power in battles!")
                .WithColor(DiscordColor.Purple);

            var groupedOracles = response.Data.GroupBy(o => o.Category);

            foreach (var category in groupedOracles)
            {
                var oracleList = string.Join("\n", category.Select(o =>
                    $"**{o.DisplayName}** - {o.SubscriptionCost} SOL (Power: {o.PowerLevel})"));

                embed.AddField($"📂 {category.Key}", oracleList);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ow-subscribe", "Subscribe to an oracle")]
        public async Task SubscribeCommand(
            InteractionContext ctx,
            [Option("oracle", "Oracle name (e.g., whale-behavior)")] string oracleName)
        {
            await ctx.DeferAsync();

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

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ow-battle", "Challenge another player to battle")]
        public async Task BattleCommand(
            InteractionContext ctx,
            [Option("opponent", "Player to challenge")] DiscordUser opponent,
            [Option("wager", "SOL amount to wager")] double wager)
        {
            await ctx.DeferAsync();

            if (opponent.Id == ctx.User.Id)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent("❌ You cannot battle yourself!"));
                return;
            }

            var challengerId = ctx.User.Id.ToString();
            var opponentId = opponent.Id.ToString();

            var battleResponse = await _apiService.CreateBattleAsync(challengerId, opponentId, (decimal)wager);

            if (!battleResponse.Success || battleResponse.Data == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ {battleResponse.Message}"));
                return;
            }

            var battleId = battleResponse.Data.BattleId;

            var embed = new DiscordEmbedBuilder()
                .WithTitle("⚔️ Battle Created!")
                .WithDescription($"{ctx.User.Mention} challenges {opponent.Mention} to Oracle Wars!")
                .WithColor(DiscordColor.Orange)
                .AddField("💰 Wager", $"{wager} SOL")
                .AddField("🆔 Battle ID", battleId);

            var button = new DiscordButtonComponent(
                ButtonStyle.Success,
                $"execute_battle_{battleId}",
                "⚔️ Execute Battle"
            );

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(embed)
                .AddComponents(button));
        }

        [SlashCommand("ow-execute-battle", "Execute a battle by ID")]
        public async Task ExecuteBattleCommand(
            InteractionContext ctx,
            [Option("battle-id", "Battle ID to execute")] string battleId)
        {
            await ctx.DeferAsync();

            var response = await _apiService.ExecuteBattleAsync(battleId);

            if (!response.Success || response.Data == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ {response.Message}"));
                return;
            }

            var battle = response.Data;

            var embed = new DiscordEmbedBuilder()
                .WithTitle("⚔️ Battle Results!")
                .WithDescription(battle.BattleNarrative ?? "The battle has concluded!")
                .WithColor(DiscordColor.Gold)
                .AddField("🏆 Winner", $"<@{battle.Winner}>", true)
                .AddField("💰 Winnings", $"{battle.Wager} SOL", true)
                .AddField("📊 Challenger Score", battle.ChallengerTotalScore.ToString("F2"), true)
                .AddField("📊 Opponent Score", battle.OpponentTotalScore.ToString("F2"), true)
                .WithTimestamp(DateTime.UtcNow);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ow-leaderboard", "View the top players")]
        public async Task LeaderboardCommand(
            InteractionContext ctx,
            [Option("limit", "Number of players to show")] long limit = 10)
        {
            await ctx.DeferAsync();

            var response = await _apiService.GetLeaderboardAsync((int)limit);

            if (!response.Success || response.Data == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"❌ {response.Message}"));
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle("🏆 Oracle Wars Leaderboard")
                .WithColor(DiscordColor.Gold);

            var leaderboard = string.Join("\n", response.Data.Select(entry =>
                $"**#{entry.Rank}** {entry.Username} - {entry.Wins}W/{entry.Losses}L ({entry.WinRate:F1}%) - {entry.SolBalance} SOL"));

            embed.WithDescription(leaderboard);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("ow-daily", "Claim your daily bonus")]
        public async Task DailyBonusCommand(InteractionContext ctx)
        {
            await ctx.DeferAsync();

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

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
