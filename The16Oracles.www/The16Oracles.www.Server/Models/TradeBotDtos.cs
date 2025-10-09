namespace The16Oracles.www.Server.Models;

public class JupiterQuoteRequest
{
    public string InputMint { get; set; } = string.Empty;
    public string OutputMint { get; set; } = string.Empty;
    public long Amount { get; set; }
    public int SlippageBps { get; set; }
}

public class JupiterQuoteResponse
{
    public string InputMint { get; set; } = string.Empty;
    public string InAmount { get; set; } = string.Empty;
    public string OutputMint { get; set; } = string.Empty;
    public string OutAmount { get; set; } = string.Empty;
    public string OtherAmountThreshold { get; set; } = string.Empty;
    public string SwapMode { get; set; } = string.Empty;
    public int SlippageBps { get; set; }
    public PlatformFee? PlatformFee { get; set; }
    public decimal PriceImpactPct { get; set; }
    public List<RoutePlanStep> RoutePlan { get; set; } = new();
}

public class PlatformFee
{
    public string Amount { get; set; } = string.Empty;
    public int FeeBps { get; set; }
}

public class RoutePlanStep
{
    public SwapInfo SwapInfo { get; set; } = new();
    public decimal Percent { get; set; }
}

public class SwapInfo
{
    public string AmmKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string InputMint { get; set; } = string.Empty;
    public string OutputMint { get; set; } = string.Empty;
    public string InAmount { get; set; } = string.Empty;
    public string OutAmount { get; set; } = string.Empty;
    public string FeeAmount { get; set; } = string.Empty;
    public string FeeMint { get; set; } = string.Empty;
}

public class JupiterSwapRequest
{
    public JupiterQuoteResponse QuoteResponse { get; set; } = new();
    public string UserPublicKey { get; set; } = string.Empty;
    public bool WrapAndUnwrapSol { get; set; } = true;
}

public class JupiterSwapResponse
{
    public string SwapTransaction { get; set; } = string.Empty;
    public string LastValidBlockHeight { get; set; } = string.Empty;
}

public class TradeExecutionRequest
{
    public TradeDirection Direction { get; set; }
    public decimal AmountSol { get; set; }
}

public enum TradeDirection
{
    SolToToken,
    TokenToSol
}

public class TradeExecutionResponse
{
    public bool Success { get; set; }
    public string? TransactionSignature { get; set; }
    public string? ErrorMessage { get; set; }
    public TradeDetails? Details { get; set; }
}

public class TradeDetails
{
    public string InputMint { get; set; } = string.Empty;
    public string OutputMint { get; set; } = string.Empty;
    public decimal InputAmount { get; set; }
    public decimal OutputAmount { get; set; }
    public decimal PriceImpactPct { get; set; }
    public DateTime ExecutedAt { get; set; }
}

public class BotStatusResponse
{
    public bool IsRunning { get; set; }
    public bool IsEnabled { get; set; }
    public decimal CurrentSolBalance { get; set; }
    public decimal CurrentTokenBalance { get; set; }
    public decimal DailyVolumeSol { get; set; }
    public int TradesExecutedToday { get; set; }
    public DateTime? LastTradeAt { get; set; }
}

public class RiskCheckResult
{
    public bool Passed { get; set; }
    public List<string> Violations { get; set; } = new();
    public decimal CurrentDailyVolume { get; set; }
    public decimal RemainingDailyCapacity { get; set; }
}
