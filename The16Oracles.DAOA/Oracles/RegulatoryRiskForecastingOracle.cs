using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class RegulatoryRiskForecastingOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly IConfiguration _config;
    private readonly string _newsApiKey;
    private readonly List<string> _regions;
    private static readonly string[] _negKeywords = new[]
    {
            "ban", "restrict", "crackdown", "regulation",
            "compliance", "legislation", "tax", "fine", "penalty"
        };

    public string Name => "Regulatory Risk Forecasting";

    public RegulatoryRiskForecastingOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
        _newsApiKey = config["RegulatoryRisk:NewsApiKey"]
                       ?? throw new InvalidOperationException("Missing NewsApiKey");
        _regions = config.GetSection("RegulatoryRisk:Regions")
                         .Get<List<string>>()
                   ?? throw new InvalidOperationException("Missing Regions list");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        var perRegionMetrics = new Dictionary<string, (int total, int negative)>();
        foreach (var region in _regions)
        {
            var articles = await FetchArticlesAsync(region);
            var total = articles.Count;
            var negative = articles.Count(a => ContainsNegKeyword(a));
            perRegionMetrics[region] = (total, negative);
        }

        // Build metrics dictionary & compute raw risk
        var metrics = new Dictionary<string, object>();
        double sumRatios = 0, sumWeights = 0;

        foreach (var kv in perRegionMetrics)
        {
            var region = kv.Key;
            var (tot, neg) = kv.Value;
            var ratio = tot > 0 ? (double)neg / tot : 0.0;

            metrics[$"{region}_ArticleCount"] = tot;
            metrics[$"{region}_NegativeRatio"] = Math.Round(ratio, 4);

            // weight by total articles (so busy regions count more)
            sumRatios += ratio * tot;
            sumWeights += tot;
        }

        var rawRisk = sumWeights > 0 ? sumRatios / sumWeights : 0.0;
        // map to confidence in [–1, 0]: higher risk → more negative
        var score = Math.Clamp(-rawRisk, -1.0, 0.0);

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<Article>> FetchArticlesAsync(string region)
    {
        var from = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var url = $"https://newsapi.org/v2/everything" +
                  $"?q=cryptocurrency+regulation+AND+\"{Uri.EscapeDataString(region)}\"" +
                  $"&from={from}&language=en&pageSize=100" +
                  $"&apiKey={_newsApiKey}";

        var resp = await _client.GetFromJsonAsync<NewsApiResponse>(url)
                  ?? throw new InvalidOperationException($"NewsAPI error for {region}");

        return resp.Articles ?? new List<Article>();
    }

    private static bool ContainsNegKeyword(Article a)
    {
        var text = (a.Title + " " + a.Description).ToLowerInvariant();
        return _negKeywords.Any(kw => text.Contains(kw));
    }

    private class NewsApiResponse
    {
        [JsonPropertyName("articles")]
        public List<Article>? Articles { get; set; }
    }

    private class Article
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}

