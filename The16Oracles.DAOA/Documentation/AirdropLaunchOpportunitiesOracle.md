# Oracle documentation:

---

## AirdropLaunchOpportunitiesOracle

**Implements:** `IAIOracle`

An oracle that queries a GraphQL endpoint for upcoming and recent proposal events in configured “spaces” (e.g. DAO forums), computes an “opportunity index” (ratio of upcoming to recent proposals), and returns a confidence score and metrics.

---

### Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Constructor](#constructor)
4. [Public Properties](#public-properties)
5. [EvaluateAsync Method](#evaluateasync-method)

   * [GraphQL Query](#graphql-query)
   * [Opportunity Index Calculation](#opportunity-index-calculation)
   * [Result Payload](#result-payload)
6. [Supporting Types](#supporting-types)
7. [Example Usage](#example-usage)

---

## Prerequisites

* .NET 6.0 (or later)
* A registered `HttpClient` instance
* An `IConfiguration` source (e.g. `appsettings.json`)

NuGet packages:

* `System.Text.Json` (for JSON serialization attributes)
* `Microsoft.Extensions.Configuration.Abstractions`

---

## Configuration

The following configuration keys are required under your appsettings (or other configuration provider):

```jsonc
{
  "AirdropOpportunities": {
    // List of “space” identifiers to query
    "Spaces": [
      "myspecialdao.eth",
      "anotherdao.eth"
    ],

    // GraphQL endpoint URL
    "GraphQlEndpoint": "https://api.snapshot.org/graphql"
  }
}
```

* **Spaces** (`string[]`):
  A list of space IDs to filter proposals by.
* **GraphQlEndpoint** (`string`):
  URL of the GraphQL API to POST queries against.

Missing either section or value will throw an `InvalidOperationException` during instantiation.

---

## Constructor

```csharp
public AirdropLaunchOpportunitiesOracle(HttpClient client, IConfiguration config)
```

| Parameter               | Description                                                                                  |
| ----------------------- | -------------------------------------------------------------------------------------------- |
| `HttpClient client`     | Pre-configured `HttpClient` for sending GraphQL requests.                                    |
| `IConfiguration config` | Provides access to `AirdropOpportunities:Spaces` and `AirdropOpportunities:GraphQlEndpoint`. |

**Behaviors**

* Reads and caches the list of spaces.
* Reads and caches the GraphQL endpoint URL.
* Throws if either configuration value is missing.

---

## Public Properties

```csharp
public string Name => "Airdrop/Launch Opportunities";
```

* **Name**: A human-readable identifier for the module.

---

## EvaluateAsync Method

```csharp
public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
```

Fetches proposal counts and computes an opportunity index.

### 1. GraphQL Query

* **Time window**:

  * `now` = current UTC time in seconds
  * `ago` = 30 days before `now`

* **Query**:

  ```graphql
  query($spaces:[String!], $now:Int!, $ago:Int!) {
    upcoming: proposals(
      first: 20,
      where: { space_in: $spaces, start_gt: $now },
      orderBy: start, orderDirection: asc
    ) {
      id
    }
    recent: proposals(
      first: 100,
      where: { space_in: $spaces, end_gt: $ago, end_lt: $now }
    ) {
      id
    }
  }
  ```

* **Variables**:

  ```csharp
  new Dictionary<string, object> {
    ["spaces"] = _spaces,
    ["now"]    = now,
    ["ago"]    = ago
  }
  ```

* POSTs the request to `_endpoint` and deserializes into internal types.

* Throws `InvalidOperationException` if the HTTP or JSON response is invalid.

### 2. Opportunity Index Calculation

1. **Count**

   * `upcomingCount` = number of proposals with `start > now`
   * `recentCount`   = number of proposals with `end` in the last 30 days

2. **Raw Ratio**

   ```csharp
   double rawOpp = recentCount > 0
       ? (double)upcomingCount / recentCount
       : (upcomingCount > 0 ? 1.0 : 0.0);
   ```

3. **Clamped Index** (0.0?–?1.0)

   ```csharp
   double oppIndex = Math.Clamp(rawOpp, 0.0, 1.0);
   ```

4. **Round** to 4 decimal places for reporting.

### 3. Result Payload

Returns an `OracleResult` with:

| Field             | Value                            |
| ----------------- | -------------------------------- |
| `ModuleName`      | `"Airdrop/Launch Opportunities"` |
| `ConfidenceScore` | `oppIndex` (0.0 to 1.0)          |
| `Metrics`         | Dictionary with:                 |

* `"UpcomingProposals"` (int)
* `"RecentProposals (30d)"` (int)
* `"OpportunityIndex"` (double)
  \| `Timestamp`        | `DateTime.UtcNow`                                             |

---

## Supporting Types

```csharp
private class GraphqlRequest
{
    public string query { get; set; } = "";
    public Dictionary<string, object> variables { get; set; } = new();
}

private class GraphqlResponse
{
    [JsonPropertyName("data")]
    public GraphData Data { get; set; } = new();
}

private class GraphData
{
    [JsonPropertyName("upcoming")]
    public List<Proposal> Upcoming { get; set; } = new();

    [JsonPropertyName("recent")]
    public List<Proposal> Recent { get; set; } = new();
}

private class Proposal
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
}
```

* **GraphqlRequest**: wraps the query string and variables.
* **GraphqlResponse**: root for JSON response, holds `GraphData`.
* **GraphData**: contains the two lists of `Proposal`.
* **Proposal**: minimal model with `id` field.

---

## Example Usage

```csharp
// In Startup.cs or Program.cs
builder.Services
    .AddHttpClient<AirdropLaunchOpportunitiesOracle>(client => {
        // e.g. default headers, base address if desired
    })
    .AddSingleton<IAIOracle, AirdropLaunchOpportunitiesOracle>();

// Later, to evaluate:
var oracle = serviceProvider.GetRequiredService<AirdropLaunchOpportunitiesOracle>();
var result = await oracle.EvaluateAsync(myDataBundle);

Console.WriteLine($"Opportunity Index: {result.Metrics["OpportunityIndex"]}");
```

---

