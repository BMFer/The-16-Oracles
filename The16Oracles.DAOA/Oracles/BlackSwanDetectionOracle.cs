using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class BlackSwanDetectionOracle : IAIOracle
{
    private readonly HttpClient _client;
    public string Name => "Black Swan Detection & Early Warning";

    public BlackSwanDetectionOracle(HttpClient client)
        => _client = client;

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. Fetch price history
        var chart7d = await FetchMarketChartAsync(7);
        var chart30d = await FetchMarketChartAsync(30);

        // 2. Compute log returns
        var returns7d = ComputeLogReturns(chart7d.Prices);
        var returns30d = ComputeLogReturns(chart30d.Prices);

        // 3. Realized vol (simple scaling)
        var vol7d = StdDev(returns7d) * Math.Sqrt(returns7d.Count);
        var vol30d = StdDev(returns30d) * Math.Sqrt(returns30d.Count);

        // 4. Volatility ratio anomaly
        var ratio = vol30d > 0 ? vol7d / vol30d : 1.0;
        var anomalyPct = Math.Clamp(ratio - 1.0, 0.0, 1.0);

        // 5. 95% Value-at-Risk (VaR95)
        var sorted = returns7d.OrderBy(r => r).ToList();
        var idx = (int)Math.Floor(0.05 * sorted.Count);
        var var95 = -sorted[idx];   // positive number

        // 6. Normalize VaR95 (assume 5% worst loss as baseline)
        var varNorm = Math.Min(var95 / 0.05, 1.0);

        // 7. Combine into risk score (0…1), then invert for confidence (–1…0)
        var rawRisk = anomalyPct * varNorm;
        var score = Math.Clamp(-rawRisk, -1.0, 0.0);

        // 8. Package metrics
        var metrics = new Dictionary<string, object>
        {
            ["RealizedVolatility7d"] = vol7d,
            ["RealizedVolatility30d"] = vol30d,
            ["VolatilityRatio"] = ratio,
            ["VaR95"] = var95,
            ["RawRiskIndex"] = rawRisk
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<MarketChart> FetchMarketChartAsync(int days)
    {
        var url = $"https://api.coingecko.com/api/v3/coins/bitcoin/market_chart" +
                  $"?vs_currency=usd&days={days}";
        return await _client.GetFromJsonAsync<MarketChart>(url)
               ?? throw new InvalidOperationException($"Failed to fetch {days}d chart");
    }

    private List<double> ComputeLogReturns(List<PricePoint> prices)
    {
        var returns = new List<double>();
        for (int i = 1; i < prices.Count; i++)
        {
            var prev = prices[i - 1].Price;
            var curr = prices[i].Price;
            if (prev > 0)
                returns.Add(Math.Log(curr / prev));
        }
        return returns;
    }

    private double StdDev(List<double> data)
    {
        var mean = data.Average();
        var sumSq = data.Sum(d => (d - mean) * (d - mean));
        return Math.Sqrt(sumSq / data.Count);
    }

    // Helper types for JSON mapping

    private class MarketChart
    {
        /// <summary>
        /// Raw JSON binding for the "prices" array.
        /// </summary>
        [JsonPropertyName("prices")]
        public List<List<double>> RawPrices { get; set; } = new();

        /// <summary>
        /// Computed list of timestamp/price points.
        /// Ignored by the JSON serializer to avoid collisions.
        /// </summary>
        [JsonIgnore]
        public List<PricePoint> Prices =>
            RawPrices
                .Select(p => new PricePoint { Timestamp = p[0], Price = p[1] })
                .ToList();
    }

    private class PricePoint
    {
        public double Timestamp { get; set; }
        public double Price { get; set; }
    }
}

