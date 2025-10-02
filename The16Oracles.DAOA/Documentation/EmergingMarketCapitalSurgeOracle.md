# Oracle documentation:

---

## EmergingMarketCapitalSurgeOracle

**Implements:** `IAIOracle`

An oracle that measures Bitcoin on-ramp activity in key emerging markets by comparing each region’s fiat on-ramp volume to its total BTC trading volume, aggregates these ratios, and returns a confidence score (–1 … +1) with per-region metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Fields & Configuration](#fields--configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch BTC Price](#1-fetch-btc-price)
   * [2. Fetch Exchanges List](#2-fetch-exchanges-list)
   * [3. Per-Region Processing](#3-per-region-processing)

     * [a. Filter Regional Exchanges](#a-filter-regional-exchanges)
     * [b. Compute Total Volume USD](#b-compute-total-volume-usd)
     * [c. Compute Fiat On-Ramp Volume](#c-compute-fiat-on-ramp-volume)
     * [d. Compute On-Ramp Ratio](#d-compute-on-ramp-ratio)
   * [4. Aggregate Score](#4-aggregate-score)
   * [5. Return Result](#5-return-result)
6. [Private Helpers](#private-helpers)
7. [Supporting Types](#supporting-types)
8. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** (or later)
* Registered `HttpClient` in dependency-injection
* NuGet package:

  * `System.Net.Http.Json`

*No additional configuration entries are required; regions and currencies are hard-coded.*

---

## Fields & Configuration

```csharp
private readonly HttpClient _client;

// List of target emerging markets and their fiat currency codes
private readonly List<(string Region, string Currency)> _regions = new()
{
    ("India", "INR"),
    ("Brazil", "BRL"),
    ("Russia", "RUB"),
    ("South Africa", "ZAR"),
    ("Mexico", "MXN"),
    ("Indonesia", "IDR"),
    ("Turkey", "TRY"),
    ("Argentina", "ARS"),
    ("Nigeria", "NGN"),
    ("Thailand", "THB")
};
```

* **\_regions**: Each tuple defines a region name and its on-ramp currency.

---

## Constructor

```csharp
public EmergingMarketCapitalSurgeOracle(HttpClient client)
    => _client = client;
```

* **client**: used for all Coingecko API calls.

---

## Public Properties

```csharp
public string Name => "Emerging Market Capital Surge";
```

* **Name**: Descriptive identifier for this oracle.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Orchestrates the workflow in five steps:

### 1. Fetch BTC Price

```csharp
var btcPriceUsd = await FetchBtcPriceUsdAsync();
```

* Retrieves the current BTC?USD rate from Coingecko.

### 2. Fetch Exchanges List

```csharp
var exchanges = await FetchExchangesAsync();
```

* Gets all supported exchanges, including their `Country` and normalized 24h BTC volume.

### 3. Per-Region Processing

Iterates `_regions` to compute per-region metrics:

#### a. Filter Regional Exchanges

```csharp
var regionalExchanges = exchanges
    .Where(e => e.Country.Equals(region, StringComparison.OrdinalIgnoreCase))
    .ToList();
```

#### b. Compute Total Volume USD

```csharp
var totalVolBtc = regionalExchanges.Sum(e => e.TradeVolume24hNormalizedBtc);
var totalVolUsd = totalVolBtc * btcPriceUsd;
```

* **totalVolUsd**: sum of 24h BTC volume × BTC price.

#### c. Compute Fiat On-Ramp Volume

```csharp
double onRampUsd = 0;
foreach (var ex in regionalExchanges)
{
    var tickers = await FetchExchangeTickersAsync(ex.Id);
    onRampUsd += tickers
        .Where(t => t.Target.Equals(currency, StringComparison.OrdinalIgnoreCase))
        .Sum(t => t.ConvertedVolume.Usd);
}
```

* Sums all ticker volumes where `target` currency matches region.

#### d. Compute On-Ramp Ratio

```csharp
var onRampRatio = totalVolUsd > 0 
    ? onRampUsd / totalVolUsd 
    : 0;
metrics[$"{region}_TotalVolumeUSD"]     = totalVolUsd;
metrics[$"{region}_FiatOnRampVolumeUSD"] = onRampUsd;
metrics[$"{region}_OnRampRatio"]         = onRampRatio;
onRampRatios.Add(onRampRatio);
```

* Ratio of fiat on-ramp volume to overall trading volume.

### 4. Aggregate Score

```csharp
var avgOnRampRatio = onRampRatios.Any() 
    ? onRampRatios.Average()
    : 0;
var score = Math.Clamp(avgOnRampRatio * 2 - 1, -1, 1);
```

* Maps the average ratio \[0…1] linearly into \[–1…+1].

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

* Includes per-region metrics and the overall confidence score.

---

## Private Helpers

### FetchBtcPriceUsdAsync

```csharp
private async Task<double> FetchBtcPriceUsdAsync()
```

* GET `https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd`
* Returns the USD price of BTC.

### FetchExchangesAsync

```csharp
private async Task<List<Exchange>> FetchExchangesAsync()
```

* GET `https://api.coingecko.com/api/v3/exchanges`
* Returns list of exchanges with normalized BTC volume and country.

### FetchExchangeTickersAsync

```csharp
private async Task<List<Ticker>> FetchExchangeTickersAsync(string exchangeId)
```

* GET `https://api.coingecko.com/api/v3/exchanges/{exchangeId}/tickers`
* Returns the exchange’s tickers, including per-pair volumes.

---

## Supporting Types

```csharp
private class Exchange
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("country")]
    public string Country { get; set; } = "";

    [JsonPropertyName("trade_volume_24h_btc_normalized")]
    public double TradeVolume24hNormalizedBtc { get; set; }
}

private class ExchangeTickersResponse
{
    [JsonPropertyName("tickers")]
    public List<Ticker> Tickers { get; set; } = new();
}

private class Ticker
{
    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    [JsonPropertyName("converted_volume")]
    public ConvertedVolume ConvertedVolume { get; set; } = new();
}

private class ConvertedVolume
{
    [JsonPropertyName("usd")]
    public double Usd { get; set; }
}
```

* **Exchange**: Coingecko’s exchange metadata.
* **Ticker & ConvertedVolume**: Pair volumes in USD for each exchange.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<EmergingMarketCapitalSurgeOracle>()
       .AddSingleton<IAIOracle, EmergingMarketCapitalSurgeOracle>();

// Elsewhere:
var oracle = serviceProvider.GetRequiredService<EmergingMarketCapitalSurgeOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Emerging Market On-Ramp Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
```

---

*End of documentation.*
