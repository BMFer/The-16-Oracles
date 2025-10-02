using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class AiNarrativeTrendDetectionOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly string _newsKey;
    private readonly string _twitterToken;
    private static readonly string[] _positiveWords =
        { "gain", "surge", "bull", "up", "rally", "optimistic", "record" };
    private static readonly string[] _negativeWords =
        { "drop", "plunge", "bear", "down", "crash", "fear", "sell-off" };

    public string Name => "AI/Narrative Trend Detection";

    public AiNarrativeTrendDetectionOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _newsKey = config["NarrativeTrend:NewsApiKey"]
                         ?? throw new InvalidOperationException("Missing NewsApiKey");
        _twitterToken = config["NarrativeTrend:TwitterBearerToken"]
                         ?? throw new InvalidOperationException("Missing TwitterBearerToken");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. News sentiment
        var (newsAvg, newsCount) = await FetchNewsSentimentAsync();

        // 2. Social sentiment
        var (socAvg, socCount) = await FetchSocialSentimentAsync();

        // 3. Composite weighted by counts
        var total = newsCount + socCount;
        var composite = total > 0
            ? (newsAvg * newsCount + socAvg * socCount) / total
            : 0.0;

        // 4. Clamp to [–1, +1]
        var score = Math.Clamp(composite, -1.0, 1.0);

        // 5. Metrics
        var metrics = new Dictionary<string, object>
        {
            ["NewsCount"] = newsCount,
            ["NewsSentimentAvg"] = Math.Round(newsAvg, 4),
            ["SocialCount"] = socCount,
            ["SocialSentimentAvg"] = Math.Round(socAvg, 4),
            ["CompositeSentiment"] = Math.Round(composite, 4)
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<(double avg, int count)> FetchNewsSentimentAsync()
    {
        var from = DateTime.UtcNow.AddHours(-24).ToString("yyyy-MM-ddTHH:mm:ss");
        var url = $"https://newsapi.org/v2/everything" +
                   $"?q=crypto OR blockchain OR AI" +
                   $"&from={from}&language=en&pageSize=100" +
                   $"&apiKey={_newsKey}";

        var resp = await _client.GetFromJsonAsync<NewsApiResponse>(url)
                   ?? throw new InvalidOperationException("NewsAPI failure");

        var sentiments = resp.Articles
            .Select(a => AnalyzeSentiment(a.Title + " " + a.Description))
            .ToList();

        return (sentiments.DefaultIfEmpty(0.0).Average(), sentiments.Count);
    }

    private async Task<(double avg, int count)> FetchSocialSentimentAsync()
    {
        // set Bearer token
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _twitterToken);

        var query = Uri.EscapeDataString("crypto blockchain AI -is:retweet lang:en");
        var url = $"https://api.twitter.com/2/tweets/search/recent" +
                    $"?query={query}&max_results=100";

        var resp = await _client.GetFromJsonAsync<TwitterResponse>(url)
                   ?? throw new InvalidOperationException("Twitter API failure");

        var sentiments = resp.Data
            .Select(t => AnalyzeSentiment(t.Text))
            .ToList();

        return (sentiments.DefaultIfEmpty(0.0).Average(), sentiments.Count);
    }

    private double AnalyzeSentiment(string text)
    {
        var words = text
            .ToLowerInvariant()
            .Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var pos = words.Count(w => _positiveWords.Contains(w));
        var neg = words.Count(w => _negativeWords.Contains(w));
        var raw = pos + neg == 0
            ? 0.0
            : (double)(pos - neg) / (pos + neg);
        return Math.Clamp(raw, -1.0, 1.0);
    }

    // NewsAPI response models
    private class NewsApiResponse
    {
        [JsonPropertyName("articles")]
        public List<Article> Articles { get; set; } = new();
    }
    private class Article
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    // Twitter API response models
    private class TwitterResponse
    {
        [JsonPropertyName("data")]
        public List<Tweet> Data { get; set; } = new();
    }
    private class Tweet
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = "";
    }
}