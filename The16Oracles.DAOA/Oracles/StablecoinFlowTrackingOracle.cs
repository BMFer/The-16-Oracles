using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class StablecoinFlowTrackingOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly List<string> _coins;
    public string Name => "Stablecoin Flow Tracking";

    public StablecoinFlowTrackingOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _coins = config
            .GetSection("StablecoinFlow:Stablecoins")
            .Get<List<string>>()
            ?? throw new InvalidOperationException("Missing StablecoinFlow:Stablecoins");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // fetch market data for all stablecoins
        var ids = string.Join(',', _coins);
        var url = $"https://api.coingecko.com/api/v3/coins/markets" +
                  $"?vs_currency=usd&ids={ids}&order=market_cap_desc" +
                  $"&per_page=100&page=1&sparkline=false";
        var data = await _client.GetFromJsonAsync<List<CoinMarket>>(url)
                   ?? throw new InvalidOperationException("Failed to fetch stablecoin data");

        var rawScores = new List<double>();
        var metrics = new Dictionary<string, object>();

        foreach (var coin in data)
        {
            // 1. Estimate issuance/redemption % over last 24h
            var issuancePct = coin.MarketCap > 0
                ? coin.MarketCapChange24h / coin.MarketCap
                : 0.0;

            // 2. Peg deviation (abs price dev from $1)
            var pegDev = Math.Abs(coin.CurrentPrice - 1.0);

            // 3. Normalize: ±5% issuance→±1.0, 1% peg dev→1.0
            var issuanceNorm = Math.Clamp(issuancePct / 0.05, -1.0, 1.0);
            var pegDevNorm = Math.Clamp(pegDev / 0.01, 0.0, 1.0);

            // 4. Raw score: issuances positive when peg stable
            var raw = issuanceNorm * (1 - pegDevNorm);
            rawScores.Add(raw);

            // record per-coin metrics
            metrics[$"{coin.Id}_IssuancePct"] = Math.Round(issuancePct, 4);
            metrics[$"{coin.Id}_PegDeviation"] = Math.Round(pegDev, 4);
            metrics[$"{coin.Id}_RawScore"] = Math.Round(raw, 4);
        }

        // 5. Average across coins
        var avgScore = rawScores.Any()
            ? rawScores.Average()
            : 0.0;

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = Math.Clamp(avgScore, -1.0, 1.0),
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private class CoinMarket
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("current_price")]
        public double CurrentPrice { get; set; }

        [JsonPropertyName("market_cap")]
        public double MarketCap { get; set; }

        [JsonPropertyName("market_cap_change_24h")]
        public double MarketCapChange24h { get; set; }
    }
}

