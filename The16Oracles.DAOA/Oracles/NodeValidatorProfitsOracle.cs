using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class NodeValidatorProfitsOracle : IAIOracle
{
    private readonly HttpClient _client;
    public string Name => "Node/Validator Profits";

    // Cosmos REST endpoints
    private const string InflationUrl = "https://api.cosmos.network/cosmos/mint/v1beta1/inflation";
    private const string ValidatorsUrl = "https://api.cosmos.network/cosmos/staking/v1beta1/validators?status=BOND_STATUS_BONDED&pagination.limit=1000";

    public NodeValidatorProfitsOracle(HttpClient client)
        => _client = client;

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. Fetch annual inflation rate (as decimal, e.g. "0.1234" → 12.34%)
        var inflResp = await _client.GetFromJsonAsync<InflationResponse>(InflationUrl)
                       ?? throw new InvalidOperationException("Failed to fetch inflation");
        var inflation = double.Parse(inflResp.Inflation);

        // 2. Fetch all bonded validators and compute average commission rate
        var valResp = await _client.GetFromJsonAsync<ValidatorsResponse>(ValidatorsUrl)
                      ?? throw new InvalidOperationException("Failed to fetch validators");
        var rates = valResp.Validators
                    .Select(v => double.Parse(v.Commission.CommissionRates.Rate))
                    .ToList();
        var avgCommission = rates.Any() ? rates.Average() : 0.0;

        // 3. Effective staking yield after commission
        var effectiveYield = inflation * (1 - avgCommission);

        // 4. Normalize & map to confidenceScore ∈ [–1, +1]
        var normYield = Math.Min(effectiveYield, 1.0);               // cap at 100%
        var rawScore = normYield * 2 - 1;                          // [0→1]→[–1→+1]
        var score = Math.Clamp(rawScore, -1.0, 1.0);

        // 5. Package metrics
        var metrics = new Dictionary<string, object>
        {
            ["AnnualInflationRate"] = Math.Round(inflation, 6),        // e.g. 0.123456
            ["AvgCommissionRate"] = Math.Round(avgCommission, 6),
            ["EffectiveYield"] = Math.Round(effectiveYield, 6)
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private class InflationResponse
    {
        [JsonPropertyName("inflation")]
        public string Inflation { get; set; } = "0";
    }

    private class ValidatorsResponse
    {
        [JsonPropertyName("validators")]
        public List<Validator> Validators { get; set; } = new();
    }
    private class Validator
    {
        [JsonPropertyName("commission")]
        public Commission Commission { get; set; } = new();
    }
    private class Commission
    {
        [JsonPropertyName("commission_rates")]
        public CommissionRates CommissionRates { get; set; } = new();
    }
    private class CommissionRates
    {
        [JsonPropertyName("rate")]
        public string Rate { get; set; } = "0";
    }
}

