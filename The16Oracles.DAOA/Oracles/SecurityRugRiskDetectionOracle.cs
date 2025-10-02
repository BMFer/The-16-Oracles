using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class SecurityRugRiskDetectionOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;

    public string Name => "Security/Rug Risk Detection";

    public SecurityRugRiskDetectionOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        var tokens = _config
            .GetSection("RugRisk:TokenAddresses")
            .Get<List<string>>()
            ?? new List<string>();

        var ethKey = _config["RugRisk:Etherscan:ApiKey"]
                     ?? throw new InvalidOperationException("Etherscan API key missing");
        var covKey = _config["RugRisk:Covalent:ApiKey"]
                     ?? throw new InvalidOperationException("Covalent API key missing");

        int unverifiedCount = 0;
        var concentrations = new List<double>();

        foreach (var address in tokens)
        {
            // 1. Etherscan: get source code info
            var srcResp = await _client.GetFromJsonAsync<EtherscanResponse>(
                $"https://api.etherscan.io/api" +
                $"?module=contract&action=getsourcecode" +
                $"&address={address}" +
                $"&apikey={ethKey}"
            );
            bool verified = srcResp?.Result?.FirstOrDefault()?.SourceCode?.Length > 0;
            if (!verified) unverifiedCount++;

            // 2. Covalent: get top-10 holder concentration
            var covUrl = $"https://api.covalenthq.com/v1/1/tokens/{address}/token_holders/?key={covKey}&page-size=10";
            var covResp = await _client.GetFromJsonAsync<CovalentResponse>(covUrl);
            var holders = covResp?.Data?.Items ?? new List<Holder>();
            // sum percentage held by top 2 holders
            var top2 = holders.Take(2).Select(h => h.HolderShare).Sum();
            concentrations.Add(top2);
        }

        // 3. Compute metrics
        var total = tokens.Count;
        var verifiedCount = total - unverifiedCount;
        var avgConcentration = concentrations.Any()
            ? concentrations.Average()
            : 0.0;

        // 4. Raw risk: 50% weight for “unverified” fraction, 50% for holder concentration
        var fracUnverified = total > 0 ? (double)unverifiedCount / total : 0.0;
        var rawRisk = Math.Clamp(fracUnverified * 0.5 + avgConcentration * 0.5, 0.0, 1.0);

        // 5. Map to confidenceScore in [–1,0] (higher risk → more negative)
        var score = Math.Clamp(-rawRisk, -1.0, 0.0);

        var metrics = new Dictionary<string, object>
        {
            ["TotalTokensScanned"] = total,
            ["VerifiedContracts"] = verifiedCount,
            ["UnverifiedContracts"] = unverifiedCount,
            ["AvgTop2HolderConcentration"] = avgConcentration,
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

    // Helper types for Etherscan JSON
    private class EtherscanResponse
    {
        [JsonPropertyName("result")]
        public List<ContractInfo>? Result { get; set; }
    }
    private class ContractInfo
    {
        [JsonPropertyName("SourceCode")]
        public string? SourceCode { get; set; }
    }

    // Helper types for Covalent JSON
    private class CovalentResponse
    {
        [JsonPropertyName("data")]
        public CovalentData? Data { get; set; }
    }
    private class CovalentData
    {
        [JsonPropertyName("items")]
        public List<Holder>? Items { get; set; }
    }
    private class Holder
    {
        [JsonPropertyName("share")]
        public double HolderShare { get; set; }  // e.g. 0.12 for 12%
    }
}

