# Oracle documentation:

---

## NFTMarketSentimentOracle

**Implements:** `IAIOracle`

An oracle that fetches key statistics for configured NFT collections from the OpenSea API, computes a simple sentiment score based on 1-day volume spikes and floor price, and returns an aggregate confidence score (–1.0 … +1.0) along with per-collection metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Collection Stats](#1-fetch-collection-stats)
   * [2. Record Raw Metrics](#2-record-raw-metrics)
   * [3. Compute Normalized Signals](#3-compute-normalized-signals)
   * [4. Compute Raw Sentiment](#4-compute-raw-sentiment)
   * [5. Aggregate Across Collections](#5-aggregate-across-collections)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* A registered `HttpClient` in your DI container
* NuGet package:

  * `System.Net.Http.Json`

*No external API keys are required for basic OpenSea public endpoints.*

---

## Configuration

Add the following to your configuration source (e.g. `appsettings.json`):

```jsonc
{
  "NFTMarketSentiment": {
    // List of OpenSea collection slugs to monitor
    "Collections": [
      "boredapeyachtclub",
      "cryptopunks",
      "artblocks"
    ]
  }
}
```

* **NFTMarketSentiment\:Collections** (`string[]`):
  A list of NFT collection slugs (as used in OpenSea URLs).
* Missing or empty list will throw an `InvalidOperationException`.

---

## Constructor

```csharp
public NFTMarketSentimentOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                           |
| ----------------------- | ----------------------------------------------------- |
| `HttpClient client`     | Used to fetch collection stats from the OpenSea API.  |
| `IConfiguration config` | Supplies the `"NFTMarketSentiment:Collections"` list. |

Throws if the collections list is not present.

---

## Public Properties

```csharp
public string Name => "NFT Market Sentiment";
```

* **Name**: Descriptive identifier for this oracle.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs the following steps:

### 1. Fetch Collection Stats

For each `slug` in the configured collections:

```csharp
var url  = $"https://api.opensea.io/api/v1/collection/{slug}/stats";
var resp = await _client.GetFromJsonAsync<CollectionStatsResponse>(url)
           ?? throw new InvalidOperationException($"Failed to fetch stats for '{slug}'");
var s    = resp.Stats;
```

Retrieves 1-day and 7-day volume and change percentages, plus the current floor price.

### 2. Record Raw Metrics

```csharp
metrics[$"{slug}_OneDayVolume"]    = s.OneDayVolume;
metrics[$"{slug}_OneDayChange"]    = s.OneDayChange;
metrics[$"{slug}_SevenDayVolume"]  = s.SevenDayVolume;
metrics[$"{slug}_SevenDayChange"]  = s.SevenDayChange;
metrics[$"{slug}_FloorPrice"]      = s.FloorPrice;
```

Keeps the raw values for downstream inspection.

### 3. Compute Normalized Signals

* **Volume Spike**: clamp the 1-day change to \[–1 … +1]

  ```csharp
  var volSpike = Math.Clamp(s.OneDayChange, -1.0, 1.0);
  ```
* **Floor-Price Norm**: cap at 1 ETH ? 1.0

  ```csharp
  var floorNorm = Math.Min(s.FloorPrice, 1.0);
  ```

### 4. Compute Raw Sentiment

Combine the two signals:

```csharp
var raw = Math.Clamp(volSpike * floorNorm, -1.0, 1.0);
rawScores.Add(raw);
metrics[$"{slug}_RawSentiment"] = Math.Round(raw, 4);
```

A positive spike combined with a high floor price yields bullish sentiment, negative otherwise.

### 5. Aggregate Across Collections

```csharp
var avgScore = rawScores.Any() ? rawScores.Average() : 0.0;
return new OracleResult
{
  ModuleName      = Name,
  ConfidenceScore = Math.Clamp(avgScore, -1.0, 1.0),
  Metrics         = metrics,
  Timestamp       = DateTime.UtcNow
};
```

Computes the mean of per-collection raw sentiments as the final confidence score.

---

## Supporting Types

```csharp
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
```

* **CollectionStatsResponse**: Wraps the `"stats"` object in the OpenSea response.
* **Stats**: Contains the collection’s volume, change percentages, and floor price.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<NFTMarketSentimentOracle>()
       .AddSingleton<IAIOracle, NFTMarketSentimentOracle>();

// Later, in your application
var oracle = serviceProvider.GetRequiredService<NFTMarketSentimentOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"NFT Market Sentiment: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
