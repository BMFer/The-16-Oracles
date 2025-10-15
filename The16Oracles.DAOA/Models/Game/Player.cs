namespace The16Oracles.DAOA.Models.Game;

public class Player
{
    public string DiscordUserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public decimal SolBalance { get; set; } = 100m;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActive { get; set; } = DateTime.UtcNow;
    public List<string> SubscribedOracles { get; set; } = new();
    public Dictionary<string, DateTime> OracleSubscriptions { get; set; } = new();
}
