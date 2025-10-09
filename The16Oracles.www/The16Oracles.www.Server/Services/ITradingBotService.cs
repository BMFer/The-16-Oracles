using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public interface ITradingBotService
{
    Task<TradeExecutionResponse> ExecuteTradeAsync(TradeExecutionRequest request, CancellationToken cancellationToken = default);
    Task<BotStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
    bool IsEnabled { get; }
}
