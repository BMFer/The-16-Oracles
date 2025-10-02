# Oracle documentation:

---

## SecurityRugRiskDetectionOracle

**Implements:** `IAIOracle`

An oracle that scans a list of ERC-20 token contracts for “rug pull” risk by checking source-code verification on Etherscan and measuring token?holder concentration from Covalent. It combines unverified-contract fraction and top-holder concentration into a raw risk index (0…1), then maps that to a confidence score (–1.0 … 0.0).

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Load Token List & API Keys](#1-load-token-list--api-keys)
   * [2. Check Source-Code Verification](#2-check-source-code-verification)
   * [3. Measure Holder Concentration](#3-measure-holder-concentration)
   * [4. Compute Raw Risk & Score](#4-compute-raw-risk--score)
   * [5. Return OracleResult](#5-return-oracleresult)
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

Add the following to your configuration source (e.g. `appsettings.json`):

```jsonc
{
  "RugRisk": {
    // Array of ERC-20 token contract addresses to scan
    "TokenAddresses": [
      "0x1234...abcd",
      "0x5678...ef01",
      // ...
    ],

    // Etherscan API key for source-code lookup
    "Etherscan:ApiKey": "YOUR_ETHERSCAN_KEY",

    // Covalent API key for token-holder data
    "Covalent:ApiKey": "YOUR_COVALENT_KEY"
  }
}
```

| Key                        | Type       | Description                                     |
| -------------------------- | ---------- | ----------------------------------------------- |
| `RugRisk:TokenAddresses`   | `string[]` | List of token contract addresses to evaluate.   |
| `RugRisk:Etherscan:ApiKey` | `string`   | API key for Etherscan “getsourcecode” endpoint. |
| `RugRisk:Covalent:ApiKey`  | `string`   | API key for Covalent token-holders endpoint.    |

Missing an API key will throw an `InvalidOperationException` at runtime.

---

## Constructor

```csharp
public SecurityRugRiskDetectionOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                   |
| ----------------------- | ------------------------------------------------------------- |
| `HttpClient client`     | Used to call Etherscan and Covalent REST APIs.                |
| `IConfiguration config` | Supplies token list and API keys via `RugRisk` configuration. |

---

## Public Properties

```csharp
public string Name => "Security/Rug Risk Detection";
```

* **Name**: Identifier for this oracle in the returned `OracleResult`.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Performs the risk?detection workflow:

### 1. Load Token List & API Keys

```csharp
var tokens   = _config.GetSection("RugRisk:TokenAddresses")
                      .Get<List<string>>() ?? new List<string>();

var ethKey   = _config["RugRisk:Etherscan:ApiKey"]
                ?? throw new InvalidOperationException("Etherscan API key missing");
var covKey   = _config["RugRisk:Covalent:ApiKey"]
                ?? throw new InvalidOperationException("Covalent API key missing");
```

* Retrieves the list of token addresses (default empty).
* Throws if either API key is not provided.

### 2. Check Source-Code Verification

For each `address` in `tokens`:

```csharp
var srcResp = await _client.GetFromJsonAsync<EtherscanResponse>(
    $"https://api.etherscan.io/api?module=contract&action=getsourcecode&address={address}&apikey={ethKey}"
);

bool verified = srcResp?.Result?.FirstOrDefault()?.SourceCode?.Length > 0;
if (!verified) unverifiedCount++;
```

* Calls Etherscan’s `getsourcecode` endpoint.
* If `SourceCode` is empty or missing, increments `unverifiedCount`.

### 3. Measure Holder Concentration

```csharp
var covUrl = $"https://api.covalenthq.com/v1/1/tokens/{address}/token_holders/?key={covKey}&page-size=10";
var covResp = await _client.GetFromJsonAsync<CovalentResponse>(covUrl);
var holders = covResp?.Data?.Items ?? new List<Holder>();

// Sum share of top 2 holders (e.g. 0.12 for 12%)
var top2 = holders.Take(2).Sum(h => h.HolderShare);
concentrations.Add(top2);
```

* Fetches the top 10 token holders via Covalent.
* Records the combined share of the first two addresses.

### 4. Compute Raw Risk & Score

```csharp
var total            = tokens.Count;
var fracUnverified   = total > 0 ? (double)unverifiedCount / total : 0.0;
var avgConcentration = concentrations.Any()
    ? concentrations.Average()
    : 0.0;

// 50% weight each
var rawRisk = Math.Clamp(fracUnverified * 0.5 + avgConcentration * 0.5, 0.0, 1.0);

// Map [0…1] ? [–1…0]: higher raw risk ? more negative
var score = Math.Clamp(-rawRisk, -1.0, 0.0);
```

* **rawRisk**: half from unverified-contract fraction, half from top-2 holder concentration.
* **score**: inverted to the negative range for a risk-warning confidence score.

### 5. Return OracleResult

```csharp
var metrics = new Dictionary<string, object>
{
  ["TotalTokensScanned"]       = total,
  ["VerifiedContracts"]        = total - unverifiedCount,
  ["UnverifiedContracts"]      = unverifiedCount,
  ["AvgTop2HolderConcentration"] = Math.Round(avgConcentration, 4),
  ["RawRiskIndex"]             = Math.Round(rawRisk, 4)
};

return new OracleResult
{
  ModuleName      = Name,
  ConfidenceScore = score,
  Metrics         = metrics,
  Timestamp       = DateTime.UtcNow
};
```

* Returns the overall confidence score and a breakdown of verification and concentration metrics.

---

## Supporting Types

```csharp
private class EtherscanResponse
{
    [JsonPropertyName("result")]
    public List<ContractInfo>? Result { get; set; }
}

private class ContractInfo
{
    [JsonPropertyName("SourceCode")]
    public string? SourceCode { get; set; }
}

private class CovalentResponse
{
    [JsonPropertyName("data")]
    public CovalentData? Data { get; set; }
}

private class CovalentData
{
    [JsonPropertyName("items")]
    public List<Holder>? Items { get; set; }
}

private class Holder
{
    [JsonPropertyName("share")]
    public double HolderShare { get; set; }
}
```

* **EtherscanResponse ? ContractInfo**: Parses Etherscan’s JSON for `SourceCode`.
* **CovalentResponse ? CovalentData ? Holder**: Parses top-holder shares from Covalent.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<SecurityRugRiskDetectionOracle>()
       .AddSingleton<IAIOracle, SecurityRugRiskDetectionOracle>();

// Later, when invoking the oracle:
var oracle = serviceProvider.GetRequiredService<SecurityRugRiskDetectionOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Rug Risk Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
