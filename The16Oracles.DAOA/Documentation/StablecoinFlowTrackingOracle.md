# Oracle documentation:

---

## StablecoinFlowTrackingOracle

**Implements:** `IAIOracle`

An oracle that fetches recent market data for a configured list of stablecoins from CoinGecko, computes per?coin “flow” scores based on issuance/redemption rates and peg stability, then returns an overall confidence score and detailed metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Data Fetching](#1-data-fetching)
   * [2. Per-Coin Scoring](#2-per-coin-scoring)
   * [3. Aggregation](#3-aggregation)
   * [4. Result Payload](#4-result-payload)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* .NET 6.0 (or later)
* `HttpClient` registered in DI
* `IConfiguration` provider (e.g. `appsettings.json`)
* NuGet package:

  * `System.Net.Http.Json`

---

## Configuration

Add the following section to your configuration source:

```jsonc
{
  "StablecoinFlow": {
    // List of stablecoin IDs as recognized by CoinGecko
    "Stablecoins": [
      "tether",
      "usd-coin",
      "dai"
    ]
  }
}
```

* **StablecoinFlow\:Stablecoins** (`string[]`):
  A list of CoinGecko coin IDs for which to track flow metrics.
* Missing or empty list will throw an `InvalidOperationException`.

---

## Constructor

```csharp
public StablecoinFlowTrackingOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                               |
| ----------------------- | ------------------------------------------------------------------------- |
| `HttpClient client`     | Used to fetch market data from CoinGecko.                                 |
| `IConfiguration config` | Provides the `"StablecoinFlow:Stablecoins"` list. Throws if missing/null. |

---

## Public Properties

```csharp
public string Name => "Stablecoin Flow Tracking";
```

* **Name**: Human-readable module identifier.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

### 1. Data Fetching

* Builds a comma-separated list of coin IDs:

  ```csharp
  var ids = string.Join(',', _coins);
  ```
* Constructs CoinGecko markets URL:

  ```
  https://api.coingecko.com/api/v3/coins/markets
    ?vs_currency=usd
    &ids={ids}
    &order=market_cap_desc
    &per_page=100
    &page=1
    &sparkline=false
  ```
* Fetches JSON into `List<CoinMarket>`; throws `InvalidOperationException` on failure.

### 2. Per-Coin Scoring

For each `coin` in the returned data:

1. **Issuance/Redemption %**

   ```csharp
   var issuancePct = coin.MarketCap > 0
       ? coin.MarketCapChange24h / coin.MarketCap
       : 0.0;
   ```
2. **Peg Deviation** (from \$1)

   ```csharp
   var pegDev = Math.Abs(coin.CurrentPrice - 1.0);
   ```
3. **Normalization**

   * Issuance: ±5% ? ±1.0

     ```csharp
     var issuanceNorm = Math.Clamp(issuancePct / 0.05, -1.0, 1.0);
     ```
   * Peg: 1% deviation ? 1.0 (worst)

     ```csharp
     var pegDevNorm = Math.Clamp(pegDev / 0.01, 0.0, 1.0);
     ```
4. **Raw Score**
   Positive issuance only counts when peg is stable:

   ```csharp
   var raw = issuanceNorm * (1 - pegDevNorm);
   ```
5. **Record Metrics**

   ```csharp
   metrics[$"{coin.Id}_IssuancePct"]   = Math.Round(issuancePct, 4);
   metrics[$"{coin.Id}_PegDeviation"]  = Math.Round(pegDev, 4);
   metrics[$"{coin.Id}_RawScore"]      = Math.Round(raw, 4);
   rawScores.Add(raw);
   ```

### 3. Aggregation

* **Average Score** across all coins (or 0 if none):

  ```csharp
  var avgScore = rawScores.Any() ? rawScores.Average() : 0.0;
  ```
* **Clamped Confidence**:

  ```csharp
  var confidence = Math.Clamp(avgScore, -1.0, 1.0);
  ```

### 4. Result Payload

Returns an `OracleResult`:

| Field             | Description                                                      |
| ----------------- | ---------------------------------------------------------------- |
| `ModuleName`      | `"Stablecoin Flow Tracking"`                                     |
| `ConfidenceScore` | Clamped average raw score (–1.0 … +1.0)                          |
| `Metrics`         | Detailed per-coin metrics (issuance %, peg deviation, raw score) |
| `Timestamp`       | `DateTime.UtcNow`                                                |

---

## Supporting Types

```csharp
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
```

* **CoinMarket**: Mirrors the JSON fields returned by the CoinGecko `/coins/markets` endpoint.

---

## Example Usage

```csharp
// In Program.cs (or Startup.cs)
builder.Services
    .AddHttpClient<StablecoinFlowTrackingOracle>()
    .AddSingleton<IAIOracle, StablecoinFlowTrackingOracle>();

// When you want to evaluate:
var oracle = serviceProvider.GetRequiredService<StablecoinFlowTrackingOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Stablecoin Flow Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
