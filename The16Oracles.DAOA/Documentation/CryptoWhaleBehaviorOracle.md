# Oracle documentation:

---

## CryptoWhaleBehaviorOracle

**Implements:** `IAIOracle`

An oracle that polls Whale Alert for large on-chain transfers over the past 24 hours, computes accumulation vs. distribution flows, detects clustering of sources, then combines these into a whale-behavior confidence score (–1.0 … +1.0) with detailed metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Whale Transactions](#1-fetch-whale-transactions)
   * [2. Compute Core Metrics](#2-compute-core-metrics)
   * [3. Cluster Ratio](#3-cluster-ratio)
   * [4. Normalize & Score](#4-normalize--score)
   * [5. Package Metrics](#5-package-metrics)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** (or later)
* Registered `HttpClient` in your DI container
* NuGet package:

  * `System.Net.Http.Json`

---

## Configuration

Add the following keys under your configuration source (e.g. `appsettings.json`):

```jsonc
{
  "WhaleBehavior": {
    // Whale Alert API key
    "ApiKey": "YOUR_WHALE_ALERT_API_KEY",
    // Minimum USD value per transfer to include
    "MinValueUsd": 1000000,
    // Cap for normalization of total net flow
    "MaxFlowUsdCap": 500000000
  }
}
```

| Key                           | Type   | Description                                                |
| ----------------------------- | ------ | ---------------------------------------------------------- |
| `WhaleBehavior:ApiKey`        | string | API key for Whale Alert.                                   |
| `WhaleBehavior:MinValueUsd`   | double | Minimum USD value threshold to fetch transfers.            |
| `WhaleBehavior:MaxFlowUsdCap` | double | USD cap to normalize net flow (e.g. \$500 M ? score ±1.0). |

Missing the API key will throw on construction; numeric values default to 0 if unset.

---

## Constructor

```csharp
public CryptoWhaleBehaviorOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                          |
| ----------------------- | -------------------------------------------------------------------- |
| `HttpClient client`     | For fetching Whale Alert transactions via HTTP.                      |
| `IConfiguration config` | Supplies `WhaleBehavior:ApiKey`, `MinValueUsd`, and `MaxFlowUsdCap`. |

Throws `InvalidOperationException` if the API key is missing.

---

## Public Properties

```csharp
public string Name => "Crypto Whale Behavior";
```

* **Name**: Identifier for this oracle module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

### 1. Fetch Whale Transactions

```csharp
var now   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
var start = now - 24 * 3600;
var url   = $"https://api.whale-alert.io/v1/transactions" +
            $"?api_key={_apiKey}" +
            $"&min_value={_minValueUsd}" +
            $"&start={start}&end={now}";

var resp = await _client.GetFromJsonAsync<WhaleAlertResponse>(url)
           ?? throw new InvalidOperationException("Failed to fetch whale transactions");
var txs = resp.Transactions;
```

* Retrieves all transfers ? `MinValueUsd` in the last 24 hours.

### 2. Compute Core Metrics

```csharp
var totalCount = txs.Count;
var totalUsd   = txs.Sum(t => t.AmountUsd);

// Accumulation: exchange ? non-exchange
var accumUsd = txs
    .Where(t => t.From.OwnerType == "exchange" && t.To.OwnerType != "exchange")
    .Sum(t => t.AmountUsd);

// Distribution: non-exchange ? exchange
var distUsd = txs
    .Where(t => t.To.OwnerType == "exchange" && t.From.OwnerType != "exchange")
    .Sum(t => t.AmountUsd);

var netFlowUsd = accumUsd - distUsd;
```

* **totalCount**: number of large transfers.
* **totalUsd**: sum of USD value of these transfers.
* **accumUsd**: inflows from exchange wallets.
* **distUsd**: outflows into exchange wallets.
* **netFlowUsd**: accumulation minus distribution.

### 3. Cluster Ratio

```csharp
var uniqueSources = txs.Select(t => t.From.Address).Distinct().Count();
var clusterRatio = totalCount > 0
    ? 1.0 - ((double)uniqueSources / totalCount)
    : 0.0;
```

* Measures concentration of whale sources:

  * **clusterRatio** ? 1.0 when few unique sources send many transactions, ? 0 when all sources are distinct.

### 4. Normalize & Score

```csharp
var netFlowNorm = Math.Clamp(netFlowUsd / _cap, -1.0, 1.0);
var rawScore    = netFlowNorm * clusterRatio;
var score       = Math.Clamp(rawScore, -1.0, 1.0);
```

* **netFlowNorm**: rescaled net flow relative to cap (`MaxFlowUsdCap`).
* **rawScore**: net flow weighted by clustering.
* **score**: final confidence score in \[–1.0 … +1.0].

### 5. Package Metrics

```csharp
var metrics = new Dictionary<string, object>
{
    ["TotalTransfers"]    = totalCount,
    ["TotalVolumeUSD"]    = Math.Round(totalUsd, 2),
    ["AccumulationUSD"]   = Math.Round(accumUsd, 2),
    ["DistributionUSD"]   = Math.Round(distUsd, 2),
    ["NetFlowUSD"]        = Math.Round(netFlowUsd, 2),
    ["DistinctSources"]   = uniqueSources,
    ["ClusterRatio"]      = Math.Round(clusterRatio, 4)
};

return new OracleResult
{
    ModuleName      = Name,
    ConfidenceScore = score,
    Metrics         = metrics,
    Timestamp       = DateTime.UtcNow
};
```

* Returns an `OracleResult` including the computed score and detailed per-field metrics.

---

## Supporting Types

```csharp
private class WhaleAlertResponse
{
    [JsonPropertyName("transactions")]
    public List<Transaction> Transactions { get; set; } = new();
}

private class Transaction
{
    [JsonPropertyName("amount_usd")]
    public double AmountUsd { get; set; }

    [JsonPropertyName("from")]
    public Entity From { get; set; } = new();

    [JsonPropertyName("to")]
    public Entity To { get; set; } = new();
}

private class Entity
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = "";

    [JsonPropertyName("owner_type")]
    public string OwnerType { get; set; } = "";
}
```

* Maps the JSON schema returned by Whale Alert’s `/transactions` endpoint.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<CryptoWhaleBehaviorOracle>()
       .AddSingleton<IAIOracle, CryptoWhaleBehaviorOracle>();

// When executing:
var oracle = serviceProvider.GetRequiredService<CryptoWhaleBehaviorOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Whale Behavior Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
