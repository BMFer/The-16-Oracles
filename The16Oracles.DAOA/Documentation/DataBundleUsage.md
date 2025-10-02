# DataBundle Documentation

---

```csharp
// a very basic DataBundle shape
public class DataBundle
{
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Payload { get; set; } = new();
}
```

---

### 1. C# client example

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

// ...

public async Task CallAirdropOracleAsync(HttpClient http)
{
    // 1) Build your DataBundle
    var bundle = new DataBundle {
        Timestamp = DateTime.UtcNow,
        Payload = new Dictionary<string, object> {
            // you can pass anything your oracle might need,
            // e.g. an address to filter by or some context flags
            ["walletAddress"] = "0xABCD...1234",
            ["includeTestnets"] = false
        }
    };

    // 2) POST to your oracle endpoint
    var response = await http.PostAsJsonAsync(
        "https://your-api.com/oracles/airdrop-opportunities",
        bundle
    );
    response.EnsureSuccessStatusCode();

    // 3) Read back the result
    var result = await response.Content.ReadFromJsonAsync<OracleResult>();
    Console.WriteLine($"Score: {result.ConfidenceScore}");
    foreach (var kv in result.Metrics)
        Console.WriteLine($"{kv.Key}: {kv.Value}");
}
```

---

### 2. `curl` example

```bash
curl -X POST https://your-api.com/oracles/airdrop-opportunities \
     -H "Content-Type: application/json" \
     -d '{
           "timestamp": "2025-06-10T15:00:00Z",
           "payload": {
             "walletAddress": "0xABCD...1234",
             "includeTestnets": false
           }
         }'
```

The response will be your usual `OracleResult` JSON:

```json
{
  "moduleName": "Airdrop/Launch Opportunities",
  "confidenceScore": 0.42,
  "metrics": {
    "UpcomingProposals": 7,
    "RecentProposals (30d)": 20,
    "OpportunityIndex": 0.35
  },
  "timestamp": "2025-06-10T15:00:01Z"
}
```

---

> **Note:** Most of the oracles ignore the contents of `DataBundle` for now—so you can safely pass an empty object `{}`.  But having a `Payload` dictionary lets you extend your oracles later (e.g. filter by address, date‐range, feature flags, etc.).
