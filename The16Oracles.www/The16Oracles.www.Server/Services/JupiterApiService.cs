using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using The16Oracles.www.Server.Models;

namespace The16Oracles.www.Server.Services;

public class JupiterApiService : IJupiterApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JupiterApiService> _logger;
    private readonly TradingConfiguration _config;

    public JupiterApiService(
        HttpClient httpClient,
        IOptions<TradeBotConfiguration> config,
        ILogger<JupiterApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value.Trading;
        _httpClient.BaseAddress = new Uri(_config.JupiterApiUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
    }

    public async Task<JupiterQuoteResponse> GetQuoteAsync(
        JupiterQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = $"?inputMint={request.InputMint}" +
                            $"&outputMint={request.OutputMint}" +
                            $"&amount={request.Amount}" +
                            $"&slippageBps={request.SlippageBps}";

            _logger.LogInformation("Requesting Jupiter quote: {Query}", queryParams);

            var response = await _httpClient.GetAsync($"/quote{queryParams}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var quote = JsonSerializer.Deserialize<JupiterQuoteResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (quote == null)
            {
                throw new InvalidOperationException("Failed to deserialize Jupiter quote response");
            }

            _logger.LogInformation(
                "Jupiter quote received: Input={Input}, Output={Output}, Price Impact={PriceImpact}%",
                quote.InAmount,
                quote.OutAmount,
                quote.PriceImpactPct);

            return quote;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while requesting Jupiter quote");
            throw new InvalidOperationException("Failed to get quote from Jupiter", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting Jupiter quote");
            throw;
        }
    }

    public async Task<JupiterSwapResponse> GetSwapTransactionAsync(
        JupiterSwapRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Requesting swap transaction from Jupiter");

            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/swap", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var swapResponse = JsonSerializer.Deserialize<JupiterSwapResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (swapResponse == null || string.IsNullOrEmpty(swapResponse.SwapTransaction))
            {
                throw new InvalidOperationException("Invalid swap transaction response from Jupiter");
            }

            _logger.LogInformation("Swap transaction received from Jupiter");

            return swapResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while requesting swap transaction");
            throw new InvalidOperationException("Failed to get swap transaction from Jupiter", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting swap transaction");
            throw;
        }
    }
}
