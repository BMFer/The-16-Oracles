using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Oracles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
