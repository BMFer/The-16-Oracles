using Microsoft.Extensions.Options;
using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public class RiskManagementService : IRiskManagementService
{
    private readonly RiskManagementConfiguration _config;
    private readonly ILogger<RiskManagementService> _logger;
    private readonly object _lock = new();

    private decimal _dailyVolume = 0;
    private DateTime _currentDay = DateTime.UtcNow.Date;
    private readonly List<TradeRecord> _todayTrades = new();

    public RiskManagementService(
        IOptions<TradeBotConfiguration> config,
        ILogger<RiskManagementService> logger)
    {
        _config = config.Value.RiskManagement;
        _logger = logger;
    }

    public Task<RiskCheckResult> CheckTradeRiskAsync(
        decimal notionalSol,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            CheckAndResetDailyCounters();

            var violations = new List<string>();
            var result = new RiskCheckResult
            {
                CurrentDailyVolume = _dailyVolume,
                RemainingDailyCapacity = _config.MaxDailyNotionalSol - _dailyVolume
            };

            // Check single trade limit
            if (notionalSol > _config.MaxTradeNotionalSol)
            {
                violations.Add($"Trade notional {notionalSol} SOL exceeds max trade limit {_config.MaxTradeNotionalSol} SOL");
                _logger.LogWarning("Trade rejected: exceeds max trade limit");
            }

            // Check daily volume limit
            if (_dailyVolume + notionalSol > _config.MaxDailyNotionalSol)
            {
                violations.Add($"Trade would exceed daily volume limit. Current: {_dailyVolume} SOL, Limit: {_config.MaxDailyNotionalSol} SOL");
                _logger.LogWarning("Trade rejected: would exceed daily volume limit");
            }

            // Check minimum trade size (0.01 SOL)
            if (notionalSol < 0.01m)
            {
                violations.Add("Trade notional must be at least 0.01 SOL");
                _logger.LogWarning("Trade rejected: below minimum trade size");
            }

            result.Passed = violations.Count == 0;
            result.Violations = violations;

            if (result.Passed)
            {
                _logger.LogInformation(
                    "Risk check passed for {Notional} SOL. Daily volume: {DailyVolume}/{MaxDaily} SOL",
                    notionalSol,
                    _dailyVolume,
                    _config.MaxDailyNotionalSol);
            }

            return Task.FromResult(result);
        }
    }

    public Task RecordTradeAsync(decimal notionalSol, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            CheckAndResetDailyCounters();

            _dailyVolume += notionalSol;
            _todayTrades.Add(new TradeRecord
            {
                NotionalSol = notionalSol,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Trade recorded: {Notional} SOL. Daily volume now: {DailyVolume}/{MaxDaily} SOL",
                notionalSol,
                _dailyVolume,
                _config.MaxDailyNotionalSol);

            return Task.CompletedTask;
        }
    }

    public Task<decimal> GetDailyVolumeAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            CheckAndResetDailyCounters();
            return Task.FromResult(_dailyVolume);
        }
    }

    public Task ResetDailyCountersAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _dailyVolume = 0;
            _todayTrades.Clear();
            _currentDay = DateTime.UtcNow.Date;
            _logger.LogInformation("Daily counters reset");
            return Task.CompletedTask;
        }
    }

    private void CheckAndResetDailyCounters()
    {
        var today = DateTime.UtcNow.Date;
        if (today > _currentDay)
        {
            _logger.LogInformation("New day detected, resetting daily counters");
            _dailyVolume = 0;
            _todayTrades.Clear();
            _currentDay = today;
        }
    }

    private class TradeRecord
    {
        public decimal NotionalSol { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
