# Oracle documentation:

---

## TechAdoptionCurvesOracle

**Implements:** `IAIOracle`

An oracle that analyzes the recent Total Value Locked (TVL) history of configured blockchain networks to estimate both growth trends and new?launch activity, then combines these into an adoption?curve confidence score (–1.0 … +1.0) along with detailed metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch & Sort TVL History](#1-fetch--sort-tvl-history)
   * [2. Detect New Launches](#2-detect-new-launches)
   * [3. Compute Growth Rates](#3-compute-growth-rates)
   * [4. Aggregate Metrics](#4-aggregate-metrics)
   * [5. Compute Adoption Index](#5-compute-adoption-index)
   * [6. Return OracleResult](#6-return-oracleresult)
6. [Private Helpers](#private-helpers)
7. [Supporting Types](#supporting-types)
8. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* Registered `HttpClient` in your dependency-injection container
* NuGet package:

  * `System.Net.Http.Json`

*No additional API keys are required; it uses the public DefiLlama charts endpoint.*

---

## Configuration

Add the following section to your configuration (e.g. `appsettings.json`):

```jsonc
{
  "TechAdoption": {
    // List of chain slugs as recognized by DefiLlama
    "Chains": [
      "ethereum",
      "arbitrum",
      "optimism",
      "polygon"
    ]
  }
}
```

| Key                   | Type       | Description                                  |
| --------------------- | ---------- | -------------------------------------------- |
| `TechAdoption:Chains` | `string[]` | List of chain identifiers (DefiLlama slugs). |

Omitting or leaving the list empty will throw an `InvalidOperationException`.

---

## Constructor

```csharp
public TechAdoptionCurvesOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                |
| ----------------------- | ------------------------------------------ |
| `HttpClient client`     | Used to call the DefiLlama charts API.     |
| `IConfiguration config` | Supplies the `"TechAdoption:Chains"` list. |

Throws if the chains list is missing.

---

## Public Properties

```csharp
public string Name => "Tech Adoption Curves";
```

* **Name**: Human-readable identifier for this oracle.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs six main steps:

### 1. Fetch & Sort TVL History

```csharp
var cutoff    = DateTimeOffset.UtcNow
                  .AddDays(-LookbackDays)
                  .ToUnixTimeSeconds();
var history   = await FetchChainChartAsync(chainSlug, LookbackDays);
var sorted    = history.OrderBy(pt => pt.Timestamp).ToList();
var first     = sorted.First();
var last      = sorted.Last();
```

* Retrieves the last `LookbackDays` days of TVL data for each chain.
* Sorts by timestamp to identify the earliest and latest points.

### 2. Detect New Launches

```csharp
if (first.Timestamp >= cutoff)
    newLaunchCount++;
```

* If the earliest data point falls within the lookback window, treats the chain as a “new launch.”

### 3. Compute Growth Rates

```csharp
var growth = first.TotalLiquidityUSD > 0
    ? (last.TotalLiquidityUSD - first.TotalLiquidityUSD)
      / first.TotalLiquidityUSD
    : 0.0;

growthRates.Add(growth);
metrics[$"{chainSlug}_GrowthPct"] = Math.Round(growth, 4);
```

* Calculates the percentage change in TVL over the period.
* Records per-chain growth in `metrics`.

### 4. Aggregate Metrics

```csharp
var avgGrowth      = growthRates.Any() ? growthRates.Average() : 0.0;
var totalChains    = _chains.Count;
var newLaunchRatio = totalChains > 0
    ? (double)newLaunchCount / totalChains
    : 0.0;

metrics["AverageGrowthPct"] = Math.Round(avgGrowth, 4);
metrics["NewLaunchCount"]   = newLaunchCount;
metrics["NewLaunchRatio"]   = Math.Round(newLaunchRatio, 4);
```

* Computes average growth and the fraction of chains that are new launches.

### 5. Compute Adoption Index

```csharp
var growthNorm  = Math.Min(avgGrowth, 1.0);           // cap 100% ? 1.0
var rawIndex    = growthNorm * 0.7 + newLaunchRatio * 0.3;
var confidence  = Math.Clamp(rawIndex * 2 - 1, -1.0, 1.0);
```

* Weights growth at 70% and new-launch ratio at 30%.
* Maps \[0…1] composite to \[–1…+1] confidence.

### 6. Return OracleResult

```csharp
return new OracleResult
{
  ModuleName      = Name,
  ConfidenceScore = confidence,
  Metrics         = metrics,
  Timestamp       = DateTime.UtcNow
};
```

* Returns the composite confidence score and all collected metrics.

---

## Private Helpers

### FetchChainChartAsync

```csharp
private async Task<List<ChartPoint>> FetchChainChartAsync(string slug, int days)
```

* **Endpoint:** `https://api.llama.fi/charts/{slug}?days={days}`
* Returns a JSON array of `{ date: timestamp, totalLiquidityUSD }`.

Throws on failure.

---

## Supporting Types

```csharp
private class ChartPoint
{
    [JsonPropertyName("date")]
    public long   Timestamp         { get; set; }

    [JsonPropertyName("totalLiquidityUSD")]
    public double TotalLiquidityUSD { get; set; }
}
```

* **ChartPoint**: Represents a single TVL data point from DefiLlama.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<TechAdoptionCurvesOracle>()
       .AddSingleton<IAIOracle, TechAdoptionCurvesOracle>();

// Later in your application
var oracle = serviceProvider.GetRequiredService<TechAdoptionCurvesOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Adoption Confidence: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

---

*End of documentation.*
