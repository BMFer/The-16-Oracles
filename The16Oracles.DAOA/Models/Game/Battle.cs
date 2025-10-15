namespace The16Oracles.DAOA.Models.Game;

public class Battle
{
    public string BattleId { get; set; } = Guid.NewGuid().ToString();
    public string Challenger { get; set; } = string.Empty;
    public string Opponent { get; set; } = string.Empty;
    public decimal Wager { get; set; }
    public string? Winner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public BattleStatus Status { get; set; } = BattleStatus.Pending;
    public Dictionary<string, double> ChallengerOracleScores { get; set; } = new();
    public Dictionary<string, double> OpponentOracleScores { get; set; } = new();
    public double ChallengerTotalScore { get; set; }
    public double OpponentTotalScore { get; set; }
    public string? BattleNarrative { get; set; }
}

public enum BattleStatus
{
    Pending,
    Active,
    Completed,
    Cancelled
}
