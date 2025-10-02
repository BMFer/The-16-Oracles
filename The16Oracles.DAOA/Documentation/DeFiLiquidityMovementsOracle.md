# Oracle documentation:

---

## DeFiLiquidityMovementsOracle

**Implements:** `IAIOracle`

Analyzes total value locked (TVL) shifts and DeFi lending composition over the past day, combining these into a liquidity?movement confidence score (–1.0 … +1.0) along with key metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Constructor](#constructor)
3. [Public Properties](#public-properties)
4. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch TVL History](#1-fetch-tvl-history)
   * [2. Compute TVL Shift Percentage](#2-compute-tvl-shift-percentage)
   * [3. Fetch Lending TVL](#3-fetch-lending-tvl)
   * [4. Compute Lending Ratio](#4-compute-lending-ratio)
   * [5. Score Calculation](#5-score-calculation)
   * [6. Assemble Result](#6-assemble-result)
5. [Private Helpers](#private-helpers)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** (or later)
* A registered `HttpClient` in your DI container
* NuGet packages:

  * `System.Net.Http.Json`
  * `Newtonsoft.Json`

*No external configuration required; all endpoints are hard-coded to Llama APIs.*

---

## Constructor

```csharp
public DeFiLiquidityMovementsOracle(HttpClient client)
```

| Parameter           | Description                                             |
| ------------------- | ------------------------------------------------------- |
| `HttpClient client` | Used to fetch TVL history and protocol data from Llama. |

---

## Public Properties

```csharp
public string Name => "DeFi Liquidity Movements";
```

* **Name**: Human-readable identifier for this module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Orchestrates four main calculation steps:

### 1. Fetch TVL History

```csharp
var (latestTvl, previousTvl) = await FetchTvlHistoryAsync();
```

Retrieves a time series of total TVL (USD) and returns the two most recent data points.

### 2. Compute TVL Shift Percentage

```csharp
var tvlShiftPct = previousTvl > 0
    ? (latestTvl - previousTvl) / previousTvl
    : 0.0;
```

Percentage change in TVL over the last day.

### 3. Fetch Lending TVL

```csharp
var lendingTvl = await FetchLendingTvlAsync();
```

Summed TVL of all protocols categorized as “Lending”.

### 4. Compute Lending Ratio

```csharp
var lendingRatio = latestTvl > 0
    ? lendingTvl / latestTvl
    : 0.0;
```

Share of total TVL that is locked in lending protocols.

### 5. Score Calculation

```csharp
var rawScore = tvlShiftPct * lendingRatio;
var score    = Math.Clamp(rawScore, -1.0, 1.0);
```

Positive when TVL is rising and concentrated in lending; negative when TVL falls.

### 6. Assemble Result

```csharp
var metrics = new Dictionary<string, object>
{
    ["TotalTVL_USD"]     = latestTvl,
    ["TVLShift1d_Pct"]   = tvlShiftPct,
    ["LendingTVL_USD"]   = lendingTvl,
    ["LendingRatio"]     = lendingRatio
};

return new OracleResult
{
    ModuleName      = Name,
    ConfidenceScore = score,
    Metrics         = metrics,
    Timestamp       = DateTime.UtcNow
};
```

* **ConfidenceScore**: Signed indicator of healthy (positive) vs. weakening (negative) DeFi liquidity.
* **Metrics**: Detailed values for further analysis.

---

## Private Helpers

### FetchTvlHistoryAsync

```csharp
private async Task<(double latest, double previous)> FetchTvlHistoryAsync()
```

1. GET `https://api.llama.fi/charts` ? `List<TvlPoint>`
2. Ensure ?2 data points, sort by `Date`, return the last two `TotalLiquidityUSD` values.

Throws if fetch fails or insufficient data.

### FetchLendingTvlAsync

```csharp
private async Task<double> FetchLendingTvlAsync()
```

1. GET `https://api.llama.fi/protocols` (raw JSON via `GetStringAsync`)
2. Deserialize to `List<Protocol>`
3. Sum `Tvl` for entries where `Category` equals `"Lending"` (case-insensitive)

Throws if fetch or parse fails.

---

## Supporting Types

```csharp
private class TvlPoint
{
    public long? Date { get; set; }
    public double TotalLiquidityUSD { get; set; }
}

private class Protocol
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public double? Tvl { get; set; }
}
```

* **TvlPoint**: Represents a timestamped TVL data point from Llama charts.
* **Protocol**: Represents a DeFi protocol’s metadata and TVL from Llama.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<DeFiLiquidityMovementsOracle>()
       .AddSingleton<IAIOracle, DeFiLiquidityMovementsOracle>();

// Elsewhere in your application
var oracle = serviceProvider.GetRequiredService<DeFiLiquidityMovementsOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"DeFi Liquidity Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

---

*End of documentation.*
