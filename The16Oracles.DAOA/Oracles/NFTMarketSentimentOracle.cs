using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class NFTMarketSentimentOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly List<string> _slugs;
    public string Name => "NFT Market Sentiment";

    public NFTMarketSentimentOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _slugs = config
          .GetSection("NFTMarketSentiment:Collections")
          .Get<List<string>>()
          ?? throw new InvalidOperationException("Missing Collections list");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        var rawScores = new List<double>();
        var metrics = new Dictionary<string, object>();

        foreach (var slug in _slugs)
        {
            // 1. Fetch collection stats :contentReference[oaicite:1]{index=1}
            var url = $"https://api.opensea.io/api/v1/collection/{slug}/stats";
            var resp = await _client.GetFromJsonAsync<CollectionStatsResponse>(url)
                       ?? throw new InvalidOperationException($"Failed to fetch stats for '{slug}'");

            var s = resp.Stats;
            // record raw metrics
            metrics[$"{slug}_OneDayVolume"] = s.OneDayVolume;
            metrics[$"{slug}_OneDayChange"] = s.OneDayChange;
            metrics[$"{slug}_SevenDayVolume"] = s.SevenDayVolume;
            metrics[$"{slug}_SevenDayChange"] = s.SevenDayChange;
            metrics[$"{slug}_FloorPrice"] = s.FloorPrice;

            // 2. Compute normalized signals
            // Volume spike is the API’s one_day_change (e.g. 0.25 = +25%) :contentReference[oaicite:2]{index=2}
            var volSpike = Math.Clamp(s.OneDayChange, -1.0, 1.0);
            // Floor‐price norm: cap at 1 ETH → 1.0
            var floorNorm = Math.Min(s.FloorPrice, 1.0);

            // 3. Raw sentiment = spike × floor‐price norm
            var raw = Math.Clamp(volSpike * floorNorm, -1.0, 1.0);
            rawScores.Add(raw);
            metrics[$"{slug}_RawSentiment"] = Math.Round(raw, 4);
        }

        // 4. Aggregate across collections
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

    private class CollectionStatsResponse
    {
        [JsonPropertyName("stats")]
        public Stats Stats { get; set; } = new();
    }

    private class Stats
    {
        [JsonPropertyName("one_day_volume")]
        public double OneDayVolume { get; set; }

        [JsonPropertyName("one_day_change")]
        public double OneDayChange { get; set; }

        [JsonPropertyName("seven_day_volume")]
        public double SevenDayVolume { get; set; }

        [JsonPropertyName("seven_day_change")]
        public double SevenDayChange { get; set; }

        [JsonPropertyName("floor_price")]
        public double FloorPrice { get; set; }
    }
}

