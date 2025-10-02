using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class L2ActivityMonitoringOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly List<string> _slugs;
    private readonly string _arbKey;
    private readonly string _optiKey;

    public string Name => "L2 Activity Monitoring";

    public L2ActivityMonitoringOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _slugs = config.GetSection("L2Activity:Chains").Get<List<string>>()
                    ?? new List<string>();
        _arbKey = config["L2Activity:Arbiscan:ApiKey"]
                    ?? throw new InvalidOperationException("Missing Arbiscan key");
        _optiKey = config["L2Activity:OptimisticEtherscan:ApiKey"]
                    ?? throw new InvalidOperationException("Missing Optimism key");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. Fetch all chains data
        var chains = await _client.GetFromJsonAsync<List<ChainInfo>>(
            "https://api.llama.fi/chains")
            ?? throw new InvalidOperationException("Failed to fetch chains");

        double sumChangeNorm = 0, sumGasNorm = 0;
        int count = 0;
        var metrics = new Dictionary<string, object>();

        foreach (var slug in _slugs)
        {
            var info = chains
                .FirstOrDefault(c => c.Chain.Equals(slug, StringComparison.OrdinalIgnoreCase));
            if (info == null) continue;

            // Raw metrics
            metrics[$"{slug}_TVL_USD"] = info.Tvl;
            metrics[$"{slug}_TVLChange1d"] = info.Change1d;
            metrics[$"{slug}_TVLChange7d"] = info.Change7d;

            // Normalize 1d change: ±5% → ±1.0
            var changeNorm = Math.Clamp(info.Change1d / 0.05, -1.0, 1.0);
            sumChangeNorm += changeNorm;

            // Fetch gas price
            var gas = await FetchGasPriceAsync(slug);
            metrics[$"{slug}_GasPrice_Gwei"] = gas;

            // Normalize gas: 50 Gwei → 1.0
            var gasNorm = Math.Min(gas / 50.0, 1.0);
            sumGasNorm += gasNorm;

            count++;
        }

        // 2. Aggregate normalized signals
        var avgChange = count > 0 ? sumChangeNorm / count : 0.0;
        var avgGas = count > 0 ? sumGasNorm / count : 0.0;

        metrics["AverageChange1dNorm"] = Math.Round(avgChange, 4);
        metrics["AverageGasNorm"] = Math.Round(avgGas, 4);

        // 3. Composite score: 60% TVL-change, 40% gas usage
        var rawScore = avgChange * 0.6 + avgGas * 0.4;
        var score = Math.Clamp(rawScore, -1.0, 1.0);

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<double> FetchGasPriceAsync(string slug)
    {
        string url = slug switch
        {
            "arbitrum" => $"https://api.arbiscan.io/api?module=gastracker&action=gasoracle&apikey={_arbKey}",
            "optimism" => $"https://api-optimistic.etherscan.io/api?module=gastracker&action=gasoracle&apikey={_optiKey}",
            _ => null
        };
        if (url == null) return 0.0;

        var resp = await _client.GetFromJsonAsync<GasOracleResponse>(url)
                   ?? throw new InvalidOperationException($"Gas API failed for {slug}");
        // average Safe, Proposed, Fast
        var r = resp.Result;
        var prices = new[] {
                double.Parse(r.SafeGasPrice),
                double.Parse(r.ProposeGasPrice),
                double.Parse(r.FastGasPrice)
            };
        return prices.Average();
    }

    private class ChainInfo
    {
        [JsonPropertyName("chain")]
        public string Chain { get; set; } = "";
        [JsonPropertyName("tvl")]
        public double Tvl { get; set; }
        [JsonPropertyName("change_1d")]
        public double Change1d { get; set; }
        [JsonPropertyName("change_7d")]
        public double Change7d { get; set; }
    }

    private class GasOracleResponse
    {
        [JsonPropertyName("result")]
        public GasResult Result { get; set; } = new();
    }
    private class GasResult
    {
        [JsonPropertyName("SafeGasPrice")]
        public string SafeGasPrice { get; set; } = "0";
        [JsonPropertyName("ProposeGasPrice")]
        public string ProposeGasPrice { get; set; } = "0";
        [JsonPropertyName("FastGasPrice")]
        public string FastGasPrice { get; set; } = "0";
    }
}

