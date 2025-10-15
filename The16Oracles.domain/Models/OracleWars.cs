namespace The16Oracles.domain.Models
{
    public class GameResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class Player
    {
        public string DiscordUserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public decimal SolBalance { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public List<string> SubscribedOracles { get; set; } = new();
    }

    public class OracleDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal SubscriptionCost { get; set; }
        public int PowerLevel { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class Battle
    {
        public string BattleId { get; set; } = string.Empty;
        public string Challenger { get; set; } = string.Empty;
        public string Opponent { get; set; } = string.Empty;
        public decimal Wager { get; set; }
        public string? Winner { get; set; }
        public string Status { get; set; } = string.Empty;
        public double ChallengerTotalScore { get; set; }
        public double OpponentTotalScore { get; set; }
        public string? BattleNarrative { get; set; }
    }

    public class LeaderboardEntry
    {
        public string Username { get; set; } = string.Empty;
        public int Wins { get; set; }
        public int Losses { get; set; }
        public decimal SolBalance { get; set; }
        public double WinRate { get; set; }
        public int Rank { get; set; }
    }

    public class CreatePlayerRequest
    {
        public string DiscordUserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class SubscribeOracleRequest
    {
        public string DiscordUserId { get; set; } = string.Empty;
        public string OracleName { get; set; } = string.Empty;
    }

    public class CreateBattleRequest
    {
        public string ChallengerUserId { get; set; } = string.Empty;
        public string OpponentUserId { get; set; } = string.Empty;
        public decimal Wager { get; set; }
    }
}
