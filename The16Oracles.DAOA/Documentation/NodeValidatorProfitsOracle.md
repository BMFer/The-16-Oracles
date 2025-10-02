# Oracle documentation:

---

## NodeValidatorProfitsOracle

**Implements:** `IAIOracle`

An oracle that queries the Cosmos REST API for current network inflation and bonded-validator commission rates, computes the effective staking yield after commission, normalizes it, and returns a confidence score (–1.0 … +1.0) along with key metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Constants & Endpoints](#constants--endpoints)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Inflation Rate](#1-fetch-inflation-rate)
   * [2. Fetch Validators & Compute Commission](#2-fetch-validators--compute-commission)
   * [3. Compute Effective Yield](#3-compute-effective-yield)
   * [4. Normalize & Score](#4-normalize--score)
   * [5. Package Metrics & Return](#5-package-metrics--return)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* **.NET 6.0** or later
* A registered `HttpClient` in your DI container
* NuGet package:

  * `System.Net.Http.Json`

---

## Constants & Endpoints

```csharp
private const string InflationUrl  = "https://api.cosmos.network/cosmos/mint/v1beta1/inflation";
private const string ValidatorsUrl = "https://api.cosmos.network/cosmos/staking/v1beta1/validators?status=BOND_STATUS_BONDED&pagination.limit=1000";
```

* **InflationUrl**: Returns the current annual inflation rate (as a decimal string).
* **ValidatorsUrl**: Returns all bonded validators and their commission rates.

---

## Constructor

```csharp
public NodeValidatorProfitsOracle(HttpClient client)
    => _client = client;
```

* **client**: Used to call the Cosmos REST API.

---

## Public Properties

```csharp
public string Name => "Node/Validator Profits";
```

* **Name**: Human-readable identifier for the oracle module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs the following steps:

### 1. Fetch Inflation Rate

```csharp
var inflResp   = await _client.GetFromJsonAsync<InflationResponse>(InflationUrl)
                   ?? throw new InvalidOperationException("Failed to fetch inflation");
var inflation  = double.Parse(inflResp.Inflation);
```

* Retrieves the current network inflation (e.g. `"0.1234"` ? 12.34%).

### 2. Fetch Validators & Compute Commission

```csharp
var valResp         = await _client.GetFromJsonAsync<ValidatorsResponse>(ValidatorsUrl)
                          ?? throw new InvalidOperationException("Failed to fetch validators");
var rates           = valResp.Validators
                       .Select(v => double.Parse(v.Commission.CommissionRates.Rate))
                       .ToList();
var avgCommission   = rates.Any() ? rates.Average() : 0.0;
```

* Fetches all bonded validators.
* Parses each validator’s commission `Rate` (string) to `double`.
* Computes the average commission rate.

### 3. Compute Effective Yield

```csharp
var effectiveYield = inflation * (1 - avgCommission);
```

* The staking yield after deducting the average commission.

### 4. Normalize & Score

```csharp
var normYield = Math.Min(effectiveYield, 1.0);      // cap at 100%
var rawScore  = normYield * 2 - 1;                  // [0?1]?[–1?+1]
var score     = Math.Clamp(rawScore, -1.0, 1.0);
```

* **normYield**: Caps the effective yield to 100% (1.0).
* **rawScore**: Maps \[0 … 1] to \[–1 … +1].
* **score**: Clamped final confidence score.

### 5. Package Metrics & Return

```csharp
var metrics = new Dictionary<string, object>
{
    ["AnnualInflationRate"] = Math.Round(inflation, 6),
    ["AvgCommissionRate"]   = Math.Round(avgCommission, 6),
    ["EffectiveYield"]      = Math.Round(effectiveYield, 6)
};

return new OracleResult
{
    ModuleName      = Name,
    ConfidenceScore = score,
    Metrics         = metrics,
    Timestamp       = DateTime.UtcNow
};
```

* Returns key values for inflation, commission, and post-commission yield.

---

## Supporting Types

```csharp
private class InflationResponse
{
    [JsonPropertyName("inflation")]
    public string Inflation { get; set; } = "0";
}

private class ValidatorsResponse
{
    [JsonPropertyName("validators")]
    public List<Validator> Validators { get; set; } = new();
}

private class Validator
{
    [JsonPropertyName("commission")]
    public Commission Commission { get; set; } = new();
}

private class Commission
{
    [JsonPropertyName("commission_rates")]
    public CommissionRates CommissionRates { get; set; } = new();
}

private class CommissionRates
{
    [JsonPropertyName("rate")]
    public string Rate { get; set; } = "0";
}
```

* **InflationResponse**: Wraps the `"inflation"` field.
* **ValidatorsResponse**: Contains the array of bonded `Validator` objects.
* **Validator ? Commission ? CommissionRates**: Nested classes to extract the commission `Rate` string.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<NodeValidatorProfitsOracle>()
       .AddSingleton<IAIOracle, NodeValidatorProfitsOracle>();

// Later in your application
var oracle = serviceProvider.GetRequiredService<NodeValidatorProfitsOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Node Validator Profit Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
