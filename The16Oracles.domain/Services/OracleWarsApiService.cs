using System.Text;
using Newtonsoft.Json;
using The16Oracles.domain.Models;

namespace The16Oracles.domain.Services
{
    public interface IOracleWarsApiService
    {
        Task<GameResponse<Player>> CreatePlayerAsync(string discordUserId, string username);
        Task<GameResponse<Player>> GetPlayerAsync(string discordUserId);
        Task<GameResponse<Player>> SubscribeToOracleAsync(string discordUserId, string oracleName);
        Task<GameResponse<Player>> UnsubscribeFromOracleAsync(string discordUserId, string oracleName);
        Task<GameResponse<List<OracleDefinition>>> GetAvailableOraclesAsync();
        Task<GameResponse<Battle>> CreateBattleAsync(string challengerUserId, string opponentUserId, decimal wager);
        Task<GameResponse<Battle>> ExecuteBattleAsync(string battleId);
        Task<GameResponse<List<LeaderboardEntry>>> GetLeaderboardAsync(int limit = 10);
        Task<GameResponse<Player>> ClaimDailyBonusAsync(string discordUserId);
    }

    public class OracleWarsApiService : IOracleWarsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public OracleWarsApiService(string baseUrl = "https://localhost:5001")
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
        }

        public async Task<GameResponse<Player>> CreatePlayerAsync(string discordUserId, string username)
        {
            var request = new CreatePlayerRequest
            {
                DiscordUserId = discordUserId,
                Username = username
            };

            return await PostAsync<CreatePlayerRequest, Player>("/api/game/player/create", request);
        }

        public async Task<GameResponse<Player>> GetPlayerAsync(string discordUserId)
        {
            return await GetAsync<Player>($"/api/game/player/{discordUserId}");
        }

        public async Task<GameResponse<Player>> SubscribeToOracleAsync(string discordUserId, string oracleName)
        {
            var request = new SubscribeOracleRequest
            {
                DiscordUserId = discordUserId,
                OracleName = oracleName
            };

            return await PostAsync<SubscribeOracleRequest, Player>("/api/game/oracle/subscribe", request);
        }

        public async Task<GameResponse<Player>> UnsubscribeFromOracleAsync(string discordUserId, string oracleName)
        {
            var url = $"/api/game/oracle/unsubscribe?discordUserId={discordUserId}&oracleName={oracleName}";
            var response = await _httpClient.DeleteAsync($"{_baseUrl}{url}");
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GameResponse<Player>>(content)
                ?? new GameResponse<Player> { Success = false, Message = "Failed to parse response" };
        }

        public async Task<GameResponse<List<OracleDefinition>>> GetAvailableOraclesAsync()
        {
            return await GetAsync<List<OracleDefinition>>("/api/game/oracles");
        }

        public async Task<GameResponse<Battle>> CreateBattleAsync(string challengerUserId, string opponentUserId, decimal wager)
        {
            var request = new CreateBattleRequest
            {
                ChallengerUserId = challengerUserId,
                OpponentUserId = opponentUserId,
                Wager = wager
            };

            return await PostAsync<CreateBattleRequest, Battle>("/api/game/battle/create", request);
        }

        public async Task<GameResponse<Battle>> ExecuteBattleAsync(string battleId)
        {
            return await PostAsync<object, Battle>($"/api/game/battle/{battleId}/execute", new { });
        }

        public async Task<GameResponse<List<LeaderboardEntry>>> GetLeaderboardAsync(int limit = 10)
        {
            return await GetAsync<List<LeaderboardEntry>>($"/api/game/leaderboard?limit={limit}");
        }

        public async Task<GameResponse<Player>> ClaimDailyBonusAsync(string discordUserId)
        {
            return await PostAsync<object, Player>($"/api/game/daily-bonus/{discordUserId}", new { });
        }

        private async Task<GameResponse<TResponse>> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new GameResponse<TResponse>
                    {
                        Success = false,
                        Message = $"API request failed: {response.StatusCode}"
                    };
                }

                return JsonConvert.DeserializeObject<GameResponse<TResponse>>(content)
                    ?? new GameResponse<TResponse> { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new GameResponse<TResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        private async Task<GameResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new GameResponse<TResponse>
                    {
                        Success = false,
                        Message = $"API request failed: {response.StatusCode}"
                    };
                }

                return JsonConvert.DeserializeObject<GameResponse<TResponse>>(responseContent)
                    ?? new GameResponse<TResponse> { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                return new GameResponse<TResponse>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }
}
