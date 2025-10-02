# Oracle documentation:

---

## TokenomicsSupplyCurvesOracle

**Implements:** `IAIOracle`

An oracle that analyzes token supply dynamics over the past 30 days—comparing current vs. historical circulating supply growth—and examines vesting contract balances to compute a raw “supply?health” index for each token. It aggregates these indices into a final confidence score (–1.0 … +1.0) with detailed per?token metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Load Token Config & API Key](#1-load-token-config--api-key)
   * [2. Fetch Current Supply](#2-fetch-current-supply)
   * [3. Fetch 30-Day-Ago Circulating Supply](#3-fetch-30-day-ago-circulating-supply)
   * [4. Compute Supply Growth](#4-compute-supply-growth)
   * [5. Fetch Vesting Balances](#5-fetch-vesting-balances)
   * [6. Build Raw Index](#6-build-raw-index)
   * [7. Aggregate & Score](#7-aggregate--score)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* Registered `HttpClient` in your DI container
* NuGet package:

  * `System.Net.Http.Json`

---

## Configuration

Add the following section to your configuration source (e.g. `appsettings.json`):

```jsonc
{
  "Etherscan": {
    "ApiKey": "YOUR_ETHERSCAN_API_KEY"
  },
  "Tokenomics": {
    "Tokens": [
      {
        "Id": "token-slug-on-coingecko",
        "ContractAddress": "0x1234…abcd",
        "Decimals": 18,
        "VestingContracts": [
          {
            "Address": "0xvest1…",
            "Name": "TeamVesting",
            "EndTimestamp": 1700000000
          },
          {
            "Address": "0xvest2…",
            "Name": "InvestorVesting",
            "EndTimestamp": 1710000000
          }
        ]
      },
      {
        "Id": "another-token",
        "ContractAddress": "0xabcd…1234",
        "Decimals": 6,
        "VestingContracts": []
      }
    ]
  }
}
```

| Key                  | Type              | Description                                      |
| -------------------- | ----------------- | ------------------------------------------------ |
| `Etherscan:ApiKey`   | `string`          | API key for Etherscan’s `tokenbalance` endpoint. |
| `Tokenomics:Tokens`  | `TokenConfig[]`   | List of token configurations to evaluate.        |
|   `Id`               | `string`          | CoinGecko token identifier.                      |
|   `ContractAddress`  | `string`          | ERC-20 contract address.                         |
|   `Decimals`         | `int`             | Token decimal precision.                         |
|   `VestingContracts` | `VestingConfig[]` | List of vesting contract details.                |
|     `Address`        | `string`          | Vesting contract address.                        |
|     `Name`           | `string`          | Friendly label for the vesting pool.             |
|     `EndTimestamp`   | `long`            | Unix timestamp when vesting ends.                |

---

## Constructor

```csharp
public TokenomicsSupplyCurvesOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                           |
| ----------------------- | --------------------------------------------------------------------- |
| `HttpClient client`     | Used for all HTTP requests (CoinGecko and Etherscan).                 |
| `IConfiguration config` | Supplies the Etherscan API key and the list of `TokenConfig` entries. |

Throws `InvalidOperationException` if either the API key or token list is missing.

---

## Public Properties

```csharp
public string Name => "Tokenomics & Supply Curves";
```

* **Name**: Identifier for this oracle module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs the following steps:

### 1. Load Token Config & API Key

```csharp
_etherscanKey = config["Etherscan:ApiKey"];
_tokens       = config.GetSection("Tokenomics:Tokens").Get<List<TokenConfig>>();
```

Throws if the Etherscan key or token list is not provided.

### 2. Fetch Current Supply

```csharp
var coinResp = await _client.GetFromJsonAsync<CoinDataResponse>(coinUrl);
var currentTotal = coinResp.MarketData.TotalSupply;
var currentCirc  = coinResp.MarketData.CirculatingSupply;
```

* GET `https://api.coingecko.com/api/v3/coins/{Id}`
* Extracts `total_supply` and `circulating_supply`.

### 3. Fetch 30-Day-Ago Circulating Supply

```csharp
var histResp = await _client.GetFromJsonAsync<CoinHistoryResponse>(histUrl);
var histCirc = histResp.MarketData?.CirculatingSupply ?? currentCirc;
```

* GET `https://api.coingecko.com/api/v3/coins/{Id}/history?date={dd-MM-yyyy}`
* Falls back to current circulating supply if history is missing.

### 4. Compute Supply Growth

```csharp
var supplyGrowth = histCirc > 0
    ? (currentCirc - histCirc) / histCirc
    : 0.0;
var supplyGrowthNorm = Math.Clamp(supplyGrowth, 0.0, 1.0);

metrics[$"{Id}_SupplyGrowth30dPct"] = Math.Round(supplyGrowth * 100, 2);
```

* Computes 30-day circulation growth and clamps to \[0 … 1].

### 5. Fetch Vesting Balances

```csharp
var vestRatios = new List<double>();
foreach (var vest in token.VestingContracts)
{
    var vestResp = await _client.GetFromJsonAsync<EtherscanBalanceResponse>(vestUrl);
    var raw       = BigInteger.Parse(vestResp.Result);
    var unvested  = (double)raw / Math.Pow(10, token.Decimals);
    var ratio     = currentTotal > 0 ? unvested / currentTotal : 0.0;
    vestRatios.Add(ratio);

    metrics[$"{Id}_Unvested_{vest.Name}_Tokens"] = Math.Round(unvested, 4);
    metrics[$"{Id}_Unvested_{vest.Name}_Ratio"]  = Math.Round(ratio, 4);
}

var avgVestRatio = vestRatios.Any() ? vestRatios.Average() : 0.0;
var vestNorm     = Math.Clamp(avgVestRatio, 0.0, 1.0);
metrics[$"{Id}_AvgUnvestedRatio"] = Math.Round(avgVestRatio, 4);
```

* Calls Etherscan’s `account&action=tokenbalance` for each vesting contract.
* Converts raw balance to human units, computes unvested?to?total ratio.

### 6. Build Raw Index

```csharp
var rawIndex = (1 - supplyGrowthNorm) * (1 - vestNorm);
rawIndices.Add(rawIndex);
metrics[$"{Id}_RawIndex"] = Math.Round(rawIndex, 4);
```

* Rewards low supply growth and low vesting concentration.

### 7. Aggregate & Score

```csharp
var avgIndex = rawIndices.Any() ? rawIndices.Average() : 0.0;
var score    = Math.Clamp(avgIndex * 2 - 1, -1.0, 1.0);
```

* Averages per-token raw indices, maps \[0…1] to \[–1…+1] confidence.

---

## Supporting Types

```csharp
private class CoinDataResponse
{
    [JsonPropertyName("market_data")]
    public MarketData MarketData { get; set; } = default!;
}
private class MarketData
{
    [JsonPropertyName("total_supply")]
    public double TotalSupply { get; set; }

    [JsonPropertyName("circulating_supply")]
    public double CirculatingSupply { get; set; }
}
private class CoinHistoryResponse
{
    [JsonPropertyName("market_data")]
    public MarketData? MarketData { get; set; }
}
private class EtherscanBalanceResponse
{
    [JsonPropertyName("result")]
    public string Result { get; set; } = "0";
}
private class TokenConfig
{
    public string Id { get; set; } = "";
    public string ContractAddress { get; set; } = "";
    public int Decimals { get; set; }
    public List<VestingConfig> VestingContracts { get; set; } = new();
}
private class VestingConfig
{
    public string Address { get; set; } = "";
    public string Name { get; set; } = "";
    public long EndTimestamp { get; set; }
}
```

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<TokenomicsSupplyCurvesOracle>()
       .AddSingleton<IAIOracle, TokenomicsSupplyCurvesOracle>();

// When running:
var oracle = serviceProvider.GetRequiredService<TokenomicsSupplyCurvesOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Tokenomics Confidence: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
