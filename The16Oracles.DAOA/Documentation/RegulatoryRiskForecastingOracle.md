# Oracle documentation:

---

## RegulatoryRiskForecastingOracle

**Implements:** `IAIOracle`

An oracle that polls the NewsAPI for cryptocurrency regulation–related articles across configured regions, detects negative regulatory keywords, computes a weighted negative-article ratio, and emits a regulatory?risk confidence score (–1.0 … 0.0) with per?region metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Articles per Region](#1-fetch-articles-per-region)
   * [2. Count Negative Mentions](#2-count-negative-mentions)
   * [3. Compute Weighted Risk](#3-compute-weighted-risk)
   * [4. Return OracleResult](#4-return-oracleresult)
6. [Private Helpers](#private-helpers)

   * [FetchArticlesAsync](#fetcharticlesasync)
   * [ContainsNegKeyword](#containsnegkeyword)
7. [Supporting Types](#supporting-types)
8. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* A registered `HttpClient` in your dependency-injection container
* NuGet package:

  * `System.Net.Http.Json`

---

## Configuration

Add the following keys under your configuration source (e.g. `appsettings.json`):

```jsonc
{
  "RegulatoryRisk": {
    // API key for NewsAPI.org (https://newsapi.org)
    "NewsApiKey": "YOUR_NEWSAPI_KEY",

    // List of regions to monitor for regulatory news
    "Regions": [
      "United States",
      "European Union",
      "China",
      "Japan",
      "India"
    ]
  }
}
```

| Key                         | Type       | Description                                           |
| --------------------------- | ---------- | ----------------------------------------------------- |
| `RegulatoryRisk:NewsApiKey` | `string`   | Your NewsAPI API key.                                 |
| `RegulatoryRisk:Regions`    | `string[]` | List of region names to include in the risk forecast. |

Missing either setting throws an `InvalidOperationException` at startup.

---

## Constructor

```csharp
public RegulatoryRiskForecastingOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                       |
| ----------------------- | ------------------------------------------------- |
| `HttpClient client`     | Used to fetch articles from the NewsAPI endpoint. |
| `IConfiguration config` | Supplies `NewsApiKey` and `Regions` list.         |

---

## Public Properties

```csharp
public string Name => "Regulatory Risk Forecasting";
```

* **Name**: Identifier for this oracle module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs four main steps:

### 1. Fetch Articles per Region

For each `region` in the configured list:

```csharp
var articles = await FetchArticlesAsync(region);
var total    = articles.Count;
```

* Retrieves up to 100 English news articles from the past 24 hours containing “cryptocurrency regulation” **and** the region name.

### 2. Count Negative Mentions

```csharp
var negative = articles.Count(a => ContainsNegKeyword(a));
perRegionMetrics[region] = (total, negative);
```

* A negative mention is any article whose title or description contains keywords like `"ban"`, `"crackdown"`, `"fine"`, etc.

### 3. Compute Weighted Risk

```csharp
double sumRatios = 0, sumWeights = 0;
foreach (var (region, (tot, neg)) in perRegionMetrics)
{
    var ratio = tot > 0 ? (double)neg / tot : 0.0;
    metrics[$"{region}_ArticleCount"]   = tot;
    metrics[$"{region}_NegativeRatio"]  = Math.Round(ratio, 4);

    sumRatios  += ratio * tot;
    sumWeights += tot;
}

var rawRisk = sumWeights > 0 ? sumRatios / sumWeights : 0.0;
var score   = Math.Clamp(-rawRisk, -1.0, 0.0);
```

* **ratio**: fraction of negative articles in a region.
* **rawRisk**: total negative-article fraction weighted by region article volumes.
* **score**: flipped to \[–1 … 0] so higher risk ? more negative.

### 4. Return OracleResult

```csharp
return new OracleResult
{
    ModuleName      = Name,
    ConfidenceScore = score,
    Metrics         = metrics,
    Timestamp       = DateTime.UtcNow
};
```

* Includes per-region article counts, negative ratios, and overall risk score.

---

## Private Helpers

### FetchArticlesAsync

```csharp
private async Task<List<Article>> FetchArticlesAsync(string region)
```

* Builds a NewsAPI URL for the past 24 hours:

  ```
  https://newsapi.org/v2/everything
    ?q=cryptocurrency+regulation+AND+"{region}"
    &from={ISO_DATE_YESTERDAY}
    &language=en
    &pageSize=100
    &apiKey={_newsApiKey}
  ```
* Deserializes into `NewsApiResponse`, returns its `Articles` list.

### ContainsNegKeyword

```csharp
private static bool ContainsNegKeyword(Article a)
```

* Checks if an article’s `Title` or `Description` (lowercased) contains any keyword from:

  ```csharp
  private static readonly string[] _negKeywords = {
    "ban","restrict","crackdown","regulation",
    "compliance","legislation","tax","fine","penalty"
  };
  ```

---

## Supporting Types

```csharp
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
```

* **NewsApiResponse**: wraps the `"articles"` array from NewsAPI.
* **Article**: holds `Title` and `Description` fields for sentiment checks.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<RegulatoryRiskForecastingOracle>()
       .AddSingleton<IAIOracle, RegulatoryRiskForecastingOracle>();

// When running your oracles:
var oracle = serviceProvider.GetRequiredService<RegulatoryRiskForecastingOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Regulatory Risk Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

---

*End of documentation.*
