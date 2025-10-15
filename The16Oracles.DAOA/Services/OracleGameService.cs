using System.Collections.Concurrent;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Models.Game;

namespace The16Oracles.DAOA.Services;

public class OracleGameService : IOracleGameService
{
    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly ConcurrentDictionary<string, Battle> _battles = new();
    private readonly IEnumerable<IAIOracle> _oracles;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OracleGameService> _logger;

    public OracleGameService(
        IEnumerable<IAIOracle> oracles,
        IHttpClientFactory httpClientFactory,
        ILogger<OracleGameService> logger)
    {
        _oracles = oracles;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public Task<GameResponse<Player>> CreatePlayerAsync(CreatePlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DiscordUserId))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Discord User ID is required"
            });
        }

        if (_players.ContainsKey(request.DiscordUserId))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Player already exists",
                Data = _players[request.DiscordUserId]
            });
        }

        var player = new Player
        {
            DiscordUserId = request.DiscordUserId,
            Username = request.Username,
            SolBalance = 100m,
            CreatedAt = DateTime.UtcNow,
            LastActive = DateTime.UtcNow
        };

        _players[request.DiscordUserId] = player;

        return Task.FromResult(new GameResponse<Player>
        {
            Success = true,
            Message = $"Welcome to Oracle Wars, {player.Username}! You start with {player.SolBalance} SOL.",
            Data = player
        });
    }

    public Task<GameResponse<Player>> GetPlayerAsync(string discordUserId)
    {
        if (!_players.TryGetValue(discordUserId, out var player))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Player not found. Create a profile first!"
            });
        }

        player.LastActive = DateTime.UtcNow;

        return Task.FromResult(new GameResponse<Player>
        {
            Success = true,
            Message = "Player profile retrieved",
            Data = player
        });
    }

    public Task<GameResponse<Player>> SubscribeToOracleAsync(SubscribeOracleRequest request)
    {
        if (!_players.TryGetValue(request.DiscordUserId, out var player))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Player not found"
            });
        }

        var oracleDef = OracleRegistry.Oracles.FirstOrDefault(o => o.Name == request.OracleName);
        if (oracleDef == null)
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Oracle not found"
            });
        }

        if (player.SubscribedOracles.Contains(request.OracleName))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = $"Already subscribed to {oracleDef.DisplayName}"
            });
        }

        if (player.SolBalance < oracleDef.SubscriptionCost)
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = $"Insufficient balance. Need {oracleDef.SubscriptionCost} SOL, have {player.SolBalance} SOL"
            });
        }

        player.SolBalance -= oracleDef.SubscriptionCost;
        player.SubscribedOracles.Add(request.OracleName);
        player.OracleSubscriptions[request.OracleName] = DateTime.UtcNow;

        return Task.FromResult(new GameResponse<Player>
        {
            Success = true,
            Message = $"Successfully subscribed to {oracleDef.DisplayName}! Cost: {oracleDef.SubscriptionCost} SOL. Remaining balance: {player.SolBalance} SOL",
            Data = player
        });
    }

    public Task<GameResponse<Player>> UnsubscribeFromOracleAsync(string discordUserId, string oracleName)
    {
        if (!_players.TryGetValue(discordUserId, out var player))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Player not found"
            });
        }

        if (!player.SubscribedOracles.Contains(oracleName))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Not subscribed to this oracle"
            });
        }

        player.SubscribedOracles.Remove(oracleName);
        player.OracleSubscriptions.Remove(oracleName);

        return Task.FromResult(new GameResponse<Player>
        {
            Success = true,
            Message = $"Unsubscribed from oracle: {oracleName}",
            Data = player
        });
    }

    public Task<GameResponse<Battle>> CreateBattleAsync(CreateBattleRequest request)
    {
        if (!_players.TryGetValue(request.ChallengerUserId, out var challenger))
        {
            return Task.FromResult(new GameResponse<Battle>
            {
                Success = false,
                Message = "Challenger not found"
            });
        }

        if (!_players.TryGetValue(request.OpponentUserId, out var opponent))
        {
            return Task.FromResult(new GameResponse<Battle>
            {
                Success = false,
                Message = "Opponent not found"
            });
        }

        if (request.ChallengerUserId == request.OpponentUserId)
        {
            return Task.FromResult(new GameResponse<Battle>
            {
                Success = false,
                Message = "Cannot battle yourself"
            });
        }

        if (request.Wager < 1)
        {
            return Task.FromResult(new GameResponse<Battle>
            {
                Success = false,
                Message = "Wager must be at least 1 SOL"
            });
        }

        if (challenger.SolBalance < request.Wager)
        {
            return Task.FromResult(new GameResponse<Battle>
            {
                Success = false,
                Message = $"Insufficient balance. Need {request.Wager} SOL, have {challenger.SolBalance} SOL"
            });
        }

        if (opponent.SolBalance < request.Wager)
        {
            return Task.FromResult(new GameResponse<Battle>
            {
                Success = false,
                Message = $"Opponent has insufficient balance for this wager"
            });
        }

        var battle = new Battle
        {
            Challenger = request.ChallengerUserId,
            Opponent = request.OpponentUserId,
            Wager = request.Wager,
            Status = BattleStatus.Active
        };

        _battles[battle.BattleId] = battle;

        return Task.FromResult(new GameResponse<Battle>
        {
            Success = true,
            Message = $"Battle created! {challenger.Username} vs {opponent.Username} for {request.Wager} SOL",
            Data = battle
        });
    }

    public async Task<GameResponse<Battle>> ExecuteBattleAsync(string battleId)
    {
        if (!_battles.TryGetValue(battleId, out var battle))
        {
            return new GameResponse<Battle>
            {
                Success = false,
                Message = "Battle not found"
            };
        }

        if (battle.Status != BattleStatus.Active)
        {
            return new GameResponse<Battle>
            {
                Success = false,
                Message = "Battle is not active"
            };
        }

        if (!_players.TryGetValue(battle.Challenger, out var challenger) ||
            !_players.TryGetValue(battle.Opponent, out var opponent))
        {
            return new GameResponse<Battle>
            {
                Success = false,
                Message = "Players not found"
            };
        }

        // Calculate scores based on subscribed oracles
        var challengerScore = await CalculatePlayerScore(challenger);
        var opponentScore = await CalculatePlayerScore(opponent);

        battle.ChallengerTotalScore = challengerScore.totalScore;
        battle.OpponentTotalScore = opponentScore.totalScore;
        battle.ChallengerOracleScores = challengerScore.oracleScores;
        battle.OpponentOracleScores = opponentScore.oracleScores;

        // Determine winner
        string winnerId;
        string loserId;

        if (challengerScore.totalScore > opponentScore.totalScore)
        {
            winnerId = battle.Challenger;
            loserId = battle.Opponent;
        }
        else if (opponentScore.totalScore > challengerScore.totalScore)
        {
            winnerId = battle.Opponent;
            loserId = battle.Challenger;
        }
        else
        {
            // Tie - random winner
            winnerId = Random.Shared.Next(2) == 0 ? battle.Challenger : battle.Opponent;
            loserId = winnerId == battle.Challenger ? battle.Opponent : battle.Challenger;
        }

        var winner = _players[winnerId];
        var loser = _players[loserId];

        // Transfer SOL
        winner.SolBalance += battle.Wager;
        loser.SolBalance -= battle.Wager;

        // Update stats
        winner.Wins++;
        loser.Losses++;

        battle.Winner = winnerId;
        battle.Status = BattleStatus.Completed;
        battle.BattleNarrative = GenerateBattleNarrative(battle, challenger, opponent);

        return new GameResponse<Battle>
        {
            Success = true,
            Message = $"{winner.Username} wins {battle.Wager} SOL!",
            Data = battle
        };
    }

    public Task<GameResponse<List<LeaderboardEntry>>> GetLeaderboardAsync(int limit = 10)
    {
        var leaderboard = _players.Values
            .OrderByDescending(p => p.Wins)
            .ThenByDescending(p => p.SolBalance)
            .Take(limit)
            .Select((p, index) => new LeaderboardEntry
            {
                Username = p.Username,
                Wins = p.Wins,
                Losses = p.Losses,
                SolBalance = p.SolBalance,
                Rank = index + 1
            })
            .ToList();

        return Task.FromResult(new GameResponse<List<LeaderboardEntry>>
        {
            Success = true,
            Message = "Leaderboard retrieved",
            Data = leaderboard
        });
    }

    public Task<GameResponse<List<OracleDefinition>>> GetAvailableOraclesAsync()
    {
        return Task.FromResult(new GameResponse<List<OracleDefinition>>
        {
            Success = true,
            Message = "Available oracles",
            Data = OracleRegistry.Oracles
        });
    }

    public Task<GameResponse<Player>> ClaimDailyBonusAsync(string discordUserId)
    {
        if (!_players.TryGetValue(discordUserId, out var player))
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "Player not found"
            });
        }

        var lastClaim = player.LastActive;
        var hoursSinceLastClaim = (DateTime.UtcNow - lastClaim).TotalHours;

        if (hoursSinceLastClaim < 24)
        {
            var hoursRemaining = 24 - hoursSinceLastClaim;
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = $"Daily bonus already claimed. Next bonus in {hoursRemaining:F1} hours"
            });
        }

        // Calculate bonus based on subscribed oracles
        var bonusPerOracle = 2m;
        var totalBonus = player.SubscribedOracles.Count * bonusPerOracle;

        if (totalBonus == 0)
        {
            return Task.FromResult(new GameResponse<Player>
            {
                Success = false,
                Message = "No oracles subscribed. Subscribe to oracles to earn daily bonuses!"
            });
        }

        player.SolBalance += totalBonus;
        player.LastActive = DateTime.UtcNow;

        return Task.FromResult(new GameResponse<Player>
        {
            Success = true,
            Message = $"Daily bonus claimed! Earned {totalBonus} SOL from {player.SubscribedOracles.Count} subscribed oracle(s). New balance: {player.SolBalance} SOL",
            Data = player
        });
    }

    private async Task<(double totalScore, Dictionary<string, double> oracleScores)> CalculatePlayerScore(Player player)
    {
        var oracleScores = new Dictionary<string, double>();
        double totalScore = 0;

        foreach (var oracleName in player.SubscribedOracles)
        {
            var oracleDef = OracleRegistry.Oracles.FirstOrDefault(o => o.Name == oracleName);
            if (oracleDef == null) continue;

            try
            {
                // Find the corresponding oracle implementation
                var oracle = _oracles.FirstOrDefault(o =>
                    o.Name.Equals(oracleDef.DisplayName, StringComparison.OrdinalIgnoreCase));

                if (oracle != null)
                {
                    var result = await oracle.EvaluateAsync(new DataBundle());
                    var score = result.ConfidenceScore * oracleDef.PowerLevel;
                    oracleScores[oracleName] = score;
                    totalScore += score;
                }
                else
                {
                    // Fallback to simulated score if oracle not available
                    var score = Random.Shared.NextDouble() * oracleDef.PowerLevel;
                    oracleScores[oracleName] = score;
                    totalScore += score;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating oracle {OracleName}", oracleName);
                // Use fallback score on error
                var score = Random.Shared.NextDouble() * oracleDef.PowerLevel;
                oracleScores[oracleName] = score;
                totalScore += score;
            }
        }

        // Base score for players without oracles
        if (totalScore == 0)
        {
            totalScore = Random.Shared.NextDouble() * 10;
        }

        return (totalScore, oracleScores);
    }

    private string GenerateBattleNarrative(Battle battle, Player challenger, Player opponent)
    {
        var narratives = new[]
        {
            $"⚔️ Epic Oracle Battle!\n\n" +
            $"{challenger.Username} (Score: {battle.ChallengerTotalScore:F2}) challenged {opponent.Username} (Score: {battle.OpponentTotalScore:F2})\n" +
            $"Wager: {battle.Wager} SOL\n\n" +
            $"The oracles have spoken! {(battle.Winner == battle.Challenger ? challenger.Username : opponent.Username)} emerges victorious!",

            $"🌟 Battle of the Oracles!\n\n" +
            $"In an intense showdown, {challenger.Username} and {opponent.Username} pitted their oracle subscriptions against each other.\n" +
            $"Final Scores - {challenger.Username}: {battle.ChallengerTotalScore:F2} | {opponent.Username}: {battle.OpponentTotalScore:F2}\n" +
            $"Winner takes {battle.Wager} SOL!",

            $"⚡ Oracle Wars Battle Report\n\n" +
            $"Challenger: {challenger.Username} ({challenger.SubscribedOracles.Count} oracles)\n" +
            $"Defender: {opponent.Username} ({opponent.SubscribedOracles.Count} oracles)\n" +
            $"Stakes: {battle.Wager} SOL\n\n" +
            $"Victory goes to {(battle.Winner == battle.Challenger ? challenger.Username : opponent.Username)}!"
        };

        return narratives[Random.Shared.Next(narratives.Length)];
    }
}
