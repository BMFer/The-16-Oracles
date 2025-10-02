using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;
using The16Oracles.DAOA.Oracles;

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
