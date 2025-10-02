# Oracle documentation:

---

## MacroEconomicTrendsOracle

**Implements:** `IAIOracle`

An oracle that retrieves the latest U.S. 10-year Treasury yield, Consumer Price Index (CPI), and GDP from the FRED API, applies a simple bullish/bearish heuristic (high GDP, low rates, low inflation), and returns a confidence score (–1.0 … +1.0) along with the raw metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [1. Fetch Latest Observations](#1-fetch-latest-observations)
   * [2. Compute Score](#2-compute-score)
   * [3. Package Result](#3-package-result)
6. [Private Helpers](#private-helpers)

   * [FetchLatestValueAsync](#fetchlatestvalueasync)
   * [ComputeScore](#computescore)
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

Add your Federal Reserve Economic Data (FRED) API key to configuration:

```jsonc
{
  "FRED": {
    "ApiKey": "YOUR_FRED_API_KEY_HERE"
  }
}
```

* **FRED\:ApiKey** (`string`): Required to authenticate requests to the FRED API. Missing this key will cause an exception on startup.

---

## Constructor

```csharp
public MacroEconomicTrendsOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                 |
| ----------------------- | ------------------------------------------- |
| `HttpClient client`     | Used to POST and GET HTTP requests to FRED. |
| `IConfiguration config` | Supplies the `FRED:ApiKey`.                 |

Throws `InvalidOperationException` if the API key is not present.

---

## Public Properties

```csharp
public string Name => "Macro Economics Trends";
```

* **Name**: The identifier reported in the resulting `OracleResult`.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

1. **Fetch Latest Observations**

   ```csharp
   var rate10Y = await FetchLatestValueAsync("DGS10");       // 10-year Treasury yield
   var cpi     = await FetchLatestValueAsync("CPIAUCSL");   // Consumer Price Index
   var gdp     = await FetchLatestValueAsync("GDP");        // Gross Domestic Product
   ```

2. **Compute Score**

   ```csharp
   var score = ComputeScore(rate10Y, cpi, gdp);
   ```

   * Encourages positive scores when GDP is strong relative to inflation and interest rates.

3. **Package Result**

   ```csharp
   var metrics = new Dictionary<string, object>
   {
     ["InterestRate10Y"] = rate10Y,
     ["CPI"]             = cpi,
     ["GDP"]             = gdp
   };

   return new OracleResult
   {
     ModuleName      = Name,
     ConfidenceScore = score,
     Metrics         = metrics,
     Timestamp       = DateTime.UtcNow
   };
   ```

---

## Private Helpers

### FetchLatestValueAsync

```csharp
private async Task<double> FetchLatestValueAsync(string seriesId)
```

* Constructs a FRED API URL for the given `seriesId`, requesting only the latest observation:

  ```
  https://api.stlouisfed.org/fred/series/observations
    ?series_id={seriesId}
    &api_key={_fredApiKey}
    &file_type=json
    &limit=1
    &sort_order=desc
  ```
* Deserializes into `FredResponse`, validates and parses the single observation’s `Value` as `double`.
* Throws `InvalidOperationException` if data is missing or non-numeric.

### ComputeScore

```csharp
private double ComputeScore(double rate, double cpi, double gdp)
```

1. **Inflation** (% change over a 100-point baseline)

   ```csharp
   var inflationPct = (cpi - 100) / 100.0;
   ```
2. **Normalize GDP** (scale by 10,000)

   ```csharp
   var gdpNorm = gdp / 10000.0;
   ```
3. **Normalize Rate** (scale by 100)

   ```csharp
   var rateNorm = rate / 100.0;
   ```
4. **Heuristic**

   ```csharp
   var raw = gdpNorm - rateNorm - inflationPct;
   return Math.Clamp(raw, -1.0, 1.0);
   ```

---

## Supporting Types

```csharp
private class FredResponse
{
    public Observation[] Observations { get; set; } = Array.Empty<Observation>();
}

private class Observation
{
    public string Date  { get; set; } = "";
    public string Value { get; set; } = "";
}
```

* **FredResponse**: Root object mapping the `"observations"` array.
* **Observation**: Represents a single date/value pair in the series.

---

## Example Usage

```csharp
// In Program.cs or Startup.cs
builder.Services
       .AddHttpClient<MacroEconomicTrendsOracle>()
       .AddSingleton<IAIOracle, MacroEconomicTrendsOracle>();

// Later, to run the oracle:
var oracle = serviceProvider.GetRequiredService<MacroEconomicTrendsOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Macro Trends Score: {result.ConfidenceScore}");
foreach (var kv in result.Metrics)
{
    Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

*End of documentation.*
