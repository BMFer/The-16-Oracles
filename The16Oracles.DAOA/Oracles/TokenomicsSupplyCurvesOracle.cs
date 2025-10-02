using System.Numerics;
using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;
public class TokenomicsSupplyCurvesOracle : IAIOracle
{
    private readonly HttpClient _client;
    private readonly string _etherscanKey;
    private readonly List<TokenConfig> _tokens;

    public string Name => "Tokenomics & Supply Curves";

    public TokenomicsSupplyCurvesOracle(HttpClient client, IConfiguration config)
    {
        _client = client;
        _etherscanKey = config["Etherscan:ApiKey"]
                        ?? throw new InvalidOperationException("Missing Etherscan API key");
        _tokens = config
            .GetSection("Tokenomics:Tokens")
            .Get<List<TokenConfig>>()
          ?? throw new InvalidOperationException("Missing Tokenomics:Tokens config");
    }

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        var metrics = new Dictionary<string, object>();
        var rawIndices = new List<double>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var historicalDate = DateTime.UtcNow.AddDays(-30).ToString("dd-MM-yyyy");

        foreach (var token in _tokens)
        {
            // 1. Current supply from CoinGecko
            var coinUrl = $"https://api.coingecko.com/api/v3/coins/{token.Id}" +
                          $"?localization=false&tickers=false&market_data=true" +
                          $"&community_data=false&developer_data=false&sparkline=false";
            var coinResp = await _client.GetFromJsonAsync<CoinDataResponse>(coinUrl)
                          ?? throw new InvalidOperationException($"Failed to fetch coin data for {token.Id}");
            var currentTotal = coinResp.MarketData.TotalSupply;
            var currentCirc = coinResp.MarketData.CirculatingSupply;

            // 2. 30-day-ago supply via CoinGecko history 
            var histUrl = $"https://api.coingecko.com/api/v3/coins/{token.Id}/history" +
                           $"?date={historicalDate}&localization=false";
            var histResp = await _client.GetFromJsonAsync<CoinHistoryResponse>(histUrl)
                          ?? throw new InvalidOperationException($"Failed history lookup for {token.Id}");
            var histCirc = histResp.MarketData?.CirculatingSupply ?? currentCirc;

            var supplyGrowth = histCirc > 0
                                  ? (currentCirc - histCirc) / histCirc
                                  : 0.0;
            var supplyGrowthNorm = Math.Clamp(supplyGrowth, 0.0, 1.0);

            metrics[$"{token.Id}_CurrentTotalSupply"] = currentTotal;
            metrics[$"{token.Id}_CurrentCirculatingSupply"] = currentCirc;
            metrics[$"{token.Id}_SupplyGrowth30dPct"] = Math.Round(supplyGrowth * 100, 2);

            // 3. Vesting: fetch unvested balances via Etherscan 
            var vestRatios = new List<double>();
            foreach (var vest in token.VestingContracts)
            {
                var balUrl = $"https://api.etherscan.io/api" +
                             $"?module=account&action=tokenbalance" +
                             $"&contractaddress={token.ContractAddress}" +
                             $"&address={vest.Address}" +
                             $"&tag=latest&apikey={_etherscanKey}";
                var vestResp = await _client.GetFromJsonAsync<EtherscanBalanceResponse>(balUrl)
                              ?? throw new InvalidOperationException($"Failed vesting balance for {vest.Address}");
                var raw = BigInteger.Parse(vestResp.Result);
                var unvested = (double)raw / Math.Pow(10, token.Decimals);
                var ratio = currentTotal > 0
                               ? unvested / currentTotal
                               : 0.0;
                vestRatios.Add(ratio);

                metrics[$"{token.Id}_Unvested_{vest.Name}_Tokens"] = Math.Round(unvested, 4);
                metrics[$"{token.Id}_Unvested_{vest.Name}_Ratio"] = Math.Round(ratio, 4);
            }

            var avgVestRatio = vestRatios.Any()
                               ? vestRatios.Average()
                               : 0.0;
            var vestNorm = Math.Clamp(avgVestRatio, 0.0, 1.0);

            metrics[$"{token.Id}_AvgUnvestedRatio"] = Math.Round(avgVestRatio, 4);

            // 4. Raw index: high when growth & vesting are low
            var rawIndex = (1 - supplyGrowthNorm) * (1 - vestNorm);
            rawIndices.Add(rawIndex);
            metrics[$"{token.Id}_RawIndex"] = Math.Round(rawIndex, 4);
        }

        // 5. Aggregate and map to [–1, +1]
        var avgIndex = rawIndices.Any() ? rawIndices.Average() : 0.0;
        var score = Math.Clamp(avgIndex * 2 - 1, -1.0, 1.0);

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    // --- helper types ---

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
}

