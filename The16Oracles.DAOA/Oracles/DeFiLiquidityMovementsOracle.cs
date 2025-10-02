using Newtonsoft.Json;
using System.Collections.Generic;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;

public class DeFiLiquidityMovementsOracle : IAIOracle
{
    private readonly HttpClient _client;
    public string Name => "DeFi Liquidity Movements";

    public DeFiLiquidityMovementsOracle(HttpClient client)
        => _client = client;

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        var (latestTvl, previousTvl) = await FetchTvlHistoryAsync();
        var tvlShiftPct = previousTvl > 0
            ? (latestTvl - previousTvl) / previousTvl
            : 0.0;

        var lendingTvl = await FetchLendingTvlAsync();
        var lendingRatio = latestTvl > 0
            ? lendingTvl / latestTvl
            : 0.0;

        var rawScore = tvlShiftPct * lendingRatio;
        var score = Math.Clamp(rawScore, -1.0, 1.0);

        var metrics = new Dictionary<string, object>
        {
            ["TotalTVL_USD"] = latestTvl,
            ["TVLShift1d_Pct"] = tvlShiftPct,
            ["LendingTVL_USD"] = lendingTvl,
            ["LendingRatio"] = lendingRatio
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<(double latest, double previous)> FetchTvlHistoryAsync()
    {
        var history = await _client.GetFromJsonAsync<List<TvlPoint>>(
            "https://api.llama.fi/charts")
            ?? throw new InvalidOperationException("Failed to fetch TVL history");

        if (history.Count < 2)
            throw new InvalidOperationException("Not enough TVL data points");

        var sorted = history.OrderBy(h => h.Date).ToList();
        return (sorted[^1].TotalLiquidityUSD, sorted[^2].TotalLiquidityUSD);
    }

    private async Task<double> FetchLendingTvlAsync()
    {
        var p = await _client.GetStringAsync("https://api.llama.fi/protocols");
        var j = JsonConvert.DeserializeObject<List<Protocol>>(p);
        var protocols = await _client.GetFromJsonAsync<List<Protocol>>(
            "https://api.llama.fi/protocols")
            ?? throw new InvalidOperationException("Failed to fetch protocols");

        return
        j
            .Where(p => p.Category.Equals("Lending", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Tvl ?? 0);
    }

    private class TvlPoint
    {
        public long? Date { get; set; }
        public double TotalLiquidityUSD { get; set; }
    }

    private class Protocol
    {
        public string? Name { get; set; } = "";
        public string? Category { get; set; } = "";
        public double? Tvl { get; set; }
    }
}

