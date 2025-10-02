using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;

public class ChainInteroperabilityMetricsOracle : IAIOracle
{
    private readonly HttpClient _client;
    public string Name => "Chain Interoperability Metrics";

    public ChainInteroperabilityMetricsOracle(HttpClient client)
    {
        _client = client;
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. Fetch raw bridge data
        var bridges = await _client.GetFromJsonAsync<List<BridgeInfo>>(
            "https://bridges.llama.fi/bridges")
            ?? throw new InvalidOperationException("Failed to fetch bridge data");

        // 2. Total 24h USD flow & swap count
        var totalFlowUsd = bridges.Sum(b => b.OneDayVolume);
        var totalSwaps = bridges.Sum(b => b.TxCountOneDay);

        // 3. Per-chain flow (evenly split across each bridge's chains)
        var chainFlows = new Dictionary<string, double>();
        foreach (var b in bridges)
        {
            if (b.Chains?.Count > 0)
            {
                var perChain = b.OneDayVolume / b.Chains.Count;
                foreach (var chain in b.Chains)
                {
                    if (!chainFlows.ContainsKey(chain))
                        chainFlows[chain] = 0;
                    chainFlows[chain] += perChain;
                }
            }
        }

        // 4. Normalize metrics and compute confidence score
        var flowNorm = Math.Min(totalFlowUsd / 1_000_000_000.0, 1.0);   // cap at $1B
        var swapsNorm = Math.Min(totalSwaps / 100_000.0, 1.0);   // cap at 100k txs
        var rawScore = (flowNorm + swapsNorm) / 2.0;                     // [0…1]
        var score = Math.Clamp(rawScore * 2 - 1, -1.0, 1.0);         // map to [–1…+1]

        // 5. Prepare metrics
        var metrics = new Dictionary<string, object>
        {
            ["TotalBridgeFlow24h_USD"] = totalFlowUsd,
            ["TotalCrossChainSwaps24h"] = totalSwaps,
            ["ConfidenceScore"] = score
        };
        // include top 5 chains by flow
        foreach (var kv in chainFlows
                            .OrderByDescending(kv => kv.Value)
                            .Take(5))
        {
            metrics[$"{kv.Key}_Flow24h_USD"] = Math.Round(kv.Value, 2);
        }

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private class BridgeInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("chains")]
        public List<string>? Chains { get; set; }

        [JsonPropertyName("oneDayVolume")]
        public double OneDayVolume { get; set; }

        [JsonPropertyName("txCountOneDay")]
        public int TxCountOneDay { get; set; }
    }
}
