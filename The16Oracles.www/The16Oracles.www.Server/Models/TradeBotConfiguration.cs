namespace The16Oracles.www.Server.Models;

public class TradeBotConfiguration
{
    public SolanaConfiguration Solana { get; set; } = new();
    public RiskManagementConfiguration RiskManagement { get; set; } = new();
    public TradingConfiguration Trading { get; set; } = new();
}

public class SolanaConfiguration
{
    public string RpcUrl { get; set; } = string.Empty;
    public string BotPrivateKey { get; set; } = string.Empty;
    public string TokenMint { get; set; } = string.Empty;
    public string SolMint { get; set; } = "So11111111111111111111111111111111111111112";
}

public class RiskManagementConfiguration
{
    public decimal MaxTradeNotionalSol { get; set; } = 50m;
    public decimal MaxDailyNotionalSol { get; set; } = 500m;
    public int SlippageBps { get; set; } = 30;
    public decimal MinBalanceSol { get; set; } = 0.1m;
}

public class TradingConfiguration
{
    public string JupiterApiUrl { get; set; } = "https://quote-api.jup.ag/v6";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = false;
}
