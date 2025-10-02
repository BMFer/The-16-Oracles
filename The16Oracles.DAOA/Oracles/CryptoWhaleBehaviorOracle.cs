using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;

public class CryptoWhaleBehaviorOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly string _apiKey;
    private readonly double _minValueUsd;
    private readonly double _cap;

    public string Name => "Crypto Whale Behavior";

    public CryptoWhaleBehaviorOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _apiKey = config["WhaleBehavior:ApiKey"]
                        ?? throw new InvalidOperationException("Missing WhaleBehavior:ApiKey");
        _minValueUsd = config.GetValue<double>("WhaleBehavior:MinValueUsd");
        _cap = config.GetValue<double>("WhaleBehavior:MaxFlowUsdCap");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. Fetch large transfers in last 24h
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var start = now - 24 * 3600;
        var url = $"https://api.whale-alert.io/v1/transactions" +
                  $"?api_key={_apiKey}" +
                  $"&min_value={_minValueUsd}" +
                  $"&start={start}&end={now}";

        var resp = await _client.GetFromJsonAsync<WhaleAlertResponse>(url)
                   ?? throw new InvalidOperationException("Failed to fetch whale transactions");
        var txs = resp.Transactions;

        // 2. Core metrics
        var totalCount = txs.Count;
        var totalUsd = txs.Sum(t => t.AmountUsd);

        var accumUsd = txs
            .Where(t => t.From.OwnerType == "exchange" && t.To.OwnerType != "exchange")
            .Sum(t => t.AmountUsd);
        var distUsd = txs
            .Where(t => t.To.OwnerType == "exchange" && t.From.OwnerType != "exchange")
            .Sum(t => t.AmountUsd);
        var netFlowUsd = accumUsd - distUsd;

        // 3. On-chain clustering: few distinct sources ⇒ high clusterRatio
        var uniqueSources = txs.Select(t => t.From.Address).Distinct().Count();
        var clusterRatio = totalCount > 0
            ? 1.0 - ((double)uniqueSources / totalCount)
            : 0.0;

        // 4. Normalize & score
        var netFlowNorm = Math.Clamp(netFlowUsd / _cap, -1.0, 1.0);
        var rawScore = netFlowNorm * clusterRatio;
        var score = Math.Clamp(rawScore, -1.0, 1.0);

        // 5. Package metrics
        var metrics = new Dictionary<string, object>
        {
            ["TotalTransfers"] = totalCount,
            ["TotalVolumeUSD"] = Math.Round(totalUsd, 2),
            ["AccumulationUSD"] = Math.Round(accumUsd, 2),
            ["DistributionUSD"] = Math.Round(distUsd, 2),
            ["NetFlowUSD"] = Math.Round(netFlowUsd, 2),
            ["DistinctSources"] = uniqueSources,
            ["ClusterRatio"] = Math.Round(clusterRatio, 4)
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    // --- JSON types for Whale Alert ---
    private class WhaleAlertResponse
    {
        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions { get; set; } = new();
    }
    private class Transaction
    {
        [JsonPropertyName("amount_usd")]
        public double AmountUsd { get; set; }

        [JsonPropertyName("from")]
        public Entity From { get; set; } = new();

        [JsonPropertyName("to")]
        public Entity To { get; set; } = new();
    }
    private class Entity
    {
        [JsonPropertyName("address")]
        public string Address { get; set; } = "";

        [JsonPropertyName("owner")]
        public string Owner { get; set; } = "";

        [JsonPropertyName("owner_type")]
        public string OwnerType { get; set; } = "";
    }
}

