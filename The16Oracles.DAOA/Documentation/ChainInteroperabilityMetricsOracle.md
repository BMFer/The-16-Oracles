# Oracle documentation:

---

## ChainInteroperabilityMetricsOracle

**Implements:** `IAIOracle`

An oracle that fetches cross-chain bridge activity from the Llama API, computes total and per-chain 24 h volume and swap counts, normalizes these metrics into a confidence score (–1.0 … +1.0), and returns detailed statistics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Constructor](#constructor)
3. [Public Properties](#public-properties)
4. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Raw Bridge Data](#1-fetch-raw-bridge-data)
   * [2. Aggregate Total Flow & Swaps](#2-aggregate-total-flow--swaps)
   * [3. Compute Per-Chain Flow](#3-compute-per-chain-flow)
   * [4. Normalize & Score](#4-normalize--score)
   * [5. Prepare Metrics & Return](#5-prepare-metrics--return)
5. [Supporting Types](#supporting-types)
6. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** (or later)
* A registered `HttpClient` instance in your DI container
* NuGet package:

  * `System.Net.Http.Json`

*No external configuration values required; the Llama API endpoint is hardcoded.*

---

## Constructor

```csharp
public ChainInteroperabilityMetricsOracle(HttpClient client)
```

| Parameter           | Description                                        |
| ------------------- | -------------------------------------------------- |
| `HttpClient client` | Used to fetch bridge data from `bridges.llama.fi`. |

---

## Public Properties

```csharp
public string Name => "Chain Interoperability Metrics";
```

* **Name**: Human-readable identifier for the oracle module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

This method performs five main steps:

### 1. Fetch Raw Bridge Data

```csharp
var bridges = await _client.GetFromJsonAsync<List<BridgeInfo>>(
    "https://bridges.llama.fi/bridges")
    ?? throw new InvalidOperationException("Failed to fetch bridge data");
```

* Retrieves a list of `BridgeInfo` objects, each containing the bridge’s name, associated chains, 24 h USD volume, and swap count.

### 2. Aggregate Total Flow & Swaps

```csharp
var totalFlowUsd = bridges.Sum(b => b.OneDayVolume);
var totalSwaps   = bridges.Sum(b => b.TxCountOneDay);
```

* **totalFlowUsd**: Sum of all bridges’ 24 h USD volume.
* **totalSwaps**: Sum of all bridges’ 24 h transaction counts.

### 3. Compute Per-Chain Flow

```csharp
var chainFlows = new Dictionary<string, double>();
foreach (var b in bridges)
{
    if (b.Chains?.Count > 0)
    {
        var perChain = b.OneDayVolume / b.Chains.Count;
        foreach (var chain in b.Chains)
        {
            if (!chainFlows.ContainsKey(chain))
                chainFlows[chain] = 0;
            chainFlows[chain] += perChain;
        }
    }
}
```

* Evenly splits each bridge’s volume across its supported chains, aggregating into a per-chain total.

### 4. Normalize & Score

```csharp
var flowNorm  = Math.Min(totalFlowUsd  / 1_000_000_000.0, 1.0);  // cap at $1 B
var swapsNorm = Math.Min(totalSwaps      /    100_000.0, 1.0);  // cap at 100 k txs
var rawScore  = (flowNorm + swapsNorm) / 2.0;                    // [0…1]
var score     = Math.Clamp(rawScore * 2 - 1, -1.0, 1.0);        // map to [–1…+1]
```

* **flowNorm**: 0…1 based on total USD flow relative to \$1 B.
* **swapsNorm**: 0…1 based on total swaps relative to 100 k.
* **rawScore**: average of the two normalizations.
* **score**: remapped to a signed confidence range \[–1…+1].

### 5. Prepare Metrics & Return

```csharp
var metrics = new Dictionary<string, object>
{
    ["TotalBridgeFlow24h_USD"]   = totalFlowUsd,
    ["TotalCrossChainSwaps24h"]  = totalSwaps,
    ["ConfidenceScore"]          = score
};

// Add top 5 chains by flow
foreach (var kv in chainFlows
                    .OrderByDescending(kv => kv.Value)
                    .Take(5))
{
    metrics[$"{kv.Key}_Flow24h_USD"] = Math.Round(kv.Value, 2);
}

return new OracleResult
{
    ModuleName      = Name,
    ConfidenceScore = score,
    Metrics         = metrics,
    Timestamp       = DateTime.UtcNow
};
```

* Records overall flow, swaps, and confidence.
* Reports the top five chains by 24 h flow.

---

## Supporting Types

```csharp
private class BridgeInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("chains")]
    public List<string>? Chains { get; set; }

    [JsonPropertyName("oneDayVolume")]
    public double OneDayVolume { get; set; }

    [JsonPropertyName("txCountOneDay")]
    public int TxCountOneDay { get; set; }
}
```

* **BridgeInfo**: Maps the JSON schema returned by the Llama bridges endpoint.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<ChainInteroperabilityMetricsOracle>()
       .AddSingleton<IAIOracle, ChainInteroperabilityMetricsOracle>();

// Later, to execute the oracle:
var oracle = serviceProvider.GetRequiredService<ChainInteroperabilityMetricsOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Chain Interop Confidence: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
