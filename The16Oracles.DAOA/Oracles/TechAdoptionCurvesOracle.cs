using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class TechAdoptionCurvesOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly List<string> _chains;
    private const int LookbackDays = 30;

    public string Name => "Tech Adoption Curves";

    public TechAdoptionCurvesOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _chains = config.GetSection("TechAdoption:Chains")
                        .Get<List<string>>()
                  ?? throw new InvalidOperationException("Missing TechAdoption:Chains");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-LookbackDays).ToUnixTimeSeconds();
        var growthRates = new List<double>();
        int newLaunchCount = 0;
        var metrics = new Dictionary<string, object>();

        foreach (var chainSlug in _chains)
        {
            var history = await FetchChainChartAsync(chainSlug, LookbackDays);
            if (history.Count < 2) continue;

            var sorted = history.OrderBy(pt => pt.Timestamp).ToList();
            var first = sorted.First();
            var last = sorted.Last();

            // detect "new launch" if first data point is within the lookback window
            if (first.Timestamp >= cutoff)
                newLaunchCount++;

            // compute percent growth over the period
            var growth = first.TotalLiquidityUSD > 0
                ? (last.TotalLiquidityUSD - first.TotalLiquidityUSD) / first.TotalLiquidityUSD
                : 0.0;
            growthRates.Add(growth);
            metrics[$"{chainSlug}_GrowthPct"] = Math.Round(growth, 4);
        }

        var avgGrowth = growthRates.Any() ? growthRates.Average() : 0.0;
        var totalChains = _chains.Count;
        var newLaunchRatio = totalChains > 0
            ? (double)newLaunchCount / totalChains
            : 0.0;

        metrics["AverageGrowthPct"] = Math.Round(avgGrowth, 4);
        metrics["NewLaunchCount"] = newLaunchCount;
        metrics["NewLaunchRatio"] = Math.Round(newLaunchRatio, 4);

        // normalize: assume 100% growth → 1.0
        var growthNorm = Math.Min(avgGrowth, 1.0);

        // composite adoption index: 70% weight on growth, 30% on new launches
        var rawIndex = growthNorm * 0.7 + newLaunchRatio * 0.3;

        // map [0…1] → [–1…+1] (positive adoption → positive confidence)
        var confidenceScore = Math.Clamp(rawIndex * 2 - 1, -1.0, 1.0);

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = confidenceScore,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<ChartPoint>> FetchChainChartAsync(string slug, int days)
    {
        // DefiLlama charts endpoint for chain TVL history (last 'days' days) :contentReference[oaicite:0]{index=0}
        var url = $"https://api.llama.fi/charts/{slug}?days={days}";
        return await _client.GetFromJsonAsync<List<ChartPoint>>(url)
               ?? throw new InvalidOperationException($"Failed fetching chart for '{slug}'");
    }

    private class ChartPoint
    {
        [JsonPropertyName("date")]
        public long Timestamp { get; set; }

        [JsonPropertyName("totalLiquidityUSD")]
        public double TotalLiquidityUSD { get; set; }
    }
}

