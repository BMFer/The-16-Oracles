# Oracle documentation:

---

## BlackSwanDetectionOracle

**Implements:** `IAIOracle`

An oracle that fetches Bitcoin price history from CoinGecko, computes realized volatility and tail?risk (VaR95), detects volatility spikes (“anomalies”), and returns an early?warning confidence score (–1.0 … 0.0) plus detailed metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Constructor](#constructor)
3. [Public Properties](#public-properties)
4. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Price History](#1-fetch-price-history)
   * [2. Compute Log Returns](#2-compute-log-returns)
   * [3. Realized Volatility](#3-realized-volatility)
   * [4. Volatility Anomaly](#4-volatility-anomaly)
   * [5. Value?at?Risk (VaR95)](#5-value-at-risk-var95)
   * [6. Normalization & Risk Score](#6-normalization--risk-score)
   * [7. Metrics & Result](#7-metrics--result)
5. [Private Helpers](#private-helpers)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* A registered `HttpClient` (can be default or named)
* NuGet package:

  * `System.Net.Http.Json`

*No external configuration keys are required; the CoinGecko endpoints are hardcoded.*

---

## Constructor

```csharp
public BlackSwanDetectionOracle(HttpClient client)
```

| Parameter           | Description                                         |
| ------------------- | --------------------------------------------------- |
| `HttpClient client` | Used to fetch Bitcoin market charts from CoinGecko. |

---

## Public Properties

```csharp
public string Name => "Black Swan Detection & Early Warning";
```

* **Name**: Human?readable identifier for this oracle.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

This is the core routine, broken down into seven steps:

#### 1. Fetch Price History

```csharp
var chart7d  = await FetchMarketChartAsync(7);
var chart30d = await FetchMarketChartAsync(30);
```

* Pulls 7-day and 30-day BTC/USD price series from
  `https://api.coingecko.com/api/v3/coins/bitcoin/market_chart`

#### 2. Compute Log Returns

```csharp
var returns7d  = ComputeLogReturns(chart7d.Prices);
var returns30d = ComputeLogReturns(chart30d.Prices);
```

* Calculates logarithmic returns between successive price points.

#### 3. Realized Volatility

```csharp
var vol7d  = StdDev(returns7d) * Math.Sqrt(returns7d.Count);
var vol30d = StdDev(returns30d) * Math.Sqrt(returns30d.Count);
```

* Scales the standard deviation of log returns by ?N to annualize-like for each window.

#### 4. Volatility Anomaly

```csharp
var ratio      = vol30d > 0 ? vol7d / vol30d : 1.0;
var anomalyPct = Math.Clamp(ratio - 1.0, 0.0, 1.0);
```

* Compares recent (7d) vs. longer?term (30d) volatility; any excess is treated as an “anomaly” \[0…1].

#### 5. Value?at?Risk (VaR95)

```csharp
var sorted = returns7d.OrderBy(r => r).ToList();
var idx    = (int)Math.Floor(0.05 * sorted.Count);
var var95  = -sorted[idx];  // positive tail?loss at 95% confidence
```

* Identifies the 5th?percentile worst return over 7 days and flips sign to yield a positive loss value.

#### 6. Normalization & Risk Score

```csharp
var varNorm = Math.Min(var95 / 0.05, 1.0);
var rawRisk = anomalyPct * varNorm;
var score   = Math.Clamp(-rawRisk, -1.0, 0.0);
```

* Scales VaR95 so that a 5% loss ? 1.0, multiplies by volatility anomaly, then inverts for a negative confidence score.

#### 7. Metrics & Result

```csharp
var metrics = new Dictionary<string, object>
{
  ["RealizedVolatility7d"]  = vol7d,
  ["RealizedVolatility30d"] = vol30d,
  ["VolatilityRatio"]       = ratio,
  ["VaR95"]                 = var95,
  ["RawRiskIndex"]          = rawRisk
};

return new OracleResult
{
  ModuleName      = Name,
  ConfidenceScore = score,
  Metrics         = metrics,
  Timestamp       = DateTime.UtcNow
};
```

* Returns an `OracleResult` carrying:

  * **ConfidenceScore**: Negative risk warning \[–1…0]
  * **Metrics**: Detailed numeric values for downstream analysis
  * **Timestamp**: When the evaluation completed

---

## Private Helpers

### FetchMarketChartAsync

```csharp
private async Task<MarketChart> FetchMarketChartAsync(int days)
```

* Hits CoinGecko `/market_chart?vs_currency=usd&days={days}`
* Deserializes into `MarketChart`; throws on failure.

### ComputeLogReturns

```csharp
private List<double> ComputeLogReturns(List<PricePoint> prices)
```

* Iterates price points, computing `log(curr/prev)` for each adjacent pair.

### StdDev

```csharp
private double StdDev(List<double> data)
```

* Computes the population standard deviation:

  $$
    \sigma = \sqrt{\frac{1}{N}\sum (x_i - \bar x)^2}
  $$

---

## Supporting Types

```csharp
private class MarketChart
{
  [JsonPropertyName("prices")]
  public List<List<double>> RawPrices { get; set; } = new();

  [JsonIgnore]
  public List<PricePoint> Prices =>
    RawPrices.Select(p => new PricePoint {
      Timestamp = p[0],
      Price     = p[1]
    }).ToList();
}

private class PricePoint
{
  public double Timestamp { get; set; }
  public double Price     { get; set; }
}
```

* **MarketChart**: Binds raw JSON `[ [timestamp, price], … ]` into a list of `PricePoint`.
* **PricePoint**: Simple struct for a single data point.

---

## Example Usage

```csharp
// Program.cs or Startup.cs
builder.Services
       .AddHttpClient<BlackSwanDetectionOracle>()
       .AddSingleton<IAIOracle, BlackSwanDetectionOracle>();

// Later in your code
var oracle = serviceProvider.GetRequiredService<BlackSwanDetectionOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Warning Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

---

*End of documentation.*
