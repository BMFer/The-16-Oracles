namespace The16Oracles.www.Server.Models;

public class TradingPairConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string StableCoinMint { get; set; } = string.Empty;
    public string TargetTokenMint { get; set; } = string.Empty;
    public int ProfitabilityRank { get; set; }
    public bool Enabled { get; set; } = true;
    public RiskManagementConfiguration RiskManagement { get; set; } = new();
    public decimal CurrentProfitabilityScore { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class CascadeExecutionRequest
{
    public decimal InitialAmountSol { get; set; }
    public List<string>? SpecificPairIds { get; set; }
    public int MaxCascadeDepth { get; set; } = 3;
    public bool StopOnFailure { get; set; } = true;
}

public class CascadeExecutionResponse
{
    public bool Success { get; set; }
    public List<CascadeStepResult> Steps { get; set; } = new();
    public decimal TotalProfitSol { get; set; }
    public decimal FinalAmountSol { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

public class CascadeStepResult
{
    public int StepNumber { get; set; }
    public string TradingPairId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? TransactionSignature { get; set; }
    public TradeDetails? Details { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TradingPairStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public int ProfitabilityRank { get; set; }
    public decimal CurrentProfitabilityScore { get; set; }
    public decimal StableCoinBalance { get; set; }
    public decimal TargetTokenBalance { get; set; }
    public decimal DailyVolumeSol { get; set; }
    public int TradesExecutedToday { get; set; }
    public DateTime? LastTradeAt { get; set; }
}

public class AddTradingPairRequest
{
    public string Id { get; set; } = string.Empty;
    public string StableCoinMint { get; set; } = string.Empty;
    public string TargetTokenMint { get; set; } = string.Empty;
    public int ProfitabilityRank { get; set; }
    public RiskManagementConfiguration? RiskManagement { get; set; }
}

public class UpdateProfitabilityRankRequest
{
    public int NewRank { get; set; }
}
