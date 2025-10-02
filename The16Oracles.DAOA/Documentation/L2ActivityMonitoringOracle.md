# Oracle documentation:

---

## L2ActivityMonitoringOracle

**Implements:** `IAIOracle`

Monitors activity on specified Layer-2 chains by fetching TVL and gas-price data, normalizes these signals, and combines them into a composite activity score (–1.0 … +1.0) along with detailed metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Chain Data](#1-fetch-chain-data)
   * [2. Per-Chain Processing](#2-per-chain-processing)

     * [a. Record Raw Metrics](#a-record-raw-metrics)
     * [b. Normalize TVL Change](#b-normalize-tvl-change)
     * [c. Fetch & Normalize Gas Price](#c-fetch--normalize-gas-price)
   * [3. Aggregate Signals](#3-aggregate-signals)
   * [4. Compute Composite Score](#4-compute-composite-score)
   * [5. Return Result](#5-return-result)
6. [Private Helpers](#private-helpers)
7. [Supporting Types](#supporting-types)
8. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* A registered `HttpClient` in your DI container
* NuGet packages:

  * `System.Net.Http.Json`

---

## Configuration

Add the following to your configuration source (e.g. `appsettings.json`):

```jsonc
{
  "L2Activity": {
    // List of chain slugs to monitor (e.g. "arbitrum", "optimism")
    "Chains": [ "arbitrum", "optimism" ],

    // API key for Arbiscan gas endpoint
    "Arbiscan:ApiKey": "YOUR_ARBISCAN_KEY",

    // API key for Optimism Etherscan gas endpoint
    "OptimisticEtherscan:ApiKey": "YOUR_OPTIMISM_KEY"
  }
}
```

* **Chains** (`string[]`): List of chain identifiers matching the `chain` field from Llama API.
* **Arbiscan\:ApiKey** (`string`): Required for fetching Arbitrum gas prices.
* **OptimisticEtherscan\:ApiKey** (`string`): Required for fetching Optimism gas prices.

Missing any API key will throw an `InvalidOperationException` at startup.

---

## Constructor

```csharp
public L2ActivityMonitoringOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                                    |
| ----------------------- | ------------------------------------------------------------------------------ |
| `HttpClient client`     | Used for all HTTP requests (Llama and gas-oracle APIs).                        |
| `IConfiguration config` | Supplies `L2Activity:Chains`, `Arbiscan:ApiKey`, `OptimisticEtherscan:ApiKey`. |

---

## Public Properties

```csharp
public string Name => "L2 Activity Monitoring";
```

* **Name**: Human-readable identifier for the module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs the monitoring workflow in five steps:

### 1. Fetch Chain Data

```csharp
var chains = await _client.GetFromJsonAsync<List<ChainInfo>>(
    "https://api.llama.fi/chains")
    ?? throw new InvalidOperationException("Failed to fetch chains");
```

Retrieves a list of supported chains with their TVL and 1d/7d changes.

### 2. Per-Chain Processing

Loop over each configured slug in `L2Activity:Chains`:

#### a. Record Raw Metrics

```csharp
metrics[$"{slug}_TVL_USD"]    = info.Tvl;
metrics[$"{slug}_TVLChange1d"] = info.Change1d;
metrics[$"{slug}_TVLChange7d"] = info.Change7d;
```

* **Tvl**: Total value locked (USD).
* **Change1d**, **Change7d**: 1-day and 7-day TVL percentage changes.

#### b. Normalize TVL Change

```csharp
var changeNorm = Math.Clamp(info.Change1d / 0.05, -1.0, 1.0);
sumChangeNorm += changeNorm;
```

Maps a ±5 % daily change to ±1.0.

#### c. Fetch & Normalize Gas Price

```csharp
var gas = await FetchGasPriceAsync(slug);
metrics[$"{slug}_GasPrice_Gwei"] = gas;

var gasNorm = Math.Min(gas / 50.0, 1.0);
sumGasNorm += gasNorm;
```

* Fetches average gas price (Safe/Propose/Fast) via Arbiscan or Optimism API.
* Maps 50 Gwei to 1.0 (cap).

### 3. Aggregate Signals

```csharp
var avgChange = count > 0 ? sumChangeNorm / count : 0.0;
var avgGas    = count > 0 ? sumGasNorm    / count : 0.0;

metrics["AverageChange1dNorm"] = Math.Round(avgChange, 4);
metrics["AverageGasNorm"]      = Math.Round(avgGas, 4);
```

Computes the mean normalized change and gas signals across all monitored chains.

### 4. Compute Composite Score

```csharp
var rawScore = avgChange * 0.6 + avgGas * 0.4;
var score    = Math.Clamp(rawScore, -1.0, 1.0);
```

Weights TVL change at 60% and gas usage at 40%, then clamps to \[–1.0 … +1.0].

### 5. Return Result

```csharp
return new OracleResult
{
  ModuleName      = Name,
  ConfidenceScore = score,
  Metrics         = metrics,
  Timestamp       = DateTime.UtcNow
};
```

Packages the composite score and all collected metrics.

---

## Private Helpers

### FetchGasPriceAsync

```csharp
private async Task<double> FetchGasPriceAsync(string slug)
```

1. Determines the appropriate URL based on `slug`:

   * `"arbitrum"` ? Arbiscan gas-oracle endpoint
   * `"optimism"` ? Optimistic Etherscan gas-oracle endpoint
2. GET ? `GasOracleResponse`
3. Parses `SafeGasPrice`, `ProposeGasPrice`, `FastGasPrice` (strings) to `double` and returns their average.

Throws `InvalidOperationException` on failure.

---

## Supporting Types

```csharp
private class ChainInfo
{
  [JsonPropertyName("chain")]
  public string Chain { get; set; } = "";

  [JsonPropertyName("tvl")]
  public double Tvl { get; set; }

  [JsonPropertyName("change_1d")]
  public double Change1d { get; set; }

  [JsonPropertyName("change_7d")]
  public double Change7d { get; set; }
}

private class GasOracleResponse
{
  [JsonPropertyName("result")]
  public GasResult Result { get; set; } = new();
}

private class GasResult
{
  [JsonPropertyName("SafeGasPrice")]
  public string SafeGasPrice { get; set; } = "0";

  [JsonPropertyName("ProposeGasPrice")]
  public string ProposeGasPrice { get; set; } = "0";

  [JsonPropertyName("FastGasPrice")]
  public string FastGasPrice { get; set; } = "0";
}
```

* **ChainInfo**: Maps Llama API chain entries.
* **GasOracleResponse/GasResult**: Maps Arbiscan/Etherscan gas-oracle JSON.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<L2ActivityMonitoringOracle>()
       .AddSingleton<IAIOracle, L2ActivityMonitoringOracle>();

// Somewhere in your application
var oracle = serviceProvider.GetRequiredService<L2ActivityMonitoringOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"L2 Activity Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

---

*End of documentation.*
