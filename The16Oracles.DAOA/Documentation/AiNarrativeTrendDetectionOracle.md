# Oracle documentation:

---

## AiNarrativeTrendDetectionOracle

**Implements:** `IAIOracle`

An oracle that fetches and analyzes sentiment from both news headlines and recent tweets about “crypto”, “blockchain” or “AI”, computes a composite sentiment score (?1.0 to +1.0), and returns it along with raw metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)
6. [Private Helpers](#private-helpers)
7. [Supporting Types](#supporting-types)
8. [Example Usage](#example-usage)

---

## Prerequisites

* .NET 6.0 (or later)
* A registered `HttpClient` instance
* An `IConfiguration` provider (e.g. `appsettings.json`)
* NuGet packages:

  * `System.Net.Http.Json`
  * `System.Text.Json`

---

## Configuration

Add the following keys under your configuration source:

```jsonc
{
  "NarrativeTrend": {
    // Your NewsAPI.org API key
    "NewsApiKey": "YOUR_NEWSAPI_KEY_HERE",

    // Your Twitter API Bearer token
    "TwitterBearerToken": "YOUR_TWITTER_BEARER_TOKEN_HERE"
  }
}
```

Both values are required; omission will throw `InvalidOperationException` on startup.

---

## Constructor

```csharp
public AiNarrativeTrendDetectionOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                                   |
| ----------------------- | ----------------------------------------------------------------------------- |
| `HttpClient client`     | Used for both NewsAPI and Twitter API HTTP requests.                          |
| `IConfiguration config` | Supplies `NarrativeTrend:NewsApiKey` and `NarrativeTrend:TwitterBearerToken`. |

Throws if either API key is missing.

---

## Public Properties

```csharp
public string Name => "AI/Narrative Trend Detection";
```

* **Name**: Identifier for the oracle module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

1. **Fetch News Sentiment**
   Calls `FetchNewsSentimentAsync()` to pull up to 100 English news articles from the last 24 hours matching “crypto OR blockchain OR AI”, analyzes each title+description, and returns `(avgSentiment, count)`.

2. **Fetch Social Sentiment**
   Calls `FetchSocialSentimentAsync()` to pull up to 100 recent English tweets (excluding retweets) matching the same keywords, analyzes each tweet’s text, and returns `(avgSentiment, count)`.

3. **Composite Score**

   ```csharp
   var total = newsCount + socCount;
   var composite = total > 0
       ? (newsAvg * newsCount + socAvg * socCount) / total
       : 0.0;
   var score = Math.Clamp(composite, -1.0, +1.0);
   ```

   A weighted average of the two channels, clamped to \[?1.0, +1.0].

4. **Metrics**

   ```csharp
   var metrics = new Dictionary<string, object> {
     ["NewsCount"]            = newsCount,
     ["NewsSentimentAvg"]     = Math.Round(newsAvg, 4),
     ["SocialCount"]          = socCount,
     ["SocialSentimentAvg"]   = Math.Round(socAvg, 4),
     ["CompositeSentiment"]   = Math.Round(composite, 4)
   };
   ```

5. **Return**

   ```csharp
   return new OracleResult {
     ModuleName      = Name,
     ConfidenceScore = score,
     Metrics         = metrics,
     Timestamp       = DateTime.UtcNow
   };
   ```

---

## Private Helpers

### FetchNewsSentimentAsync

```csharp
private async Task<(double avg, int count)> FetchNewsSentimentAsync()
```

* Builds a NewsAPI URL for the last 24 hours:

  ```csharp
  var from = DateTime.UtcNow.AddHours(-24)
                 .ToString("yyyy-MM-ddTHH:mm:ss");
  var url = $"https://newsapi.org/v2/everything" +
            $"?q=crypto OR blockchain OR AI" +
            $"&from={from}&language=en&pageSize=100" +
            $"&apiKey={_newsKey}";
  ```
* Deserializes into `NewsApiResponse`, analyzes each article via `AnalyzeSentiment`, and returns the average and count.

### FetchSocialSentimentAsync

```csharp
private async Task<(double avg, int count)> FetchSocialSentimentAsync()
```

* Sets Twitter Bearer token on `HttpClient`.
* Queries Twitter v2 Recent Search:

  ```csharp
  var query = Uri.EscapeDataString("crypto blockchain AI -is:retweet lang:en");
  var url   = $"https://api.twitter.com/2/tweets/search/recent" +
              $"?query={query}&max_results=100";
  ```
* Deserializes into `TwitterResponse`, analyzes each tweet’s text, and returns the average and count.

### AnalyzeSentiment

```csharp
private double AnalyzeSentiment(string text)
```

* Splits `text` into words, counts occurrences of predefined positive vs. negative keywords:

  ```csharp
  private static readonly string[] _positiveWords = {
    "gain","surge","bull","up","rally","optimistic","record"
  };
  private static readonly string[] _negativeWords = {
    "drop","plunge","bear","down","crash","fear","sell-off"
  };
  ```
* Returns `(pos - neg)/(pos + neg)`, or 0 if neither appears, clamped to \[?1, +1].

---

## Supporting Types

```csharp
// for NewsAPI
private class NewsApiResponse {
  [JsonPropertyName("articles")]
  public List<Article> Articles { get; set; } = new();
}
private class Article {
  [JsonPropertyName("title")]
  public string Title { get; set; } = "";
  [JsonPropertyName("description")]
  public string Description { get; set; } = "";
}

// for Twitter API
private class TwitterResponse {
  [JsonPropertyName("data")]
  public List<Tweet> Data { get; set; } = new();
}
private class Tweet {
  [JsonPropertyName("text")]
  public string Text { get; set; } = "";
}
```

These private models mirror the JSON structures returned by each external API.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<AiNarrativeTrendDetectionOracle>()
       .AddSingleton<IAIOracle, AiNarrativeTrendDetectionOracle>();

// Later, when you want to run the oracle:
var oracle = serviceProvider.GetRequiredService<AiNarrativeTrendDetectionOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Composite Sentiment: {result.Metrics["CompositeSentiment"]}");
Console.WriteLine($"News Articles Analyzed: {result.Metrics["NewsCount"]}");
Console.WriteLine($"Tweets Analyzed: {result.Metrics["SocialCount"]}");
```

---
