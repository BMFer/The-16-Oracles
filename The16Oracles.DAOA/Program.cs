using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Models.Solana;
using The16Oracles.DAOA.Models.SplToken;
using The16Oracles.DAOA.Oracles;
using The16Oracles.DAOA.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register our oracle (and underlying HttpClient)
builder.Services.AddHttpClient<MacroEconomicTrendsOracle>();
builder.Services.AddSingleton<IAIOracle, MacroEconomicTrendsOracle>();

builder.Services.AddHttpClient<DeFiLiquidityMovementsOracle>();
builder.Services.AddSingleton<IAIOracle, DeFiLiquidityMovementsOracle>();

builder.Services.AddHttpClient<BlackSwanDetectionOracle>();
builder.Services.AddSingleton<IAIOracle, BlackSwanDetectionOracle>();

builder.Services.AddHttpClient<EmergingMarketCapitalSurgeOracle>();
builder.Services.AddSingleton<IAIOracle, EmergingMarketCapitalSurgeOracle>();

builder.Services.AddHttpClient<SecurityRugRiskDetectionOracle>();
builder.Services.AddSingleton<IAIOracle, SecurityRugRiskDetectionOracle>();

builder.Services.AddHttpClient<RegulatoryRiskForecastingOracle>();
builder.Services.AddSingleton<IAIOracle, RegulatoryRiskForecastingOracle>();

builder.Services.AddHttpClient<AirdropLaunchOpportunitiesOracle>();
builder.Services.AddSingleton<IAIOracle, AirdropLaunchOpportunitiesOracle>();

builder.Services.AddHttpClient<ChainInteroperabilityMetricsOracle>();
builder.Services.AddSingleton<IAIOracle, ChainInteroperabilityMetricsOracle>();

builder.Services.AddHttpClient<L2ActivityMonitoringOracle>();
builder.Services.AddSingleton<IAIOracle, L2ActivityMonitoringOracle>();

builder.Services.AddHttpClient<NFTMarketSentimentOracle>(client =>
{
    var key = builder.Configuration["NFTMarketSentiment:OpenSeaApiKey"]
              ?? throw new InvalidOperationException("Missing OpenSeaApiKey");
    client.DefaultRequestHeaders.Add("X-API-KEY", key);
});
builder.Services.AddSingleton<IAIOracle, NFTMarketSentimentOracle>();

// register HttpClient and the oracle
builder.Services.AddHttpClient<AiNarrativeTrendDetectionOracle>();
builder.Services.AddSingleton<IAIOracle, AiNarrativeTrendDetectionOracle>();

builder.Services.AddHttpClient<TokenomicsSupplyCurvesOracle>();
builder.Services.AddSingleton<IAIOracle, TokenomicsSupplyCurvesOracle>();

builder.Services.AddHttpClient<CryptoWhaleBehaviorOracle>();
builder.Services.AddSingleton<IAIOracle, CryptoWhaleBehaviorOracle>();

builder.Services.AddHttpClient<NodeValidatorProfitsOracle>();
builder.Services.AddSingleton<IAIOracle, NodeValidatorProfitsOracle>();

builder.Services.AddHttpClient<StablecoinFlowTrackingOracle>();
builder.Services.AddSingleton<IAIOracle, StablecoinFlowTrackingOracle>();

builder.Services.AddHttpClient<TechAdoptionCurvesOracle>();
builder.Services.AddSingleton<IAIOracle, TechAdoptionCurvesOracle>();

// Register Solana service
builder.Services.AddSingleton<ISolanaService, SolanaService>();

// Register SPL Token service
builder.Services.AddSingleton<ISplTokenService, SplTokenService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/oracles/macro-trends", async (MacroEconomicTrendsOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetMacroEconomicsTrends")
.WithTags("DAOA Oracles")
.WithOpenApi();


app.MapGet("/api/oracles/defi-liquidity", async (DeFiLiquidityMovementsOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetDefi-liquidity")
.WithTags("DAOA Oracles")
.WithOpenApi();


app.MapGet("/api/oracles/black-swan", async (BlackSwanDetectionOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetBlack-swan")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/emerging-market-surge", async (EmergingMarketCapitalSurgeOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetEmerging-market-surge")
.WithTags("DAOA Oracles")
.WithOpenApi();

//app.MapGet("/api/oracles/rug-risk", async (SecurityRugRiskDetectionOracle oracle) =>
//{
//    var result = await oracle.EvaluateAsync(new SecurityRugRiskDetection() {  MyProperty = 0     });
//    return Results.Ok(result);
//})
//.WithName("GetRug-risk")
//.WithTags("DAOA Oracles")
//.WithOpenApi();

app.MapGet("/api/oracles/regulatory-risk", async (RegulatoryRiskForecastingOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetRegulatory-risk")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/airdrop-opportunities", async (AirdropLaunchOpportunitiesOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetAirdrop-opportunities")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/chain-interoperability", async (ChainInteroperabilityMetricsOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetChain-interoperability")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/tech-adoption", async (TechAdoptionCurvesOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetTech-adoption")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/node-validator-profits", async (NodeValidatorProfitsOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetNode-validator-profits")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/l2-activity", async (L2ActivityMonitoringOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetL2-activity")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/nft-sentiment", async (NFTMarketSentimentOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetNft-sentiment")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/ai-narrative-trends", async (AiNarrativeTrendDetectionOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetAi-narrative-trends")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/tokenomics-supply", async (TokenomicsSupplyCurvesOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetTokenomics-supply")
.WithTags("DAOA Oracles")
.WithOpenApi();

app.MapGet("/api/oracles/crypto-whale-behavior", async (CryptoWhaleBehaviorOracle oracle) =>
{
    var result = await oracle.EvaluateAsync(new DataBundle());
    return Results.Ok(result);
})
.WithName("GetCrypto-whale-behavior")
.WithTags("DAOA Oracles")
.WithOpenApi();

#region Solana API Endpoints

// Account & Balance Management
app.MapPost("/api/solana/balance", async (ISolanaService solanaService, BalanceRequest request) =>
{
    var result = await solanaService.GetBalanceAsync(request);
    return Results.Ok(result);
})
.WithName("GetSolanaBalance")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/address", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new SolanaCommandRequest
    {
        Command = "address",
        Flags = flags
    };
    var result = await solanaService.ExecuteCommandAsync(request);
    return Results.Ok(result);
})
.WithName("GetSolanaAddress")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapPost("/api/solana/transfer", async (ISolanaService solanaService, TransferRequest request) =>
{
    var result = await solanaService.TransferAsync(request);
    return Results.Ok(result);
})
.WithName("TransferSol")
.WithTags("Solana CLI")
.WithOpenApi();

// Airdrop
app.MapPost("/api/solana/airdrop", async (ISolanaService solanaService, AirdropRequest request) =>
{
    var result = await solanaService.AirdropAsync(request);
    return Results.Ok(result);
})
.WithName("RequestAirdrop")
.WithTags("Solana CLI")
.WithOpenApi();

// Account Information
app.MapPost("/api/solana/account", async (ISolanaService solanaService, AccountRequest request) =>
{
    var result = await solanaService.GetAccountAsync(request);
    return Results.Ok(result);
})
.WithName("GetSolanaAccount")
.WithTags("Solana CLI")
.WithOpenApi();

// Transaction Management
app.MapPost("/api/solana/transaction-history", async (ISolanaService solanaService, TransactionHistoryRequest request) =>
{
    var result = await solanaService.GetTransactionHistoryAsync(request);
    return Results.Ok(result);
})
.WithName("GetTransactionHistory")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapPost("/api/solana/confirm", async (ISolanaService solanaService, ConfirmRequest request) =>
{
    var result = await solanaService.ConfirmTransactionAsync(request);
    return Results.Ok(result);
})
.WithName("ConfirmTransaction")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/transaction-count", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var result = await solanaService.GetTransactionCountAsync(flags);
    return Results.Ok(result);
})
.WithName("GetTransactionCount")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/prioritization-fees", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new PrioritizationFeesRequest { Flags = flags };
    var result = await solanaService.GetPrioritizationFeesAsync(request);
    return Results.Ok(result);
})
.WithName("GetPrioritizationFees")
.WithTags("Solana CLI")
.WithOpenApi();

// Block & Slot Information
app.MapPost("/api/solana/block", async (ISolanaService solanaService, BlockRequest request) =>
{
    var result = await solanaService.GetBlockAsync(request);
    return Results.Ok(result);
})
.WithName("GetBlock")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/block-height", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var result = await solanaService.GetBlockHeightAsync(flags);
    return Results.Ok(result);
})
.WithName("GetBlockHeight")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/slot", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var result = await solanaService.GetSlotAsync(flags);
    return Results.Ok(result);
})
.WithName("GetSlot")
.WithTags("Solana CLI")
.WithOpenApi();

// Epoch Information
app.MapGet("/api/solana/epoch-info", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new EpochInfoRequest { Flags = flags };
    var result = await solanaService.GetEpochInfoAsync(request);
    return Results.Ok(result);
})
.WithName("GetEpochInfo")
.WithTags("Solana CLI")
.WithOpenApi();

// Cluster Information
app.MapGet("/api/solana/cluster-version", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var result = await solanaService.GetClusterVersionAsync(flags);
    return Results.Ok(result);
})
.WithName("GetClusterVersion")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/genesis-hash", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var result = await solanaService.GetGenesisHashAsync(flags);
    return Results.Ok(result);
})
.WithName("GetGenesisHash")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/supply", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new SupplyRequest { Flags = flags };
    var result = await solanaService.GetSupplyAsync(request);
    return Results.Ok(result);
})
.WithName("GetSupply")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/inflation", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new InflationRequest { Flags = flags };
    var result = await solanaService.GetInflationAsync(request);
    return Results.Ok(result);
})
.WithName("GetInflation")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapGet("/api/solana/largest-accounts", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url, [Microsoft.AspNetCore.Mvc.FromQuery] int? limit) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new LargestAccountsRequest { Flags = flags, Limit = limit };
    var result = await solanaService.GetLargestAccountsAsync(request);
    return Results.Ok(result);
})
.WithName("GetLargestAccounts")
.WithTags("Solana CLI")
.WithOpenApi();

// Validator Information
app.MapGet("/api/solana/validators", async (ISolanaService solanaService, [Microsoft.AspNetCore.Mvc.FromQuery] string? url) =>
{
    var flags = new SolanaGlobalFlags { Url = url };
    var request = new ValidatorsRequest { Flags = flags };
    var result = await solanaService.GetValidatorsAsync(request);
    return Results.Ok(result);
})
.WithName("GetValidators")
.WithTags("Solana CLI")
.WithOpenApi();

// Stake Account Management
app.MapPost("/api/solana/stake-account", async (ISolanaService solanaService, StakeAccountRequest request) =>
{
    var result = await solanaService.GetStakeAccountAsync(request);
    return Results.Ok(result);
})
.WithName("GetStakeAccount")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapPost("/api/solana/create-stake-account", async (ISolanaService solanaService, CreateStakeAccountRequest request) =>
{
    var result = await solanaService.CreateStakeAccountAsync(request);
    return Results.Ok(result);
})
.WithName("CreateStakeAccount")
.WithTags("Solana CLI")
.WithOpenApi();

app.MapPost("/api/solana/delegate-stake", async (ISolanaService solanaService, DelegateStakeRequest request) =>
{
    var result = await solanaService.DelegateStakeAsync(request);
    return Results.Ok(result);
})
.WithName("DelegateStake")
.WithTags("Solana CLI")
.WithOpenApi();

// Vote Account Management
app.MapPost("/api/solana/vote-account", async (ISolanaService solanaService, VoteAccountRequest request) =>
{
    var result = await solanaService.GetVoteAccountAsync(request);
    return Results.Ok(result);
})
.WithName("GetVoteAccount")
.WithTags("Solana CLI")
.WithOpenApi();

// Generic command endpoint for advanced usage
app.MapPost("/api/solana/execute", async (ISolanaService solanaService, SolanaCommandRequest request) =>
{
    var result = await solanaService.ExecuteCommandAsync(request);
    return Results.Ok(result);
})
.WithName("ExecuteSolanaCommand")
.WithTags("Solana CLI")
.WithOpenApi();

#endregion

#region SPL Token API Endpoints

// Account Management
app.MapPost("/api/spl-token/accounts", async (ISplTokenService splTokenService, TokenAccountsRequest request) =>
{
    var result = await splTokenService.GetAccountsAsync(request);
    return Results.Ok(result);
})
.WithName("GetTokenAccounts")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/address", async (ISplTokenService splTokenService, TokenAddressRequest request) =>
{
    var result = await splTokenService.GetAddressAsync(request);
    return Results.Ok(result);
})
.WithName("GetTokenAddress")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/balance", async (ISplTokenService splTokenService, TokenBalanceRequest request) =>
{
    var result = await splTokenService.GetBalanceAsync(request);
    return Results.Ok(result);
})
.WithName("GetTokenBalance")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/create-account", async (ISplTokenService splTokenService, CreateTokenAccountRequest request) =>
{
    var result = await splTokenService.CreateAccountAsync(request);
    return Results.Ok(result);
})
.WithName("CreateTokenAccount")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/close", async (ISplTokenService splTokenService, CloseTokenAccountRequest request) =>
{
    var result = await splTokenService.CloseAccountAsync(request);
    return Results.Ok(result);
})
.WithName("CloseTokenAccount")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/gc", async (ISplTokenService splTokenService, GarbageCollectRequest request) =>
{
    var result = await splTokenService.GarbageCollectAsync(request);
    return Results.Ok(result);
})
.WithName("GarbageCollectTokenAccounts")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Token Operations
app.MapPost("/api/spl-token/create-token", async (ISplTokenService splTokenService, CreateTokenRequest request) =>
{
    var result = await splTokenService.CreateTokenAsync(request);
    return Results.Ok(result);
})
.WithName("CreateToken")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/mint", async (ISplTokenService splTokenService, MintTokensRequest request) =>
{
    var result = await splTokenService.MintAsync(request);
    return Results.Ok(result);
})
.WithName("MintTokens")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/burn", async (ISplTokenService splTokenService, BurnTokensRequest request) =>
{
    var result = await splTokenService.BurnAsync(request);
    return Results.Ok(result);
})
.WithName("BurnTokens")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/transfer", async (ISplTokenService splTokenService, TransferTokensRequest request) =>
{
    var result = await splTokenService.TransferAsync(request);
    return Results.Ok(result);
})
.WithName("TransferTokens")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/supply", async (ISplTokenService splTokenService, TokenSupplyRequest request) =>
{
    var result = await splTokenService.GetSupplyAsync(request);
    return Results.Ok(result);
})
.WithName("GetTokenSupply")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/close-mint", async (ISplTokenService splTokenService, CloseMintRequest request) =>
{
    var result = await splTokenService.CloseMintAsync(request);
    return Results.Ok(result);
})
.WithName("CloseTokenMint")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Token Delegation
app.MapPost("/api/spl-token/approve", async (ISplTokenService splTokenService, ApproveRequest request) =>
{
    var result = await splTokenService.ApproveAsync(request);
    return Results.Ok(result);
})
.WithName("ApproveDelegate")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/revoke", async (ISplTokenService splTokenService, RevokeRequest request) =>
{
    var result = await splTokenService.RevokeAsync(request);
    return Results.Ok(result);
})
.WithName("RevokeDelegate")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Token Authority
app.MapPost("/api/spl-token/authorize", async (ISplTokenService splTokenService, AuthorizeRequest request) =>
{
    var result = await splTokenService.AuthorizeAsync(request);
    return Results.Ok(result);
})
.WithName("AuthorizeToken")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Freeze/Thaw Operations
app.MapPost("/api/spl-token/freeze", async (ISplTokenService splTokenService, FreezeAccountRequest request) =>
{
    var result = await splTokenService.FreezeAsync(request);
    return Results.Ok(result);
})
.WithName("FreezeTokenAccount")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/thaw", async (ISplTokenService splTokenService, ThawAccountRequest request) =>
{
    var result = await splTokenService.ThawAsync(request);
    return Results.Ok(result);
})
.WithName("ThawTokenAccount")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Native SOL Wrapping
app.MapPost("/api/spl-token/wrap", async (ISplTokenService splTokenService, WrapRequest request) =>
{
    var result = await splTokenService.WrapAsync(request);
    return Results.Ok(result);
})
.WithName("WrapSol")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/unwrap", async (ISplTokenService splTokenService, UnwrapRequest request) =>
{
    var result = await splTokenService.UnwrapAsync(request);
    return Results.Ok(result);
})
.WithName("UnwrapSol")
.WithTags("SPL Token CLI")
.WithOpenApi();

app.MapPost("/api/spl-token/sync-native", async (ISplTokenService splTokenService, SyncNativeRequest request) =>
{
    var result = await splTokenService.SyncNativeAsync(request);
    return Results.Ok(result);
})
.WithName("SyncNativeAccount")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Display
app.MapPost("/api/spl-token/display", async (ISplTokenService splTokenService, DisplayRequest request) =>
{
    var result = await splTokenService.DisplayAsync(request);
    return Results.Ok(result);
})
.WithName("DisplayTokenInfo")
.WithTags("SPL Token CLI")
.WithOpenApi();

// Generic command endpoint for advanced usage
app.MapPost("/api/spl-token/execute", async (ISplTokenService splTokenService, SplTokenCommandRequest request) =>
{
    var result = await splTokenService.ExecuteCommandAsync(request);
    return Results.Ok(result);
})
.WithName("ExecuteSplTokenCommand")
.WithTags("SPL Token CLI")
.WithOpenApi();

#endregion

#region OOB Example Code

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

#endregion

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
