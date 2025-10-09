using The16Oracles.www.Server.Models;
using The16Oracles.www.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Trading Bot
builder.Services.Configure<TradeBotConfiguration>(
    builder.Configuration.GetSection("TradeBot"));

// Register HttpClient for Jupiter API
builder.Services.AddHttpClient<IJupiterApiService, JupiterApiService>();

// Register Trading Bot Services
builder.Services.AddSingleton<IRiskManagementService, RiskManagementService>();
builder.Services.AddSingleton<ISolanaTransactionService, SolanaTransactionService>();
builder.Services.AddSingleton<ITradingBotService, TradingBotService>();
builder.Services.AddSingleton<IProfitabilityAnalyzer, ProfitabilityAnalyzer>();
builder.Services.AddSingleton<ITradingBotOrchestrator, TradingBotOrchestrator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
