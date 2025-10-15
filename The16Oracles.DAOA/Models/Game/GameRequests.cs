namespace The16Oracles.DAOA.Models.Game;

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

public class GameResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class LeaderboardEntry
{
    public string Username { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public decimal SolBalance { get; set; }
    public double WinRate => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) * 100 : 0;
    public int Rank { get; set; }
}
