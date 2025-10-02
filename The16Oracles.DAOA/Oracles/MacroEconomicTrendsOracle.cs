
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class MacroEconomicTrendsOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly string _fredApiKey;
    public string Name => "Macro Economics Trends";

    public MacroEconomicTrendsOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _fredApiKey = config["FRED:ApiKey"]
                      ?? throw new InvalidOperationException("FRED:ApiKey missing.");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // Fetch the latest observations
        var rate10Y = await FetchLatestValueAsync("DGS10");
        var cpi = await FetchLatestValueAsync("CPIAUCSL");
        var gdp = await FetchLatestValueAsync("GDP");

        // Basic scoring: positive GDP and low inflation → bullish
        var score = ComputeScore(rate10Y, cpi, gdp);

        var metrics = new Dictionary<string, object>
        {
            ["InterestRate10Y"] = rate10Y,
            ["CPI"] = cpi,
            ["GDP"] = gdp
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }
    private async Task<double> FetchLatestValueAsync(string seriesId)
    {
        // Ask for just the newest observation by sorting descending
        var url = $"https://api.stlouisfed.org/fred/series/observations" +
                  $"?series_id={seriesId}" +
                  $"&api_key={_fredApiKey}" +
                  $"&file_type=json" +
                  $"&limit=1" +
                  $"&sort_order=desc";

        var response = await _client.GetFromJsonAsync<FredResponse>(url)
                       ?? throw new InvalidOperationException("Invalid FRED response");

        // There should be exactly one item; if it's non-numeric (e.g. "."), treat as missing
        var raw = response.Observations.FirstOrDefault()?.Value;
        if (string.IsNullOrEmpty(raw) || raw == "." || !double.TryParse(raw, out var v))
            throw new InvalidOperationException($"No valid data for series '{seriesId}'");

        return v;
    }

    private double ComputeScore(double rate, double cpi, double gdp)
    {
        // very simple heuristic:
        //   - encourage buys when GDP high
        //   - discourage when rates high or inflation high
        var inflationPct = (cpi - 100) / 100.0;
        var gdpNorm = gdp / 10000.0;
        var rateNorm = rate / 100.0;

        var raw = gdpNorm - rateNorm - inflationPct;
        return Math.Clamp(raw, -1.0, 1.0);
    }

    // helper types for JSON parsing
    private class FredResponse
    {
        public Observation[] Observations { get; set; } = Array.Empty<Observation>();
    }
    private class Observation
    {
        public string Date { get; set; } = "";
        public string Value { get; set; } = "";
    }
}

