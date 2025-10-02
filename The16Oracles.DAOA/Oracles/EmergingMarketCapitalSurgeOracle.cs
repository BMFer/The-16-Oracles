using System.Text.Json.Serialization;
using The16Oracles.DAOA.Interfaces;
using The16Oracles.DAOA.Models;

namespace The16Oracles.DAOA.Oracles;

public class EmergingMarketCapitalSurgeOracle : IAIOracle
{
    private readonly HttpClient _client;
    public string Name => "Emerging Market Capital Surge";

    // Define your target emerging markets and their fiat codes:
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

    public EmergingMarketCapitalSurgeOracle(HttpClient client)
        => _client = client;

    public async Task<OracleResult> EvaluateAsync(DataBundle bundle)
    {
        // 1. Get BTC price in USD
        var btcPriceUsd = await FetchBtcPriceUsdAsync();

        // 2. Fetch all exchanges
        var exchanges = await FetchExchangesAsync();

        var metrics = new Dictionary<string, object>();
        var onRampRatios = new List<double>();

        // 3. Process each region
        foreach (var (region, currency) in _regions)
        {
            // filter exchanges by country
            var regionalExchanges = exchanges
                .Where(e => string.Equals(e.Country, region, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // sum total BTC volume and convert to USD
            var totalVolBtc = regionalExchanges.Sum(e => e.TradeVolume24hNormalizedBtc);
            var totalVolUsd = totalVolBtc * btcPriceUsd;

            // sum fiat-on-ramp volume in USD
            double onRampUsd = 0;
            foreach (var ex in regionalExchanges)
            {
                var tickers = await FetchExchangeTickersAsync(ex.Id);
                onRampUsd += tickers
                    .Where(t => string.Equals(t.Target, currency, StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.ConvertedVolume.Usd);
            }

            var onRampRatio = totalVolUsd > 0 ? onRampUsd / totalVolUsd : 0;
            onRampRatios.Add(onRampRatio);

            // record per-region metrics
            metrics[$"{region}_TotalVolumeUSD"] = totalVolUsd;
            metrics[$"{region}_FiatOnRampVolumeUSD"] = onRampUsd;
            metrics[$"{region}_OnRampRatio"] = onRampRatio;
        }

        // 4. Aggregate into a single score
        var avgOnRampRatio = onRampRatios.Any() ? onRampRatios.Average() : 0;
        // map [0…1]→[–1…+1]: 0→–1, 0.5→0, 1→+1
        var score = Math.Clamp(avgOnRampRatio * 2 - 1, -1, 1);

        return new OracleResult
        {
            ModuleName = Name,
            ConfidenceScore = score,
            Metrics = metrics,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<double> FetchBtcPriceUsdAsync()
    {
        var url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd";
        var resp = await _client.GetFromJsonAsync<Dictionary<string, Dictionary<string, double>>>(url)
                   ?? throw new InvalidOperationException("Failed to fetch BTC price");
        return resp["bitcoin"]["usd"];
    }

    private async Task<List<Exchange>> FetchExchangesAsync()
    {
        var url = "https://api.coingecko.com/api/v3/exchanges";
        return await _client.GetFromJsonAsync<List<Exchange>>(url)
               ?? throw new InvalidOperationException("Failed to fetch exchanges");
    }

    private async Task<List<Ticker>> FetchExchangeTickersAsync(string exchangeId)
    {
        var url = $"https://api.coingecko.com/api/v3/exchanges/{exchangeId}/tickers";
        var resp = await _client.GetFromJsonAsync<ExchangeTickersResponse>(url)
                   ?? throw new InvalidOperationException($"Failed to fetch tickers for {exchangeId}");
        return resp.Tickers;
    }

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
}

