using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;

public class AirdropLaunchOpportunitiesOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly List<string> _spaces;
    private readonly string _endpoint;

    public string Name => "Airdrop/Launch Opportunities";

    public AirdropLaunchOpportunitiesOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _spaces = config
            .GetSection("AirdropOpportunities:Spaces")
            .Get<List<string>>()
            ?? throw new InvalidOperationException("Missing Spaces list");
        _endpoint = config["AirdropOpportunities:GraphQlEndpoint"]
                    ?? throw new InvalidOperationException("Missing GraphQlEndpoint");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // Unix timestamps
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var ago = now - 30 * 86400;

        // GraphQL query: upcoming vs. recent (last 30d)
        var gqlRequest = new GraphqlRequest
        {
            query = @"
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
}",
            variables = new Dictionary<string, object>
            {
                ["spaces"] = _spaces,
                ["now"] = now,
                ["ago"] = ago
            }
        };

        var resp = await _client.PostAsJsonAsync(_endpoint, gqlRequest);
        var graph = await resp.Content.ReadFromJsonAsync<GraphqlResponse>()
                    ?? throw new InvalidOperationException("GraphQL response invalid");

        var upcomingCount = graph.Data.Upcoming.Count;
        var recentCount = graph.Data.Recent.Count;

        // Opportunity index = upcoming / recent (clamped to [0,1])
        double rawOpp = recentCount > 0
            ? (double)upcomingCount / recentCount
            : (upcomingCount > 0 ? 1.0 : 0.0);
        var oppIndex = Math.Clamp(rawOpp, 0.0, 1.0);

        var metrics = new Dictionary<string, object>
        {
            ["UpcomingProposals"] = upcomingCount,
            ["RecentProposals (30d)"] = recentCount,
            ["OpportunityIndex"] = Math.Round(oppIndex, 4)
        };

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = oppIndex,   // [+0…+1]
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

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
}